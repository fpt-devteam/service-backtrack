using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.CreateInventoryItem;

public sealed class CreateInventoryItemHandler(
    IOrganizationInventoryRepository inventoryRepository,
    IMembershipRepository membershipRepository,
    IEmbeddingService embeddingService) : IRequestHandler<CreateInventoryItemCommand, OrganizationInventoryResult>
{
    public async Task<OrganizationInventoryResult> Handle(CreateInventoryItemCommand command, CancellationToken cancellationToken)
    {
        // Check if user is a member of the organization
        var membership = await membershipRepository.GetByOrgAndUserAsync(command.OrgId, command.UserId, cancellationToken);
        if (membership == null)
        {
            throw new ForbiddenException(MembershipErrors.NotAMember);
        }

        // Generate embedding synchronously
        var contentForEmbedding = $@"Item: {command.ItemName}
Description: {command.Description}";

        if (!string.IsNullOrWhiteSpace(command.DistinctiveMarks))
        {
            contentForEmbedding += $"\nDistinctive marks: {command.DistinctiveMarks}";
        }

        contentForEmbedding += $"\n\nThis item is {command.ItemName.ToLower()}.";

        var embedding = await embeddingService.GenerateEmbeddingAsync(contentForEmbedding, cancellationToken);

        var inventory = new OrganizationInventory
        {
            Id = Guid.NewGuid(),
            OrgId = command.OrgId,
            LoggedById = command.UserId,
            ItemName = command.ItemName,
            Description = command.Description,
            DistinctiveMarks = command.DistinctiveMarks,
            ImageUrls = command.ImageUrls,
            StorageLocation = command.StorageLocation,
            ContentEmbedding = embedding,
            Status = OrganizationInventoryStatus.InStorage,
            LoggedAt = DateTimeOffset.UtcNow,
        };

        await inventoryRepository.CreateAsync(inventory);
        await inventoryRepository.SaveChangesAsync();

        return new OrganizationInventoryResult
        {
            Id = inventory.Id,
            OrgId = inventory.OrgId,
            LoggedById = inventory.LoggedById,
            ItemName = inventory.ItemName,
            Description = inventory.Description,
            DistinctiveMarks = inventory.DistinctiveMarks,
            ImageUrls = inventory.ImageUrls,
            StorageLocation = inventory.StorageLocation,
            Status = inventory.Status.ToString(),
            LoggedAt = inventory.LoggedAt,
            CreatedAt = inventory.CreatedAt
        };
    }
}
