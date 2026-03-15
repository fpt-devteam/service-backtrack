using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.UpdateInventoryItem;

public sealed class UpdateInventoryItemHandler(
    IOrganizationInventoryRepository inventoryRepository,
    IMembershipRepository membershipRepository,
    IEmbeddingService embeddingService) : IRequestHandler<UpdateInventoryItemCommand, OrganizationInventoryResult>
{
    public async Task<OrganizationInventoryResult> Handle(UpdateInventoryItemCommand command, CancellationToken cancellationToken)
    {
        // Authorization check
        var membership = await membershipRepository.GetByOrgAndUserAsync(command.OrgId, command.UserId, cancellationToken);
        if (membership == null)
        {
            throw new ForbiddenException(MembershipErrors.NotAMember);
        }

        var inventory = await inventoryRepository.GetByIdAsync(command.Id, true)
            ?? throw new NotFoundException(PostErrors.NotFound);

        if (inventory.OrgId != command.OrgId)
        {
            throw new ForbiddenException(PostErrors.Forbidden);
        }

        bool needsReEmbedding = false;

        if (command.ItemName != null && inventory.ItemName != command.ItemName)
        {
            inventory.ItemName = command.ItemName;
            needsReEmbedding = true;
        }

        if (command.Description != null && inventory.Description != command.Description)
        {
            inventory.Description = command.Description;
            needsReEmbedding = true;
        }

        if (command.DistinctiveMarks != null && inventory.DistinctiveMarks != command.DistinctiveMarks)
        {
            inventory.DistinctiveMarks = command.DistinctiveMarks;
            needsReEmbedding = true;
        }

        if (command.ImageUrls != null)
        {
            inventory.ImageUrls = command.ImageUrls;
        }

        if (command.StorageLocation != null)
        {
            inventory.StorageLocation = command.StorageLocation;
        }

        if (command.Status != null && Enum.TryParse<OrganizationInventoryStatus>(command.Status, true, out var status))
        {
            inventory.Status = status;
        }

        if (needsReEmbedding)
        {
            var contentForEmbedding = $@"Item: {inventory.ItemName}
Description: {inventory.Description}";

            if (!string.IsNullOrWhiteSpace(inventory.DistinctiveMarks))
            {
                contentForEmbedding += $"\nDistinctive marks: {inventory.DistinctiveMarks}";
            }

            contentForEmbedding += $"\n\nThis item is {inventory.ItemName.ToLower()}.";
            inventory.MultimodalEmbedding = await embeddingService.GenerateMultimodalEmbeddingAsync(contentForEmbedding, null, null, cancellationToken);
        }

        inventory.UpdatedAt = DateTimeOffset.UtcNow;
        inventoryRepository.Update(inventory);
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
