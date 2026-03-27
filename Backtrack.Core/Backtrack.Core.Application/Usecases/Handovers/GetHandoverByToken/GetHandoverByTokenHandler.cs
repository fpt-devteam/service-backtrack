using System.Text;
using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Backtrack.Core.Application.Usecases.Handovers.GetHandoverByToken;

public sealed class GetHandoverByTokenHandler(
    IHandoverRepository handoverRepository,
    IOrgFormTemplateRepository orgFormTemplateRepository,
    IConfiguration configuration) : IRequestHandler<GetHandoverByTokenQuery, HandoverDetailResult>
{
    public async Task<HandoverDetailResult> Handle(GetHandoverByTokenQuery query, CancellationToken cancellationToken)
    {
        // Decode token to get handover ID
        var handoverId = DecodeHandoverToken(query.Token);
        if (handoverId == null)
        {
            throw new ValidationException(HandoverErrors.InvalidToken);
        }

        var handover = await handoverRepository.GetByIdWithExtensionAsync(handoverId.Value, cancellationToken)
            ?? throw new NotFoundException(HandoverErrors.NotFound);

        if (handover.Status == HandoverStatus.Expired)
        {
            throw new ValidationException(HandoverErrors.AlreadyExpired);
        }

        if (handover.Status == HandoverStatus.Confirmed)
        {
            throw new ValidationException(HandoverErrors.AlreadyConfirmed);
        }

        // Get form template if this is an org handover
        var formTemplate = handover.Type == HandoverType.Org && handover.OrgExtension != null
            ? await orgFormTemplateRepository.GetByOrgIdAsync(handover.OrgExtension.OrgId, cancellationToken)
            : null;

        return new HandoverDetailResult
        {
            Id = handover.Id,
            Type = handover.Type.ToString(),
            FinderPostId = handover.FinderPostId,
            OwnerPostId = handover.OwnerPostId,
            Status = handover.Status.ToString(),
            ConfirmedAt = handover.ConfirmedAt,
            ExpiresAt = handover.ExpiresAt,
            CreatedAt = handover.CreatedAt,
            OrgExtension = handover.OrgExtension != null ? new HandoverOrgExtensionResult
            {
                Id = handover.OrgExtension.Id,
                OrgId = handover.OrgExtension.OrgId,
                StaffId = handover.OrgExtension.StaffId,
                OwnerVerified = handover.OrgExtension.OwnerVerified,
                OwnerFormData = handover.OrgExtension.OwnerFormData,
                StaffConfirmedAt = handover.OrgExtension.StaffConfirmedAt,
                OwnerConfirmedAt = handover.OrgExtension.OwnerConfirmedAt
            } : null,
            FormTemplate = formTemplate?.Fields
        };
    }

    private Guid? DecodeHandoverToken(string token)
    {
        try
        {
            // Simple base64 encoding of handover ID for now
            // In production, use signed JWT or encrypted token
            var secret = configuration["Handover:TokenSecret"] ?? "default-secret";
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = decoded.Split(':');
            if (parts.Length != 2)
                return null;

            // Verify signature (simple HMAC for now)
            var handoverIdStr = parts[0];
            var signature = parts[1];

            using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var expectedSignature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(handoverIdStr)));

            if (signature != expectedSignature)
                return null;

            return Guid.Parse(handoverIdStr);
        }
        catch
        {
            return null;
        }
    }
}
