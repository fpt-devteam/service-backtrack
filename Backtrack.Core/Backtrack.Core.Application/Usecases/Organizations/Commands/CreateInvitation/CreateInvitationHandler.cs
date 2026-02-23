using System.Security.Cryptography;
using Backtrack.Core.Application.Events;
using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Messaging;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Application.Usecases.Organizations.Commands.CreateInvitation;

public sealed class CreateInvitationHandler : IRequestHandler<CreateInvitationCommand, CreateInvitationResult>
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IJoinInvitationRepository _invitationRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<CreateInvitationHandler> _logger;
    private const int ExpireDays = 3;

    public CreateInvitationHandler(
        IMembershipRepository membershipRepository,
        IJoinInvitationRepository invitationRepository,
        IOrganizationRepository organizationRepository,
        IEventPublisher eventPublisher,
        ILogger<CreateInvitationHandler> logger)
    {
        _membershipRepository = membershipRepository;
        _invitationRepository = invitationRepository;
        _organizationRepository = organizationRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<CreateInvitationResult> Handle(CreateInvitationCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.UserId))
        {
            throw new InvalidOperationException("UserId is not provided when initializing CreateInvitationCommand.");
        }

        // Parse role
        if (!Enum.TryParse<MembershipRole>(command.Role, ignoreCase: true, out var role))
        {
            throw new ValidationException(MembershipErrors.InvalidMembershipRole);
        }

        // Verify caller is OrgAdmin
        var callerMembership = await _membershipRepository.GetByOrgAndUserAsync(command.OrgId, command.UserId, cancellationToken);
        if (callerMembership is null)
        {
            throw new ForbiddenException(MembershipErrors.NotAMember);
        }
        if (callerMembership.Role != MembershipRole.OrgAdmin)
        {
            throw new ForbiddenException(MembershipErrors.InsufficientRole);
        }

        // Check no pending invitation already exists for this email+org
        var existingInvitation = await _invitationRepository.GetPendingByEmailAndOrgAsync(command.Email, command.OrgId, cancellationToken);
        if (existingInvitation is not null)
        {
            throw new ConflictException(InvitationErrors.AlreadyInvited);
        }

        // Check email is not already a member of the org
        var existingMemberships = await _membershipRepository.GetByOrgAndUserEmailAsync(command.OrgId, command.Email, cancellationToken);
        if (existingMemberships is not null)
        {
            throw new ConflictException(MembershipErrors.AlreadyAMember);
        }

        // Load organization for event data
        var organization = await _organizationRepository.GetByIdAsync(command.OrgId);
        if (organization is null)
        {
            throw new NotFoundException(OrganizationErrors.NotFound);
        }

        // Generate secure random hash code
        var hashCode = GenerateSecureToken();

        var invitation = new JoinInvitation
        {
            Id = Guid.NewGuid(),
            OrganizationId = command.OrgId,
            Email = command.Email,
            Role = role,
            HashCode = hashCode,
            ExpiredTime = DateTimeOffset.UtcNow.AddDays(ExpireDays),
            Status = InvitationStatus.Pending,
            InvitedBy = command.UserId,
        };
        //log hashcode
        _logger.LogInformation("Generated invitation hash code: {HashCode} for email: {Email}", hashCode, command.Email);


        await _invitationRepository.CreateAsync(invitation);
        await _invitationRepository.SaveChangesAsync();

        // Publish integration event for notification service
        await _eventPublisher.PublishInvitationCreatedAsync(new InvitationCreatedIntegrationEvent
        {
            InvitationId = invitation.Id,
            Email = invitation.Email,
            OrganizationName = organization.Name,
            InviterName = command.UserName,
            Role = invitation.Role.ToString(),
            HashCode = invitation.HashCode,
            ExpiredTime = invitation.ExpiredTime,
            EventTimestamp = DateTimeOffset.UtcNow,
        });

        return new CreateInvitationResult
        {
            Id = invitation.Id,
            Email = invitation.Email,
            Role = invitation.Role.ToString(),
            Status = invitation.Status.ToString(),
            ExpiredTime = invitation.ExpiredTime,
            CreatedAt = invitation.CreatedAt,
        };
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
