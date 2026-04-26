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
        var clauses = new List<string>();
        var parameters = new List<NpgsqlParameter>();

        if (filters?.PostType != null)
        {
            clauses.Add("AND post_type = @postType");
            parameters.Add(new("@postType", filters.PostType.Value.ToString()));
        }
        if (filters?.Category != null)
        {
            clauses.Add("AND category = @category");
            parameters.Add(new("@category", filters.Category.Value.ToString()));
        }
        if (filters?.Geo != null)
        {
            clauses.Add(@"AND ST_DWithin(location::geography,
                ST_SetSRID(ST_MakePoint(@longitude, @latitude), 4326)::geography,
                @radius)");
            parameters.Add(new("@longitude", filters.Geo.Location.Longitude));
            parameters.Add(new("@latitude", filters.Geo.Location.Latitude));
            parameters.Add(new("@radius", filters.Geo.RadiusInKm * 1000));
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
        if (filters?.SubcategoryCode != null)
        {
            clauses.Add("AND subcategory_id = (SELECT id FROM subcategories WHERE code = @subcategoryCode AND deleted_at IS NULL LIMIT 1)");
            parameters.Add(new("@subcategoryCode", filters.SubcategoryCode));
        }
        if (filters?.SubcategoryId != null)
        {
            clauses.Add("AND subcategory_id = @subcategoryId");
            parameters.Add(new("@subcategoryId", filters.SubcategoryId.Value));
        }

        return (string.Join("\n                ", clauses), parameters);
    }

    public override async Task<Post?> GetByIdAsync(Guid id, bool isTrack = false)
    {
        IQueryable<Post> query = _dbSet
            .Include(p => p.Author)
            .Include(p => p.Organization)
            .Include(p => p.CardDetail)
            .Include(p => p.PersonalBelongingDetail)
            .Include(p => p.ElectronicDetail)
            .Include(p => p.OtherDetail);
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
            dataParams.Add(new("@limit", pagedQuery.Limit));
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
            .Include(p => p.CardDetail)
            .Include(p => p.PersonalBelongingDetail)
            .Include(p => p.ElectronicDetail)
            .Include(p => p.OtherDetail)
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
            .Include(p => p.CardDetail)
            .Include(p => p.PersonalBelongingDetail)
            .Include(p => p.ElectronicDetail)
            .Include(p => p.OtherDetail)
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

        var window = TimeSpan.FromDays(PostSimilarityThresholds.TimeWindowDays);
        var filters = new PostFilters
        {
            Status = PostStatus.Active,
            SubcategoryId = post.SubcategoryId,
            Geo = post.Location != null
                ? new GeoFilter(post.Location, PostSimilarityThresholds.MaxDistanceMeters / 1000.0)
                : null,
            Time = new TimeFilter(
                From: post.EventTime - window,
                To: post.EventTime + window)
        };

        var (filterSql, filterParams) = BuildFilters(filters);

        var sql = $"""
            SELECT id, (1.0 - (embedding <=> @embedding)) AS similarity
            FROM posts
            WHERE deleted_at IS NULL
                AND id != @postId
                AND embedding_status = 'Ready'
                AND embedding IS NOT NULL
                AND location IS NOT NULL
                AND post_type != @postType
                AND author_id != @authorId
                AND (1.0 - (embedding <=> @embedding)) >= @minSimilarity
                {filterSql}
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
            command.Parameters.Add(new NpgsqlParameter("@postId", post.Id));
            command.Parameters.Add(new NpgsqlParameter("@embedding", embeddingVec));
            command.Parameters.Add(new NpgsqlParameter("@postType", post.PostType.ToString()));
            command.Parameters.Add(new NpgsqlParameter("@authorId", post.AuthorId));
            command.Parameters.Add(new NpgsqlParameter("@minSimilarity", PostSimilarityThresholds.MediumSimilarityThreshold));
            command.Parameters.AddRange(filterParams.ToArray());

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
            .Include(p => p.CardDetail)
            .Include(p => p.PersonalBelongingDetail)
            .Include(p => p.ElectronicDetail)
            .Include(p => p.OtherDetail)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var postMap = posts.ToDictionary(p => p.Id);
        return rankedIds
            .Where(r => postMap.ContainsKey(r.Id))
            .Select(r => (postMap[r.Id], r.Similarity));
    }

    public async Task<Dictionary<(PostType Type, PostStatus Status), int>> GetBreakdownAsync(
        CancellationToken cancellationToken = default)
    {
        var data = await _dbSet
            .AsNoTracking()
            .Where(p => p.DeletedAt == null)
            .GroupBy(p => new { p.PostType, p.Status })
            .Select(g => new { g.Key.PostType, g.Key.Status, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return data.ToDictionary(d => (d.PostType, d.Status), d => d.Count);
    }

    public async Task<List<(string Period, int Count)>> GetGrowthChartAsync(
        int months,
        CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMonths(-months);
        const string sql = @"
            SELECT TO_CHAR(DATE_TRUNC('month', created_at), 'YYYY-MM') AS period,
                   COUNT(*)::int AS count
            FROM posts
            WHERE created_at >= @cutoff
              AND deleted_at IS NULL
            GROUP BY DATE_TRUNC('month', created_at)
            ORDER BY 1";

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        var result = new List<(string, int)>();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(new NpgsqlParameter("@cutoff", cutoff));
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add((reader.GetString(0), reader.GetInt32(1)));

        return result;
    }

    public async Task<List<(string Period, PostType PostType, PostStatus Status, int Count)>> GetMonthlyPostKpiAsync(
        int months,
        CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMonths(-months);
        const string sql = @"
            SELECT TO_CHAR(DATE_TRUNC('month', created_at), 'YYYY-MM') AS period,
                   post_type,
                   status,
                   COUNT(*)::int AS count
            FROM posts
            WHERE deleted_at IS NULL
              AND created_at >= @cutoff
            GROUP BY DATE_TRUNC('month', created_at), post_type, status
            ORDER BY 1";

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        var result = new List<(string, PostType, PostStatus, int)>();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(new NpgsqlParameter("@cutoff", cutoff));
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var period = reader.GetString(0);
            var postType = Enum.Parse<PostType>(reader.GetString(1));
            var status = Enum.Parse<PostStatus>(reader.GetString(2));
            var count = reader.GetInt32(3);
            result.Add((period, postType, status, count));
        }

        return result;
    }

    public async Task<List<(int Year, int Month, PostType PostType, int Count)>> GetPostMonthlyAsync(
        int months, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMonths(-months);
        const string sql = @"
            SELECT EXTRACT(YEAR  FROM created_at)::int AS year,
                   EXTRACT(MONTH FROM created_at)::int AS month,
                   post_type,
                   COUNT(*)::int AS count
            FROM posts
            WHERE deleted_at IS NULL
              AND created_at >= @cutoff
            GROUP BY EXTRACT(YEAR FROM created_at), EXTRACT(MONTH FROM created_at), post_type
            ORDER BY 1, 2";

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        var result = new List<(int, int, PostType, int)>();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(new NpgsqlParameter("@cutoff", cutoff));
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add((reader.GetInt32(0), reader.GetInt32(1), Enum.Parse<PostType>(reader.GetString(2)), reader.GetInt32(3)));

        return result;
    }

    public async Task<Dictionary<Guid, (int Total, int Returned)>> GetStatsByOrgIdsAsync(
        IEnumerable<Guid> orgIds,
        CancellationToken cancellationToken = default)
    {
        var ids = orgIds.ToList();
        var rows = await _dbSet.AsNoTracking()
            .Where(p => p.OrganizationId.HasValue && ids.Contains(p.OrganizationId!.Value))
            .GroupBy(p => p.OrganizationId!.Value)
            .Select(g => new
            {
                OrgId = g.Key,
                Total = g.Count(),
                Returned = g.Count(p => p.Status == PostStatus.Returned)
            })
            .ToListAsync(cancellationToken);
        return rows.ToDictionary(r => r.OrgId, r => (r.Total, r.Returned));
    }

    public async Task<IEnumerable<Post>> GetCardMatchCandidatesAsync(
        Post post,
        CancellationToken cancellationToken = default)
    {
        if (post.Status != PostStatus.Active)
            return [];

        var cardNumberHash = post.CardDetail?.CardNumberHash;
        var holderNameNormalized = post.CardDetail?.HolderNameNormalized;

        // Nothing to match on — skip card matching entirely
        if (cardNumberHash is null && holderNameNormalized is null)
            return [];

        var window = TimeSpan.FromDays(PostSimilarityThresholds.TimeWindowDays);

        // Core card matching: CardNumberHash (definitive) OR HolderNameNormalized (strong signal)
        // Secondary filters: distance + time window to reduce false positives
        const string sql = """
            SELECT p.id
            FROM posts p
            INNER JOIN post_card_details pcd ON pcd.post_id = p.id
            WHERE p.deleted_at IS NULL
                AND p.id != @postId
                AND p.status = 'Active'
                AND p.post_type != @postType
                AND p.author_id != @authorId
                AND p.location IS NOT NULL
                AND ST_DWithin(
                    p.location::geography,
                    ST_SetSRID(ST_MakePoint(@longitude, @latitude), 4326)::geography,
                    @maxDistance)
                AND p.event_time BETWEEN @fromTime AND @toTime
                AND (
                    (@cardNumberHash IS NOT NULL AND pcd.card_number_hash = @cardNumberHash)
                    OR
                    (@holderNameNormalized IS NOT NULL AND pcd.holder_name_normalized = @holderNameNormalized)
                )
            """;

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        var ids = new List<Guid>();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = sql;
            cmd.Parameters.Add(new NpgsqlParameter("@postId", post.Id));
            cmd.Parameters.Add(new NpgsqlParameter("@postType", post.PostType.ToString()));
            cmd.Parameters.Add(new NpgsqlParameter("@authorId", post.AuthorId));
            cmd.Parameters.Add(new NpgsqlParameter("@longitude", post.Location.Longitude));
            cmd.Parameters.Add(new NpgsqlParameter("@latitude", post.Location.Latitude));
            cmd.Parameters.Add(new NpgsqlParameter("@maxDistance", PostSimilarityThresholds.MaxDistanceMeters));
            cmd.Parameters.Add(new NpgsqlParameter("@fromTime", post.EventTime - window));
            cmd.Parameters.Add(new NpgsqlParameter("@toTime", post.EventTime + window));
            cmd.Parameters.Add(new NpgsqlParameter("@cardNumberHash", NpgsqlTypes.NpgsqlDbType.Text)
            { Value = (object?)cardNumberHash ?? DBNull.Value });
            cmd.Parameters.Add(new NpgsqlParameter("@holderNameNormalized", NpgsqlTypes.NpgsqlDbType.Text)
            { Value = (object?)holderNameNormalized ?? DBNull.Value });

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                ids.Add(reader.GetGuid(0));
        }

        if (ids.Count == 0)
            return [];

        return await _dbSet
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.Organization)
            .Include(p => p.CardDetail)
            .Include(p => p.PersonalBelongingDetail)
            .Include(p => p.ElectronicDetail)
            .Include(p => p.OtherDetail)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        PostFilters? filters = null,
        CancellationToken cancellationToken = default)
    {
        var (filterSql, parameters) = BuildFilters(filters);
        var sql = $@"SELECT COUNT(*) FROM posts WHERE deleted_at IS NULL {filterSql}";

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddRange(parameters.ToArray());
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<IEnumerable<Post>> SearchByTitleAsync(
        string title,
        PostFilters? filters = null,
        CancellationToken cancellationToken = default)
    {
        var (filterSql, parameters) = BuildFilters(filters);
        parameters.Insert(0, new("@title", $"%{title}%"));

        var sql = $@"
            SELECT id
            FROM posts
            WHERE deleted_at IS NULL
                AND post_title ILIKE @title
                {filterSql}
            ORDER BY created_at DESC";

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        var ids = new List<Guid>();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = sql;
            cmd.Parameters.AddRange(parameters.ToArray());

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                ids.Add(reader.GetGuid(0));
        }

        if (ids.Count == 0)
            return [];

        var posts = await _dbSet
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.Organization)
            .Include(p => p.CardDetail)
            .Include(p => p.PersonalBelongingDetail)
            .Include(p => p.ElectronicDetail)
            .Include(p => p.OtherDetail)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var postMap = posts.ToDictionary(p => p.Id);
        return ids.Where(id => postMap.ContainsKey(id)).Select(id => postMap[id]);
    }

}
