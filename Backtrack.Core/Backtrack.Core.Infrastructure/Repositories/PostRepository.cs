using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pgvector;
using System.Data;

namespace Backtrack.Core.Infrastructure.Repositories;

public class PostRepository(ApplicationDbContext context) : CrudRepositoryBase<Post, Guid>(context), IPostRepository
{
    private static (string Sql, List<NpgsqlParameter> Parameters) BuildFilters(PostFilters? filters)
    {
        var clauses    = new List<string>();
        var parameters = new List<NpgsqlParameter>();

        if (filters?.PostType != null)
        {
            clauses.Add("AND post_type = @postType");
            parameters.Add(new("@postType", filters.PostType.Value.ToString()));
        }
        if (filters?.Category != null)
        {
            clauses.Add("AND item_category = @category");
            parameters.Add(new("@category", filters.Category.Value.ToString()));
        }
        if (filters?.Geo != null)
        {
            clauses.Add(@"AND ST_DWithin(location::geography,
                ST_SetSRID(ST_MakePoint(@longitude, @latitude), 4326)::geography,
                @radius)");
            parameters.Add(new("@longitude", filters.Geo.Location.Longitude));
            parameters.Add(new("@latitude",  filters.Geo.Location.Latitude));
            parameters.Add(new("@radius",    filters.Geo.RadiusInKm * 1000));
        }
        if (filters?.Time?.From != null)
        {
            clauses.Add("AND event_time >= @fromTime");
            parameters.Add(new("@fromTime", filters.Time.From.Value));
        }
        if (filters?.Time?.To != null)
        {
            clauses.Add("AND event_time <= @toTime");
            parameters.Add(new("@toTime", filters.Time.To.Value));
        }
        if (filters?.Status != null)
        {
            clauses.Add("AND status = @status");
            parameters.Add(new("@status", filters.Status.Value.ToString()));
        }
        if (filters?.AuthorId != null)
        {
            clauses.Add("AND author_id = @authorId");
            parameters.Add(new("@authorId", filters.AuthorId));
        }
        if (filters?.OrganizationId != null)
        {
            clauses.Add("AND organization_id = @organizationId");
            parameters.Add(new("@organizationId", filters.OrganizationId.Value));
        }

        return (string.Join("\n                ", clauses), parameters);
    }

    public override async Task<Post?> GetByIdAsync(Guid id, bool isTrack = false)
    {
        IQueryable<Post> query = _dbSet.Include(p => p.Author).Include(p => p.Organization);
        if (!isTrack)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(p =>
            p.Id == id &&
            p.DeletedAt == null);
    }

    public async Task<(IEnumerable<Post> Items, int TotalCount)> GetPagedAsync(
        PagedQuery pagedQuery,
        PostFilters? filters = null,
        CancellationToken cancellationToken = default)
    {
        var (filterSql, _) = BuildFilters(filters);

        var countSql = $@"
            SELECT COUNT(*)
            FROM posts
            WHERE deleted_at IS NULL
                {filterSql}";

        var dataSql = $@"
            SELECT id
            FROM posts
            WHERE deleted_at IS NULL
                {filterSql}
            ORDER BY created_at DESC
            LIMIT @limit OFFSET @offset";

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        int totalCount;
        await using (var countCmd = conn.CreateCommand())
        {
            countCmd.CommandText = countSql;
            var (_, countParams) = BuildFilters(filters);
            countCmd.Parameters.AddRange(countParams.ToArray());
            totalCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync(cancellationToken));
        }

        var ids = new List<Guid>();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = dataSql;
            var (_, dataParams) = BuildFilters(filters);
            dataParams.Add(new("@limit",  pagedQuery.Limit));
            dataParams.Add(new("@offset", pagedQuery.Offset));
            cmd.Parameters.AddRange(dataParams.ToArray());

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                ids.Add(reader.GetGuid(0));
        }

        if (ids.Count == 0)
            return ([], totalCount);

        var posts = await _dbSet
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.Organization)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var postMap = posts.ToDictionary(p => p.Id);
        var ordered = ids.Where(id => postMap.ContainsKey(id)).Select(id => postMap[id]);

        return (ordered, totalCount);
    }

    public async Task<IEnumerable<(Post Post, double SimilarityScore)>> SearchBySemanticAsync(
        float[] queryEmbedding,
        PostFilters? filters = null,
        CancellationToken cancellationToken = default)
    {
        var (filterSql, filterParams) = BuildFilters(filters);
        var embeddingArrayLiteral = "[" + string.Join(",", queryEmbedding.Select(f => f.ToString(System.Globalization.CultureInfo.InvariantCulture))) + "]";

        var sql = $@"
            SELECT id, (1.0 - (embedding <=> @queryEmbedding::vector)) AS similarity
            FROM posts
            WHERE deleted_at IS NULL
                AND embedding_status = 'Ready'
                AND embedding IS NOT NULL
                {filterSql}
                AND (1.0 - (embedding <=> @queryEmbedding::vector)) >= @minSimilarity
            ORDER BY similarity DESC";

        var parameters = new List<NpgsqlParameter>(filterParams)
        {
            new("@queryEmbedding", embeddingArrayLiteral),
            new("@minSimilarity", PostSimilarityThresholds.LowSimilarityThreshold),
        };

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        var rankedIds = new List<(Guid Id, double Similarity)>();
        await using (var command = conn.CreateCommand())
        {
            command.CommandText = sql;
            command.Parameters.AddRange(parameters.ToArray());

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                rankedIds.Add((reader.GetGuid(0), reader.GetDouble(1)));
        }

        if (rankedIds.Count == 0)
            return [];

        var ids = rankedIds.Select(r => r.Id).ToList();
        var posts = await _dbSet
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.Organization)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var postMap = posts.ToDictionary(p => p.Id);
        return rankedIds
            .Where(r => postMap.ContainsKey(r.Id))
            .Select(r => (postMap[r.Id], r.Similarity));
    }

    public async Task<IEnumerable<(Post Post, double SimilarityScore)>> GetSimilarPostsAsync(
        Post post,
        CancellationToken cancellationToken = default)
    {
        if (post.Status != PostStatus.Active || post.EmbeddingStatus != EmbeddingStatus.Ready || post.Embedding == null)
            return [];
        const string sql = """
            SELECT id, (1.0 - (embedding <=> @embedding)) AS similarity
            FROM posts
            WHERE deleted_at IS NULL
                AND id != @postId
                AND embedding_status = 'Ready'
                AND embedding IS NOT NULL
                AND status = 'Active'
                AND location IS NOT NULL
                AND post_type != @postType
                AND author_id != @authorId
                AND ST_DWithin(
                    location::geography,
                    ST_SetSRID(ST_MakePoint(@longitude, @latitude), 4326)::geography,
                    @maxDistance
                )
                AND ABS(EXTRACT(EPOCH FROM (event_time - @eventTime)) / 86400) <= @timeWindowDays
                AND (1.0 - (embedding <=> @embedding)) >= @minSimilarity
            ORDER BY similarity DESC
            """;

        var embeddingVec = new Vector(post.Embedding ?? Array.Empty<float>());
        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        var rankedIds = new List<(Guid Id, double Similarity)>();
        await using (var command = conn.CreateCommand())
        {
            command.CommandText = sql;
            command.Parameters.Add(new NpgsqlParameter("@postId",         post.Id));
            command.Parameters.Add(new NpgsqlParameter("@embedding",      embeddingVec));
            command.Parameters.Add(new NpgsqlParameter("@postType",       post.PostType.ToString()));
            command.Parameters.Add(new NpgsqlParameter("@authorId",       post.AuthorId));
            command.Parameters.Add(new NpgsqlParameter("@longitude",      post.Location?.Longitude ?? 0));
            command.Parameters.Add(new NpgsqlParameter("@latitude",       post.Location?.Latitude  ?? 0));
            command.Parameters.Add(new NpgsqlParameter("@maxDistance",    PostSimilarityThresholds.MaxDistanceMeters));
            command.Parameters.Add(new NpgsqlParameter("@eventTime",      post.EventTime));
            command.Parameters.Add(new NpgsqlParameter("@timeWindowDays", PostSimilarityThresholds.TimeWindowDays));
            command.Parameters.Add(new NpgsqlParameter("@minSimilarity",  PostSimilarityThresholds.MediumSimilarityThreshold));

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                rankedIds.Add((reader.GetGuid(0), reader.GetDouble(1)));
        }

        if (rankedIds.Count == 0)
            return [];

        var ids   = rankedIds.Select(r => r.Id).ToList();
        var posts = await _dbSet
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.Organization)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var postMap = posts.ToDictionary(p => p.Id);
        return rankedIds
            .Where(r => postMap.ContainsKey(r.Id))
            .Select(r => (postMap[r.Id], r.Similarity));
    }

    public async Task<IEnumerable<Post>> SearchByFullTextAsync(
        string searchTerm,
        PostFilters? filters = null,
        CancellationToken cancellationToken = default)
    {
        var (filterSql, parameters) = BuildFilters(filters);
        parameters.Insert(0, new("@searchTerm", searchTerm));

        var dataSql = $@"
            SELECT id
            FROM posts
            WHERE deleted_at IS NULL
                AND item_search @@ websearch_to_tsquery('english', @searchTerm)
                {filterSql}
            ORDER BY ts_rank(item_search, websearch_to_tsquery('english', @searchTerm)) DESC";

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        var rankedIds = new List<Guid>();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = dataSql;
            cmd.Parameters.AddRange(parameters.ToArray());

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                rankedIds.Add(reader.GetGuid(0));
        }

        if (rankedIds.Count == 0)
            return [];

        var posts = await _dbSet
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.Organization)
            .Where(p => rankedIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var postMap = posts.ToDictionary(p => p.Id);
        return rankedIds.Where(id => postMap.ContainsKey(id)).Select(id => postMap[id]);
    }

}
