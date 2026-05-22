using Backtrack.Core.Application.Usecases.ReturnReport.FinderDeliveredC2CReturnReport;
using Backtrack.Core.Application.Usecases.ReturnReport.InitiateC2CReturnReport;
using Backtrack.Core.Application.Usecases.ReturnReport.OwnerConfirmC2CReturnReport;
using Backtrack.Core.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Data.Seeders;

public static class HandoverSeeder
{
    public static async Task SeedOngoingAsync(
        ApplicationDbContext db,
        ISender mediator,
        ILogger logger,
        string finderEmail,
        string ownerEmail,
        Guid finderPostId,
        Guid ownerPostId,
        CancellationToken ct = default)
    {
        var (finder, owner) = await ResolveUsersAsync(db, logger, finderEmail, ownerEmail, ct);
        if (finder is null || owner is null) return;

        if (await ExistsAsync(db, finderPostId, ownerPostId, ct))
        {
            logger.LogInformation(
                "HandoverSeeder: handover for finder post {FinderPostId} / owner post {OwnerPostId} already exists — skipping.",
                finderPostId, ownerPostId);
            return;
        }

        try
        {
            await mediator.Send(new InitiateC2CReturnReportCommand
            {
                InitiatorId = finder.Id,
                FinderPostId = finderPostId,
                OwnerPostId = ownerPostId,
            }, ct);

            logger.LogInformation(
                "HandoverSeeder: created Ongoing handover — finder {Finder}, owner {Owner}.",
                finderEmail, ownerEmail);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "HandoverSeeder: failed to create handover for {Finder} / {Owner} — skipping.",
                finderEmail, ownerEmail);
        }
    }

    public static async Task SeedDeliveredAsync(
        ApplicationDbContext db,
        ISender mediator,
        ILogger logger,
        string finderEmail,
        string ownerEmail,
        Guid finderPostId,
        Guid ownerPostId,
        CancellationToken ct = default)
    {
        var (finder, owner) = await ResolveUsersAsync(db, logger, finderEmail, ownerEmail, ct);
        if (finder is null || owner is null) return;

        var existing = await db.Set<C2CReturnReport>()
            .FirstOrDefaultAsync(r => r.FinderPostId == finderPostId && r.OwnerPostId == ownerPostId, ct);

        Guid handoverId;

        if (existing is not null)
        {
            logger.LogInformation(
                "HandoverSeeder: handover {Id} already exists (status {Status}) — skipping initiate.",
                existing.Id, existing.Status);
            handoverId = existing.Id;
        }
        else
        {
            try
            {
                var result = await mediator.Send(new InitiateC2CReturnReportCommand
                {
                    InitiatorId = finder.Id,
                    FinderPostId = finderPostId,
                    OwnerPostId = ownerPostId,
                }, ct);

                handoverId = result.Id;
                logger.LogInformation(
                    "HandoverSeeder: created Ongoing handover {Id} — finder {Finder}, owner {Owner}.",
                    handoverId, finderEmail, ownerEmail);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "HandoverSeeder: failed to initiate handover for {Finder} / {Owner} — skipping.",
                    finderEmail, ownerEmail);
                return;
            }
        }

        // Mark as Delivered if not already past Ongoing
        var report = await db.Set<C2CReturnReport>().FindAsync([handoverId], ct);
        if (report is null || report.Status != Domain.Constants.C2CReturnReportStatus.Ongoing)
        {
            logger.LogInformation(
                "HandoverSeeder: handover {Id} is not Ongoing — skipping deliver step.", handoverId);
            return;
        }

        try
        {
            await mediator.Send(new FinderDeliveredC2CReturnReportCommand
            {
                UserId = finder.Id,
                C2CReturnReportId = handoverId,
                EvidenceImageUrls = ["https://placehold.co/600x400.jpg", "https://placehold.co/600x400.jpg"],
            }, ct);

            logger.LogInformation(
                "HandoverSeeder: handover {Id} marked as Delivered.", handoverId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "HandoverSeeder: failed to mark handover {Id} as Delivered — skipping.", handoverId);
        }
    }

    public static async Task SeedConfirmedAsync(
        ApplicationDbContext db,
        ISender mediator,
        ILogger logger,
        string finderEmail,
        string ownerEmail,
        Guid finderPostId,
        Guid ownerPostId,
        CancellationToken ct = default)
    {
        var (finder, owner) = await ResolveUsersAsync(db, logger, finderEmail, ownerEmail, ct);
        if (finder is null || owner is null) return;

        var existing = await db.Set<C2CReturnReport>()
            .FirstOrDefaultAsync(r => r.FinderPostId == finderPostId && r.OwnerPostId == ownerPostId, ct);

        Guid handoverId;

        if (existing is not null)
        {
            logger.LogInformation(
                "HandoverSeeder: handover {Id} already exists (status {Status}) — skipping initiate.",
                existing.Id, existing.Status);
            handoverId = existing.Id;
        }
        else
        {
            try
            {
                // Owner (Thang) initiates
                var result = await mediator.Send(new InitiateC2CReturnReportCommand
                {
                    InitiatorId = owner.Id,
                    FinderPostId = finderPostId,
                    OwnerPostId = ownerPostId,
                }, ct);

                handoverId = result.Id;
                logger.LogInformation(
                    "HandoverSeeder: created Ongoing handover {Id} — finder {Finder}, owner {Owner}.",
                    handoverId, finderEmail, ownerEmail);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "HandoverSeeder: failed to initiate handover for {Finder} / {Owner} — skipping.",
                    finderEmail, ownerEmail);
                return;
            }
        }

        var report = await db.Set<C2CReturnReport>().FindAsync([handoverId], ct);
        if (report is null) return;

        if (report.Status == Domain.Constants.C2CReturnReportStatus.Ongoing)
        {
            // Finder (Long) delivers
            try
            {
                await mediator.Send(new FinderDeliveredC2CReturnReportCommand
                {
                    UserId = finder.Id,
                    C2CReturnReportId = handoverId,
                    EvidenceImageUrls = ["https://placehold.co/600x400.jpg", "https://placehold.co/600x400.jpg"],
                }, ct);

                logger.LogInformation("HandoverSeeder: handover {Id} marked as Delivered.", handoverId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "HandoverSeeder: failed to mark handover {Id} as Delivered — skipping.", handoverId);
                return;
            }

            await db.Entry(report).ReloadAsync(ct);
        }

        if (report.Status != Domain.Constants.C2CReturnReportStatus.Delivered)
        {
            logger.LogInformation(
                "HandoverSeeder: handover {Id} is not Delivered — skipping confirm step.", handoverId);
            return;
        }

        // Owner (Thang) confirms
        try
        {
            await mediator.Send(new OwnerConfirmC2CReturnReportCommand
            {
                UserId = owner.Id,
                C2CReturnReportId = handoverId,
            }, ct);

            logger.LogInformation("HandoverSeeder: handover {Id} marked as Confirmed.", handoverId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "HandoverSeeder: failed to confirm handover {Id} — skipping.", handoverId);
        }
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private static async Task<(User? finder, User? owner)> ResolveUsersAsync(
        ApplicationDbContext db, ILogger logger,
        string finderEmail, string ownerEmail, CancellationToken ct)
    {
        var finder = await db.Set<User>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == finderEmail, ct);

        if (finder is null)
        {
            logger.LogWarning("HandoverSeeder: finder {Email} not found — skipping.", finderEmail);
            return (null, null);
        }

        var owner = await db.Set<User>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == ownerEmail, ct);

        if (owner is null)
        {
            logger.LogWarning("HandoverSeeder: owner {Email} not found — skipping.", ownerEmail);
            return (null, null);
        }

        return (finder, owner);
    }

    private static Task<bool> ExistsAsync(
        ApplicationDbContext db, Guid finderPostId, Guid ownerPostId, CancellationToken ct) =>
        db.Set<C2CReturnReport>()
            .AnyAsync(r => r.FinderPostId == finderPostId && r.OwnerPostId == ownerPostId, ct);
}
