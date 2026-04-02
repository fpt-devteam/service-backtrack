using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases;
using Backtrack.Core.Application.Utils;
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

    public async Task<(IEnumerable<(Post Post, double? DistanceMeters)> Items, int TotalCount)> GetPagedAsync(
        PagedQuery pagedQuery,
        string? searchTerm = null,
        PostType? postType = null,
        Guid? organizationId = null,
        GeoPoint? location = null,
        double? radiusInKm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable().Where(p => p.DeletedAt == null);

        if (postType is not null)
            query = query.Where(p => p.PostType == postType);

        if (organizationId is not null)
            query = query.Where(p => p.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p =>
                EF.Functions.ILike(p.Item.ItemName, $"%{searchTerm}%") ||
                EF.Functions.ILike(p.Item.AdditionalDetails ?? "", $"%{searchTerm}%"));

        if (location != null && radiusInKm.HasValue)
        {
            var radiusInMeters = radiusInKm.Value * 1000;
            var ids = await _context.Database
                .SqlQueryRaw<Guid>(@"
                    SELECT id FROM posts
                    WHERE location IS NOT NULL
                    AND ST_DWithin(
                        location::geography,
                        ST_SetSRID(ST_MakePoint({0}, {1}), 4326)::geography,
                        {2}
                    )
                    AND deleted_at IS NULL
                ", location.Longitude, location.Latitude, radiusInMeters)
                .ToListAsync(cancellationToken);
            query = query.Where(p => ids.Contains(p.Id));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(p => p.Author)
            .Include(p => p.Organization)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(pagedQuery.Offset)
            .Take(pagedQuery.Limit)
            .ToListAsync(cancellationToken);

        if (location != null)
        {
            var withDistance = items
                .Select(p => (p, (double?)Haversine(location.Latitude, location.Longitude, p.Location!.Latitude, p.Location.Longitude)))
                .OrderBy(x => x.Item2)
                .ToList();
            return (withDistance, totalCount);
        }

        return (items.Select(p => (p, (double?)null)).ToList(), totalCount);
    }

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        var φ1 = lat1 * Math.PI / 180;
        var φ2 = lat2 * Math.PI / 180;
        var Δφ = (lat2 - lat1) * Math.PI / 180;
        var Δλ = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                Math.Cos(φ1) * Math.Cos(φ2) *
                Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
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

        // col 0: id, 1: post_type, 2: item (jsonb)
        // col 3: location, 4: external_place_id, 5: display_address
        // col 6: event_time, 7: created_at, 8: updated_at
        // col 9: embedding, 10: content_hash, 11: embedding_status, 12: author_id, 13: image_urls, 14: similarity
        var sql = $@"
            WITH ranked AS (
                SELECT
                    id, post_type, item,
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
                id, post_type, item,
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
                    Id = reader.GetGuid(0),
                    PostType = Enum.Parse<PostType>(reader.GetString(1)),
                    Item = JsonSerializer.Deserialize<PostItem>(reader.GetString(2), _jsonOptions)!,
                    Location = new GeoPoint(
                        ((Point)reader.GetValue(3)).Y,
                        ((Point)reader.GetValue(3)).X),
                    ExternalPlaceId = reader.IsDBNull(4) ? null : reader.GetString(4),
                    DisplayAddress = reader.IsDBNull(5) ? null : reader.GetString(5),
                    EventTime = reader.GetFieldValue<DateTimeOffset>(6),
                    CreatedAt = reader.GetFieldValue<DateTimeOffset>(7),
                    UpdatedAt = reader.IsDBNull(8) ? null : reader.GetFieldValue<DateTimeOffset>(8),
                    Embedding = reader.IsDBNull(9) ? null : ((Vector)reader.GetValue(9)).ToArray(),
                    ContentHash = reader.GetString(10),
                    EmbeddingStatus = Enum.Parse<EmbeddingStatus>(reader.GetString(11)),
                    PostMatchingStatus = PostMatchingStatus.Completed,
                    AuthorId = reader.GetString(12),
                    ImageUrls = JsonSerializer.Deserialize<List<string>>(reader.GetString(13), _jsonOptions) ?? new()
                };
                rawResults.Add((post, reader.GetDouble(14)));
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
        // col 0: id, 1: post_type, 2: item (jsonb)
        // col 3: location, 4: external_place_id, 5: display_address
        // col 6: event_time, 7: created_at, 8: updated_at
        // col 9: content_hash, 10: embedding_status, 11: author_id
        // col 12: image_urls, 13: similarity, 14: distance_meters

        var ic = System.Globalization.CultureInfo.InvariantCulture;
        var wEmbed = PostMatchingCriteria.EmbeddingSimilarityWeight.ToString(ic);
        var wLoc = PostMatchingCriteria.LocationWeight.ToString(ic);
        var locExpr = "(1.0 - LEAST(ST_Distance(location::geography, ST_SetSRID(ST_MakePoint(@longitude, @latitude), 4326)::geography), @maxDistance) / @maxDistance)";

        var sql = $"""
            WITH candidates AS (
                SELECT
                    id, post_type, item,
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
            SELECT id, post_type, item,
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

    public async Task<(IEnumerable<(Post Post, double DistanceMeters)> Items, int TotalCount)> GetFeedAsync(
        GeoPoint location,
        int offset,
        int limit,
        CancellationToken cancellationToken = default)
    {
        const string countSql = @"
            SELECT COUNT(*)
            FROM posts
            WHERE deleted_at IS NULL
                AND location IS NOT NULL";

        const string sql = @"
            SELECT
                id,
                ST_Distance(
                    location::geography,
                    ST_SetSRID(ST_MakePoint(@longitude, @latitude), 4326)::geography
                ) AS dist,
                event_time
            FROM posts
            WHERE deleted_at IS NULL
                AND location IS NOT NULL
            ORDER BY dist ASC, event_time DESC
            LIMIT @limit OFFSET @offset";

        var conn = _context.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        int totalCount;
        await using (var countCmd = conn.CreateCommand())
        {
            countCmd.CommandText = countSql;
            totalCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync(cancellationToken));
        }

        var idDistances = new List<(Guid Id, double Distance)>();

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = sql;
            cmd.Parameters.Add(new NpgsqlParameter("@longitude", location.Longitude));
            cmd.Parameters.Add(new NpgsqlParameter("@latitude", location.Latitude));
            cmd.Parameters.Add(new NpgsqlParameter("@limit", limit));
            cmd.Parameters.Add(new NpgsqlParameter("@offset", offset));

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                idDistances.Add((reader.GetGuid(0), reader.GetDouble(1)));
        }

        if (idDistances.Count == 0)
            return ([], totalCount);

        var ids = idDistances.Select(x => x.Id).ToList();
        var posts = await _dbSet
            .Include(p => p.Author)
            .Include(p => p.Organization)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var postMap = posts.ToDictionary(p => p.Id);

        var items = idDistances
            .Where(x => postMap.ContainsKey(x.Id))
            .Select(x => (postMap[x.Id], x.Distance))
            .ToList();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Post>> GetByAuthorIdAsync(string authorId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Author)
            .Include(p => p.Organization)
            .Where(p => p.AuthorId == authorId && p.DeletedAt == null)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

}
