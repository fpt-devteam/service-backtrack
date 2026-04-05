using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.UpdateInventoryItem;

public sealed class UpdateInventoryItemHandler(
    IOrganizationInventoryRepository inventoryRepository,
    IOrganizationRepository organizationRepository,
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

        if (command.FinderContact != null)
        {
            var org = await organizationRepository.GetByIdAsync(command.OrgId)
                ?? throw new NotFoundException(OrganizationErrors.NotFound);

            ValidateFinderContactRequiredFields(org.RequiredFinderContractFields, command.FinderContact);

            if (inventory.FinderContact != null)
            {
                inventory.FinderContact.Name = command.FinderContact.Name;
                inventory.FinderContact.Email = command.FinderContact.Email;
                inventory.FinderContact.Phone = command.FinderContact.Phone;
                inventory.FinderContact.NationalId = command.FinderContact.NationalId;
                inventory.FinderContact.OrgMemberId = command.FinderContact.OrgMemberId;
                inventory.FinderContact.UpdatedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                inventory.FinderContact = new FinderContact
                {
                    Id = Guid.NewGuid(),
                    InventoryId = inventory.Id,
                    Name = command.FinderContact.Name,
                    Email = command.FinderContact.Email,
                    Phone = command.FinderContact.Phone,
                    NationalId = command.FinderContact.NationalId,
                    OrgMemberId = command.FinderContact.OrgMemberId,
                };
            }
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
            CreatedAt = inventory.CreatedAt,
            ReceiverStaffId = inventory.ReceiverStaffId,
            HandoverStaffId = inventory.HandoverStaffId,
            FinderContact = inventory.FinderContact == null ? null : new FinderContactResult
            {
                Id = inventory.FinderContact.Id,
                Name = inventory.FinderContact.Name,
                Email = inventory.FinderContact.Email,
                Phone = inventory.FinderContact.Phone,
                NationalId = inventory.FinderContact.NationalId,
                OrgMemberId = inventory.FinderContact.OrgMemberId,
            }
        };
    }

    private static void ValidateFinderContactRequiredFields(
        List<OrgContractField> requiredFields,
        FinderContactInfo contact)
    {
        foreach (var field in requiredFields)
        {
            var missing = field switch
            {
                OrgContractField.Email => string.IsNullOrWhiteSpace(contact.Email),
                OrgContractField.Phone => string.IsNullOrWhiteSpace(contact.Phone),
                OrgContractField.NationalId => string.IsNullOrWhiteSpace(contact.NationalId),
                OrgContractField.OrgMemberId => string.IsNullOrWhiteSpace(contact.OrgMemberId),
                _ => false
            };

            if (missing)
                throw new ValidationException(OrganizationInventoryErrors.MissingRequiredOrgContractField(field.ToString()));
        }
    }
}
