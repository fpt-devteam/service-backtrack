using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using System.Data;

namespace Backtrack.Core.Infrastructure.Repositories;

public class OrganizationInventoryRepository(ApplicationDbContext context) : CrudRepositoryBase<OrganizationInventory, Guid>(context), IOrganizationInventoryRepository
{
    public override async Task<OrganizationInventory?> GetByIdAsync(Guid id, bool isTrack = false)
    {
        IQueryable<OrganizationInventory> query = _dbSet.Include(oi => oi.LoggedBy).Include(oi => oi.Organization);
        if (!isTrack)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(oi =>
            oi.Id == id &&
            oi.DeletedAt == null);
    }

    public async Task<(IEnumerable<OrganizationInventory> Items, int TotalCount)> GetPagedAsync(
        int offset,
        int limit,
        Guid? orgId = null,
        string? loggedById = null,
        OrganizationInventoryStatus? status = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (orgId.HasValue)
        {
            query = query.Where(oi => oi.OrgId == orgId.Value);
        }

        if (loggedById is not null)
        {
            query = query.Where(oi => oi.LoggedById == loggedById);
        }

        if (status is not null)
        {
            query = query.Where(oi => oi.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(oi =>
                oi.ItemName.Contains(searchTerm) ||
                oi.Description.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(oi => oi.LoggedBy)
            .Include(oi => oi.Organization)
            .OrderByDescending(oi => oi.LoggedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<(OrganizationInventory Inventory, double SimilarityScore)> Items, int TotalCount)> SearchBySemanticAsync(
        float[] queryEmbedding,
        int offset,
        int limit,
        Guid? orgId = null,
        CancellationToken cancellationToken = default)
    {
        const double MinimumSimilarityThreshold = 0.7;

        var embeddingArrayLiteral = "[" + string.Join(",", queryEmbedding.Select(f => f.ToString(System.Globalization.CultureInfo.InvariantCulture))) + "]";

        var orgCondition = orgId.HasValue
            ? $"AND org_id = '{orgId.Value}'"
            : "";

        var sql = $@"
                WITH filtered_inventories AS (
                    SELECT
                        id,
                        org_id,
                        logged_by_id,
                        item_name,
                        description,
                        distinctive_marks,
                        image_urls,
                        storage_location,
                        status,
                        logged_at,
                        created_at,
                        updated_at,
                        multimodal_embedding,
                        (multimodal_embedding <=> @queryEmbedding::vector) AS distance,
                        (1.0 - (multimodal_embedding <=> @queryEmbedding::vector)) AS similarity
                    FROM org_inventories
                    WHERE deleted_at IS NULL
                        AND multimodal_embedding IS NOT NULL
                        {orgCondition}
                )
                SELECT
                    id,
                    org_id,
                    logged_by_id,
                    item_name,
                    description,
                    distinctive_marks,
                    image_urls,
                    storage_location,
                    status,
                    logged_at,
                    created_at,
                    updated_at,
                    multimodal_embedding,
                    similarity
                FROM filtered_inventories
                WHERE similarity >= @minSimilarity
                ORDER BY distance ASC
                OFFSET @offset
                LIMIT @limit;

                SELECT COUNT(*)::int
                FROM org_inventories
                WHERE deleted_at IS NULL
                    AND multimodal_embedding IS NOT NULL
                    {orgCondition}
                    AND (1.0 - (multimodal_embedding <=> @queryEmbedding::vector)) >= @minSimilarity;
            ";

        var parameters = new List<Npgsql.NpgsqlParameter>
        {
            new Npgsql.NpgsqlParameter("@queryEmbedding", embeddingArrayLiteral),
            new Npgsql.NpgsqlParameter("@minSimilarity", MinimumSimilarityThreshold),
            new Npgsql.NpgsqlParameter("@offset", offset),
            new Npgsql.NpgsqlParameter("@limit", limit)
        };

        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddRange(parameters.ToArray());

        if (_context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
        {
            await _context.Database.OpenConnectionAsync(cancellationToken);
        }

        var results = new List<(OrganizationInventory Inventory, double SimilarityScore)>();
        int totalCount = 0;

        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var inventory = new OrganizationInventory
                {
                    Id = reader.GetGuid(0),
                    OrgId = reader.GetGuid(1),
                    LoggedById = reader.GetString(2),
                    ItemName = reader.GetString(3),
                    Description = reader.GetString(4),
                    DistinctiveMarks = reader.IsDBNull(5) ? null : reader.GetString(5),
                    ImageUrls = reader.IsDBNull(6) ? [] : reader.GetFieldValue<string[]>(6),
                    StorageLocation = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Status = Enum.Parse<OrganizationInventoryStatus>(reader.GetString(8)),
                    LoggedAt = reader.GetFieldValue<DateTimeOffset>(9),
                    CreatedAt = reader.GetFieldValue<DateTimeOffset>(10),
                    UpdatedAt = reader.IsDBNull(11) ? null : reader.GetFieldValue<DateTimeOffset>(11),
                    MultimodalEmbedding = reader.IsDBNull(12) ? null : ((Vector)reader.GetValue(12)).ToArray()
                };

                var similarity = reader.GetDouble(13);
                results.Add((inventory, similarity));
            }

            if (await reader.NextResultAsync(cancellationToken) && await reader.ReadAsync(cancellationToken))
            {
                totalCount = reader.GetInt32(0);
            }
        }

        return (results, totalCount);
    }
}
