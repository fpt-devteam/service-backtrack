using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Application.Utils;
using static Backtrack.Core.Application.Utils.GeoUtil;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Npgsql;
using NpgsqlTypes;
using Pgvector;
using System.Data;
using System.Text.Json;

namespace Backtrack.Core.Infrastructure.Repositories;

public class PostRepository(ApplicationDbContext context) : CrudRepositoryBase<Post, Guid>(context), IPostRepository
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

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
        if (filters?.Time != null)
        {
            clauses.Add("AND event_time >= @fromTime");
            clauses.Add("AND event_time <= @toTime");
            parameters.Add(new("@fromTime", filters.Time.From));
            parameters.Add(new("@toTime",   filters.Time.To));
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

    private static PostItem ReadPostItem(IDataReader reader, int startIndex) => new()
    {
        ItemName        = reader.GetString(startIndex),
        Category        = reader.IsDBNull(startIndex + 1) ? ItemCategory.Other : Enum.Parse<ItemCategory>(reader.GetString(startIndex + 1)),
        Color           = reader.IsDBNull(startIndex + 2) ? null : reader.GetString(startIndex + 2),
        Brand           = reader.IsDBNull(startIndex + 3) ? null : reader.GetString(startIndex + 3),
        Condition       = reader.IsDBNull(startIndex + 4) ? null : reader.GetString(startIndex + 4),
        Material        = reader.IsDBNull(startIndex + 5) ? null : reader.GetString(startIndex + 5),
        Size            = reader.IsDBNull(startIndex + 6) ? null : reader.GetString(startIndex + 6),
        DistinctiveMarks    = reader.IsDBNull(startIndex + 7) ? null : reader.GetString(startIndex + 7),
        AdditionalDetails   = reader.IsDBNull(startIndex + 8) ? null : reader.GetString(startIndex + 8),
    };

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
        var (filterSql, parameters) = BuildFilters(filters);

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

        parameters.Add(new("@limit", pagedQuery.Limit));
        parameters.Add(new("@offset", pagedQuery.Offset));

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        int totalCount;
        await using (var countCmd = conn.CreateCommand())
        {
            countCmd.CommandText = countSql;
            // count query doesn't need limit/offset params — use all except last two
            countCmd.Parameters.AddRange(parameters.Take(parameters.Count - 2).ToArray());
            totalCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync(cancellationToken));
        }

        var ids = new List<Guid>();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = dataSql;
            cmd.Parameters.AddRange(parameters.ToArray());

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

    public async Task<(IEnumerable<(Post Post, double SimilarityScore, double? DistanceMeters)> Items, int TotalCount)> SearchBySemanticAsync(
        float[] queryEmbedding,
        PagedQuery pagedQuery,
        PostType? postType = null,
        GeoPoint? location = null,
        double? radiusInKm = null,
        Guid? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        const double MinimumSimilarityThreshold = 0.5;

        var embeddingArrayLiteral = "[" + string.Join(",", queryEmbedding.Select(f => f.ToString(System.Globalization.CultureInfo.InvariantCulture))) + "]";

        var postTypeCondition = postType.HasValue ? $"AND post_type = '{postType.Value}'" : "";
        var orgCondition = organizationId.HasValue ? $"AND organization_id = '{organizationId.Value}'" : "";

        var hasLocation = location != null && radiusInKm.HasValue;
        double? radiusInMeters = hasLocation ? radiusInKm!.Value * 1000 : null;

        var locationCondition = hasLocation ? @"
                AND location IS NOT NULL
                AND ST_DWithin(
                    location::geography,
                    ST_SetSRID(ST_MakePoint(@longitude, @latitude), 4326)::geography,
                    @radius
                )" : "";

        // col 0:id  1:post_type
        // col 2:item_name  3:item_category  4:item_color  5:item_brand  6:item_condition
        //     7:item_material  8:item_size  9:item_distinctive_marks  10:item_additional_details
        // col 11:location  12:external_place_id  13:display_address
        // col 14:event_time  15:created_at  16:updated_at
        // col 17:embedding  18:content_hash  19:embedding_status  20:author_id  21:image_urls
        // col 22:similarity
        var sql = $@"
            WITH ranked AS (
                SELECT
                    id, post_type,
                    item_name, item_category, item_color, item_brand, item_condition,
                    item_material, item_size, item_distinctive_marks, item_additional_details,
                    location, external_place_id, display_address,
                    event_time, created_at, updated_at,
                    embedding, content_hash, embedding_status, author_id, image_urls,
                    (embedding <=> @queryEmbedding::vector) AS distance,
                    (1.0 - (embedding <=> @queryEmbedding::vector)) AS similarity
                FROM posts
                WHERE deleted_at IS NULL
                    AND embedding_status = 'Ready'
                    AND embedding IS NOT NULL
                    {postTypeCondition}
                    {orgCondition}
                    {locationCondition}
            )
            SELECT
                id, post_type,
                item_name, item_category, item_color, item_brand, item_condition,
                item_material, item_size, item_distinctive_marks, item_additional_details,
                location, external_place_id, display_address,
                event_time, created_at, updated_at,
                embedding, content_hash, embedding_status, author_id, image_urls,
                similarity
            FROM ranked
            WHERE similarity >= @minSimilarity
            ORDER BY distance ASC, id ASC
            OFFSET @offset
            LIMIT @limit;

            SELECT COUNT(*)::int
            FROM posts
            WHERE deleted_at IS NULL
                AND embedding_status = 'Ready'
                AND embedding IS NOT NULL
                {postTypeCondition}
                {orgCondition}
                {locationCondition}
                AND (1.0 - (embedding <=> @queryEmbedding::vector)) >= @minSimilarity;
        ";

        var parameters = new List<NpgsqlParameter>
        {
            new("@queryEmbedding", embeddingArrayLiteral),
            new("@minSimilarity", MinimumSimilarityThreshold),
            new("@offset", pagedQuery.Offset),
            new("@limit", pagedQuery.Limit)
        };

        if (hasLocation)
        {
            parameters.Add(new("@longitude", location!.Longitude));
            parameters.Add(new("@latitude", location.Latitude));
            parameters.Add(new("@radius", radiusInMeters!.Value));
        }

        var conn = _context.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        using var command = conn.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddRange(parameters.ToArray());

        var rawResults = new List<(Post Post, double SimilarityScore)>();
        int totalCount = 0;

        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var post = new Post
                {
                    Id              = reader.GetGuid(0),
                    PostType        = Enum.Parse<PostType>(reader.GetString(1)),
                    Item            = ReadPostItem(reader, 2),
                    Location        = new GeoPoint(((Point)reader.GetValue(11)).Y, ((Point)reader.GetValue(11)).X),
                    ExternalPlaceId = reader.IsDBNull(12) ? null : reader.GetString(12),
                    DisplayAddress  = reader.IsDBNull(13) ? null : reader.GetString(13),
                    EventTime       = reader.GetFieldValue<DateTimeOffset>(14),
                    CreatedAt       = reader.GetFieldValue<DateTimeOffset>(15),
                    UpdatedAt       = reader.IsDBNull(16) ? null : reader.GetFieldValue<DateTimeOffset>(16),
                    Embedding       = reader.IsDBNull(17) ? null : ((Vector)reader.GetValue(17)).ToArray(),
                    ContentHash     = reader.GetString(18),
                    EmbeddingStatus = Enum.Parse<EmbeddingStatus>(reader.GetString(19)),
                    PostMatchingStatus = PostMatchingStatus.Completed,
                    AuthorId        = reader.GetString(20),
                    ImageUrls       = reader.IsDBNull(21) ? [] : reader.GetFieldValue<string[]>(21).ToList()
                };
                rawResults.Add((post, reader.GetDouble(22)));
            }

            if (await reader.NextResultAsync(cancellationToken) && await reader.ReadAsync(cancellationToken))
                totalCount = reader.GetInt32(0);
        }

        if (location != null)
        {
            var withDistance = rawResults
                .Select(r => (r.Post, r.SimilarityScore, (double?)Haversine(location.Latitude, location.Longitude, r.Post.Location!.Latitude, r.Post.Location.Longitude)))
                .OrderBy(r => r.Item3)
                .ToList();
            return (withDistance, totalCount);
        }

        return (rawResults.Select(r => (r.Post, r.SimilarityScore, (double?)null)).ToList(), totalCount);
    }

    public async Task<IEnumerable<(Post Post, double Similarity, double DistanceMeters)>> GetSimilarPostsAsync(
        Post post,
        CancellationToken cancellationToken = default)
    {
        // col 0:id  1:post_type
        // col 2:item_name  3:item_category  4:item_color  5:item_brand  6:item_condition
        //     7:item_material  8:item_size  9:item_distinctive_marks  10:item_additional_details
        // col 11:location  12:external_place_id  13:display_address
        // col 14:event_time  15:created_at  16:updated_at
        // col 17:content_hash  18:embedding_status  19:author_id  20:image_urls
        // col 21:similarity  22:distance_meters

        var ic = System.Globalization.CultureInfo.InvariantCulture;
        var wEmbed = PostMatchingCriteria.EmbeddingSimilarityWeight.ToString(ic);
        var wLoc = PostMatchingCriteria.LocationWeight.ToString(ic);
        var locExpr = "(1.0 - LEAST(ST_Distance(location::geography, ST_SetSRID(ST_MakePoint(@longitude, @latitude), 4326)::geography), @maxDistance) / @maxDistance)";

        var sql = $"""
            WITH candidates AS (
                SELECT
                    id, post_type,
                    item_name, item_category, item_color, item_brand, item_condition,
                    item_material, item_size, item_distinctive_marks, item_additional_details,
                    location, external_place_id, display_address,
                    event_time, created_at, updated_at,
                    content_hash, embedding_status, author_id, image_urls,
                    (1.0 - (embedding <=> @embedding)) AS similarity,
                    ST_Distance(
                        location::geography,
                        ST_SetSRID(ST_MakePoint(@longitude, @latitude), 4326)::geography
                    ) AS distance_meters,
                    (
                        (1.0 - (embedding <=> @embedding)) * {wEmbed} +
                        {locExpr} * {wLoc}
                    ) AS match_score
                FROM posts
                WHERE deleted_at IS NULL
                    AND id != @postId
                    AND embedding_status = 'Ready'
                    AND embedding IS NOT NULL
                    AND location IS NOT NULL
                    AND post_type != @postType
                    AND author_id != @authorId
                    AND ST_DWithin(
                        location::geography,
                        ST_SetSRID(ST_MakePoint(@longitude, @latitude), 4326)::geography,
                        @maxDistance
                    )
            )
            SELECT id, post_type,
                   item_name, item_category, item_color, item_brand, item_condition,
                   item_material, item_size, item_distinctive_marks, item_additional_details,
                   location, external_place_id, display_address,
                   event_time, created_at, updated_at,
                   content_hash, embedding_status, author_id,
                   image_urls, similarity, distance_meters
            FROM candidates
            WHERE match_score >= @minScore
            ORDER BY match_score DESC
            """;

        var embeddingVec = new Vector(post.Embedding ?? Array.Empty<float>());
        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await _context.Database.OpenConnectionAsync(cancellationToken);

        await using var command = conn.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(new NpgsqlParameter("@postId", post.Id));
        command.Parameters.Add(new NpgsqlParameter("@embedding", embeddingVec));
        command.Parameters.Add(new NpgsqlParameter("@postType", post.PostType.ToString()));
        command.Parameters.Add(new NpgsqlParameter("@authorId", post.AuthorId));
        command.Parameters.Add(new NpgsqlParameter("@longitude", post.Location?.Longitude ?? 0));
        command.Parameters.Add(new NpgsqlParameter("@latitude", post.Location?.Latitude ?? 0));
        command.Parameters.Add(new NpgsqlParameter("@maxDistance", PostMatchingCriteria.MaxDistanceMeters));
        command.Parameters.Add(new NpgsqlParameter("@minScore", (double)PostMatchingCriteria.MinMatchScore));

        var results = new List<(Post Post, double Similarity, double DistanceMeters)>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var p = new Post
            {
                Id = reader.GetGuid(0),
                PostType = Enum.Parse<PostType>(reader.GetString(1)),
                Item = JsonSerializer.Deserialize<PostItem>(reader.GetString(2), _jsonOptions)!,
                Location = new GeoPoint(((Point)reader.GetValue(3)).Y, ((Point)reader.GetValue(3)).X),
                ExternalPlaceId = await reader.IsDBNullAsync(4) ? null : reader.GetString(4),
                DisplayAddress = await reader.IsDBNullAsync(5) ? null : reader.GetString(5),
                EventTime = await reader.GetFieldValueAsync<DateTimeOffset>(6, cancellationToken),
                CreatedAt = await reader.GetFieldValueAsync<DateTimeOffset>(7, cancellationToken),
                UpdatedAt = await reader.IsDBNullAsync(8) ? null : await reader.GetFieldValueAsync<DateTimeOffset>(8, cancellationToken),
                ContentHash = reader.GetString(9),
                EmbeddingStatus = Enum.Parse<EmbeddingStatus>(reader.GetString(10)),
                PostMatchingStatus = PostMatchingStatus.Completed,
                AuthorId = reader.GetString(11),
                ImageUrls = JsonSerializer.Deserialize<List<string>>(reader.GetString(12), _jsonOptions) ?? new()
            };

            results.Add((p, reader.GetDouble(13), reader.GetDouble(14)));
        }

        return results;
    }

    public async Task<IEnumerable<Post>> SearchByFullTextAsync(
        string searchTerm,
        PostFilters? filters = null,
        CancellationToken cancellationToken = default)
    {
        var (filterSql, parameters) = BuildFilters(filters);

        var sql = $@"
            SELECT id,
                   ts_rank(item_search, websearch_to_tsquery('english', @searchTerm)) AS rank
            FROM posts
            WHERE deleted_at IS NULL
                AND item_search @@ websearch_to_tsquery('english', @searchTerm)
                {filterSql}
            ORDER BY rank DESC";

        parameters.Insert(0, new("@searchTerm", searchTerm));

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        var rankedIds = new List<Guid>();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = sql;
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
