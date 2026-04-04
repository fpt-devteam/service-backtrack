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
    IOrganizationRepository organizationRepository,
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

        // Validate finder contact required fields defined by the organization
        var org = await organizationRepository.GetByIdAsync(command.OrgId)
            ?? throw new NotFoundException(OrganizationErrors.NotFound);

        ValidateFinderContactRequiredFields(org.RequiredFinderContractFields, command.FinderContact);

        // Generate embedding synchronously
        var contentForEmbedding = $@"Item: {command.ItemName}
Description: {command.Description}";

        if (!string.IsNullOrWhiteSpace(command.DistinctiveMarks))
        {
            contentForEmbedding += $"\nDistinctive marks: {command.DistinctiveMarks}";
        }

        contentForEmbedding += $"\n\nThis item is {command.ItemName.ToLower()}.";

        var embedding = await embeddingService.GenerateMultimodalEmbeddingAsync(contentForEmbedding, null, null, cancellationToken);

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
            MultimodalEmbedding = embedding,
            Status = OrganizationInventoryStatus.InStorage,
            LoggedAt = DateTimeOffset.UtcNow,
            ReceiverStaffId = command.UserId,
        };

        var finderContact = new FinderContact
        {
            Id = Guid.NewGuid(),
            InventoryId = inventory.Id,
            Name = command.FinderContact.Name,
            Email = command.FinderContact.Email,
            Phone = command.FinderContact.Phone,
            NationalId = command.FinderContact.NationalId,
            OrgMemberId = command.FinderContact.OrgMemberId,
        };

        await inventoryRepository.CreateAsync(inventory);
        inventory.FinderContact = finderContact;
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
            FinderContact = new FinderContactResult
            {
                Id = finderContact.Id,
                Name = finderContact.Name,
                Email = finderContact.Email,
                Phone = finderContact.Phone,
                NationalId = finderContact.NationalId,
                OrgMemberId = finderContact.OrgMemberId,
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
