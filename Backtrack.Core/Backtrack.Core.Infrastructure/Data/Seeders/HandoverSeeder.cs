using Backtrack.Core.Application.Usecases.ReturnReport.InitiateC2CReturnReport;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Data.Seeders;

public static class HandoverSeeder
{
    public static async Task SeedDraftAsync(
        ApplicationDbContext db,
        ISender mediator,
        ILogger logger,
        string finderEmail,
        string ownerEmail,
        Guid finderPostId,
        Guid ownerPostId,
        CancellationToken ct = default)
    {
        var finder = await db.Set<User>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == finderEmail, ct);

        if (finder is null)
        {
            logger.LogWarning("HandoverSeeder: finder user {Email} not found — skipping.", finderEmail);
            return;
        }

        var owner = await db.Set<User>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == ownerEmail, ct);

        if (owner is null)
        {
            logger.LogWarning("HandoverSeeder: owner user {Email} not found — skipping.", ownerEmail);
            return;
        }

        var alreadyExists = await db.Set<C2CReturnReport>()
            .AnyAsync(r => r.FinderPostId == finderPostId && r.OwnerPostId == ownerPostId, ct);

        if (alreadyExists)
        {
            logger.LogInformation(
                "HandoverSeeder: draft handover for finder post {FinderPostId} / owner post {OwnerPostId} already exists — skipping.",
                finderPostId, ownerPostId);
            return;
        }

        try
        {
            var command = new InitiateC2CReturnReportCommand
            {
                InitiatorId  = finder.Id,
                FinderPostId = finderPostId,
                OwnerPostId  = ownerPostId,
                OwnerId      = owner.Id,
            };

            await mediator.Send(command, ct);

            logger.LogInformation(
                "HandoverSeeder: created draft C2C return report — finder {Finder}, owner {Owner}.",
                finderEmail, ownerEmail);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "HandoverSeeder: failed to create draft handover for {Finder} / {Owner} — skipping.",
                finderEmail, ownerEmail);
        }
    }
}
