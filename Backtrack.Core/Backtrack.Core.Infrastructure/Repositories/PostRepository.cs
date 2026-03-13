using Backtrack.Core.Application.Interfaces.Repositories;
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

namespace Backtrack.Core.Infrastructure.Repositories;

public class PostRepository(ApplicationDbContext context) : CrudRepositoryBase<Post, Guid>(context), IPostRepository
{
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
        int offset,
        int limit,
        PostType? postType = null,
        string? searchTerm = null,
        double? latitude = null,
        double? longitude = null,
        double? radiusInKm = null,
        string? authorId = null,
        Guid? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (postType is not null)
        {
            query = query.Where(p => p.PostType == postType);
        }

        if (authorId is not null)
        {
            query = query.Where(p => p.AuthorId == authorId);
        }

        if (organizationId is not null)
        {
            query = query.Where(p => p.OrganizationId == organizationId);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p =>
                p.ItemName.Contains(searchTerm) ||
                p.Description.Contains(searchTerm));
        }

        if (latitude.HasValue && longitude.HasValue && radiusInKm.HasValue)
        {
            var searchLocation = new GeoPoint(latitude.Value, longitude.Value);

            var radiusInMeters = radiusInKm.Value * 1000;

            var ids = await _context.Database
                .SqlQueryRaw<Guid>(@"
                        SELECT id
                        FROM posts
                        WHERE location IS NOT NULL
                        AND ST_DWithin(
                            location::geography,
                            ST_SetSRID(ST_MakePoint({0}, {1}), 4326)::geography,
                            {2}
                        )
                        AND deleted_at IS NULL
                    ", longitude.Value, latitude.Value, radiusInMeters)
                .ToListAsync(cancellationToken);

            query = query.Where(p => ids.Contains(p.Id));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(p => p.Author)
            .Include(p => p.Organization)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<(Post Post, double SimilarityScore)> Items, int TotalCount)> SearchBySemanticAsync(
        float[] queryEmbedding,
        int offset,
        int limit,
        PostType? postType = null,
        double? latitude = null,
        double? longitude = null,
        double? radiusInKm = null,
        CancellationToken cancellationToken = default)
    {
        const double MinimumSimilarityThreshold = 0.15; // Filter out results with similarity < 55%

        // Convert embedding to PostgreSQL array format for SQL
        var embeddingArrayLiteral = "[" + string.Join(",", queryEmbedding.Select(f => f.ToString(System.Globalization.CultureInfo.InvariantCulture))) + "]";

        // Build SQL with safe string interpolation for enum (not user input)
        var postTypeCondition = postType.HasValue
            ? $"AND post_type = '{postType.Value}'"
            : "";

        var locationCondition = "";
        double? radiusInMeters = null;
        if (latitude.HasValue && longitude.HasValue && radiusInKm.HasValue)
        {
            radiusInMeters = radiusInKm.Value * 1000;
            locationCondition = @"
                    AND location IS NOT NULL
                    AND ST_DWithin(
                        location::geography,
                        ST_SetSRID(ST_MakePoint(@longitude, @latitude), 4326)::geography,
                        @radius
                    )";
        }

        // Build SQL query using PostgreSQL pgvector operations
        var sql = $@"
                WITH filtered_posts AS (
                    SELECT
                        id,
                        post_type,
                        item_name,
                        description,
                        image_urls,
                        location,
                        external_place_id,
                        display_address,
                        event_time,
                        created_at,
                        updated_at,
                        multimodal_embedding,
                        content_hash,
                        content_embedding_status,
                        (multimodal_embedding <=> @queryEmbedding::vector) AS distance,
                        (1.0 - (multimodal_embedding <=> @queryEmbedding::vector)) AS similarity,
                        author_id
                    FROM posts
                    WHERE deleted_at IS NULL
                        AND content_embedding_status = 'Ready'
                        AND multimodal_embedding IS NOT NULL
                        {postTypeCondition}
                        {locationCondition}
                )
                SELECT
                    id,
                    post_type,
                    item_name,
                    description,
                    image_urls,
                    location,
                    external_place_id,
                    display_address,
                    event_time,
                    created_at,
                    updated_at,
                    multimodal_embedding,
                    content_hash,
                    content_embedding_status,
                    similarity
                FROM filtered_posts
                WHERE similarity >= @minSimilarity
                ORDER BY distance ASC
                OFFSET @offset
                LIMIT @limit;

                SELECT COUNT(*)::int
                FROM posts
                WHERE deleted_at IS NULL
                    AND content_embedding_status = 'Ready'
                    AND multimodal_embedding IS NOT NULL
                    {postTypeCondition}
                    {locationCondition}
                    AND (1.0 - (multimodal_embedding <=> @queryEmbedding::vector)) >= @minSimilarity;
            ";

        // Create parameters
        var parameters = new List<Npgsql.NpgsqlParameter>
        {
            new Npgsql.NpgsqlParameter("@queryEmbedding", embeddingArrayLiteral),
            new Npgsql.NpgsqlParameter("@minSimilarity", MinimumSimilarityThreshold),
            new Npgsql.NpgsqlParameter("@offset", offset),
            new Npgsql.NpgsqlParameter("@limit", limit)
        };

        if (radiusInMeters.HasValue && latitude.HasValue && longitude.HasValue)
        {
            parameters.Add(new Npgsql.NpgsqlParameter("@longitude", longitude.Value));
            parameters.Add(new Npgsql.NpgsqlParameter("@latitude", latitude.Value));
            parameters.Add(new Npgsql.NpgsqlParameter("@radius", radiusInMeters.Value));
        }

        // Execute raw SQL query
        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddRange(parameters.ToArray());

        if (_context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
        {
            await _context.Database.OpenConnectionAsync(cancellationToken);
        }

        var results = new List<(Post Post, double SimilarityScore)>();
        int totalCount = 0;

        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            // Read post results
            while (await reader.ReadAsync(cancellationToken))
            {
                var post = new Post
                {
                    Id = reader.GetGuid(0),
                    PostType = Enum.Parse<PostType>(reader.GetString(1)),
                    ItemName = reader.GetString(2),
                    Description = reader.GetString(3),
                    ImageUrls = reader.GetFieldValue<string[]>(4),
                    Location = new GeoPoint(
                            ((Point)reader.GetValue(5)).Y, // latitude
                            ((Point)reader.GetValue(5)).X  // longitude
                        ),
                    ExternalPlaceId = reader.IsDBNull(6) ? null : reader.GetString(6),
                    DisplayAddress = reader.IsDBNull(7) ? null : reader.GetString(7),
                    EventTime = reader.GetFieldValue<DateTimeOffset>(8),
                    CreatedAt = reader.GetFieldValue<DateTimeOffset>(9),
                    UpdatedAt = reader.IsDBNull(10)
                        ? null
                        : reader.GetFieldValue<DateTimeOffset>(10),
                    MultimodalEmbedding = reader.IsDBNull(11)
                        ? null
                        : ((Vector)reader.GetValue(11)).ToArray(),
                    ContentHash = reader.GetString(12),
                    ContentEmbeddingStatus = (ContentEmbeddingStatus)Enum.Parse(typeof(ContentEmbeddingStatus), reader.GetString(13)),
                    AuthorId = reader.GetString(14)
                };

                var similarity = reader.GetDouble(15);
                results.Add((post, similarity));
            }

            // Read total count
            if (await reader.NextResultAsync(cancellationToken) && await reader.ReadAsync(cancellationToken))
            {
                totalCount = reader.GetInt32(0);
            }
        }

        return (results, totalCount);
    }

    public async Task<IEnumerable<(Post Post, double Similarity, double DistanceMeters)>> GetSimilarPostsAsync(
        Post post,
        CancellationToken cancellationToken = default)
    {
        var queryVec = new Vector(post.MultimodalEmbedding ?? Array.Empty<float>());
        var sql = @"
        SELECT
            id,
            post_type,
            item_name,
            description,
            image_urls,
            location,
            external_place_id,
            display_address,
            event_time,
            created_at,
            updated_at,
            multimodal_embedding,
            content_hash,
            content_embedding_status,
            author_id,
            (1.0 - (multimodal_embedding <=> @queryEmbedding)) AS similarity,
            ST_Distance(
                location::geography,
                ST_SetSRID(ST_MakePoint(@longitude, @latitude), 4326)::geography
            ) AS distance_meters
        FROM posts
        WHERE deleted_at IS NULL
            AND id != @postId
            AND content_embedding_status = 'Ready'
            AND multimodal_embedding IS NOT NULL
            AND location IS NOT NULL
            AND ST_DWithin(
                location::geography,
                ST_SetSRID(ST_MakePoint(@longitude, @latitude), 4326)::geography,
                @radius
            )
            AND post_type != @postType
            AND author_id != @authorId
            AND (1.0 - (multimodal_embedding <=> @queryEmbedding)) >= @minSimilarity
        ORDER BY similarity DESC";

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await _context.Database.OpenConnectionAsync(cancellationToken);

        await using var command = conn.CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add(new NpgsqlParameter("@postId", post.Id));
        command.Parameters.Add(new NpgsqlParameter("@queryEmbedding", queryVec));
        command.Parameters.Add(new NpgsqlParameter("@postType", post.PostType.ToString()));
        command.Parameters.Add(new NpgsqlParameter("@authorId", post.AuthorId));
        command.Parameters.Add(new NpgsqlParameter("@minSimilarity", PostSimilarityThresholds.MinMultimodalSimilarity));
        command.Parameters.Add(new NpgsqlParameter("@longitude", post.Location?.Longitude ?? 0));
        command.Parameters.Add(new NpgsqlParameter("@latitude", post.Location?.Latitude ?? 0));
        command.Parameters.Add(new NpgsqlParameter("@radius", PostSimilarityThresholds.MaxDistanceMeters));

        var results = new List<(Post Post, double Similarity, double DistanceMeters)>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var p = new Post
            {
                Id = reader.GetGuid(0),
                PostType = Enum.Parse<PostType>(reader.GetString(1)),
                ItemName = reader.GetString(2),
                Description = reader.GetString(3),
                ImageUrls = await reader.GetFieldValueAsync<string[]>(4, cancellationToken),
                Location = new GeoPoint(
                    ((Point)reader.GetValue(5)).Y,
                    ((Point)reader.GetValue(5)).X),
                ExternalPlaceId = await reader.IsDBNullAsync(6) ? null : reader.GetString(6),
                DisplayAddress = await reader.IsDBNullAsync(7) ? null : reader.GetString(7),
                EventTime = await reader.GetFieldValueAsync<DateTimeOffset>(8, cancellationToken),
                CreatedAt = await reader.GetFieldValueAsync<DateTimeOffset>(9, cancellationToken),
                UpdatedAt = await reader.IsDBNullAsync(10) ? null : await reader.GetFieldValueAsync<DateTimeOffset>(10, cancellationToken),
                MultimodalEmbedding = await reader.IsDBNullAsync(11) ? null : ((Vector)reader.GetValue(11)).ToArray(),
                ContentHash = reader.GetString(12),
                ContentEmbeddingStatus = Enum.Parse<ContentEmbeddingStatus>(reader.GetString(13)),
                AuthorId = reader.GetString(14)
            };

            results.Add((p, reader.GetDouble(15), reader.GetDouble(16)));
        }

        return results;
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
