using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Usecases.Posts.CreatePost;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Data.Seeders;

public static class PostSeeder
{
    // ──────────────────────────────────────────────
    // Locations
    // ──────────────────────────────────────────────

    private static readonly GeoPoint FptUniversityLocation = new(10.8417, 106.8100);
    private const string FptUniversityAddress = "Lô E2a-7, Đường D1, Khu Công nghệ cao, Phường Tăng Nhơn Phú A, TP Thủ Đức";

    // ──────────────────────────────────────────────
    // Post data records
    // ──────────────────────────────────────────────

    private static readonly CreatePostCommand NgoDucBinhLostStudentCard = new()
    {
        PostType        = "Lost",
        Category        = "Cards",
        SubcategoryCode = "student_card",
        ImageUrls       = [],
        Location        = FptUniversityLocation,
        DisplayAddress  = FptUniversityAddress,
        EventTime       = DateTimeOffset.UtcNow.AddDays(-3),
        CardDetail      = new CardDetailInput
        {
            HolderName           = "NGÔ ĐỨC BÌNH",
            HolderNameNormalized = "ngo duc binh",
            IssuingAuthority     = "FPT Polytechnic",
            AdditionalDetails    = "Chuyên ngành thiết kế đồ họa",
        },
    };

    private static readonly CreatePostCommand LongFptFoundStudentCard = new()
    {
        PostType        = "Found",
        Category        = "Cards",
        SubcategoryCode = "student_card",
        ImageUrls       =
        [
            "https://firebasestorage.googleapis.com/v0/b/backtrack-sep490.firebasestorage.app/o/posts%2Fimages%2Ffpt_student_card.jpg?alt=media&token=3b1b2d39-a758-43d5-9325-72e643d99849",
        ],
        Location       = FptUniversityLocation,
        DisplayAddress = FptUniversityAddress,
        EventTime      = DateTimeOffset.UtcNow.AddDays(-2),
        CardDetail     = new CardDetailInput
        {
            CardNumber           = "PS27513",
            HolderName           = "NGÔ ĐỨC BÌNH",
            HolderNameNormalized = "ngo duc binh",
            ExpiryDate           = new DateOnly(2024, 12, 1),
            IssuingAuthority     = "FPT Polytechnic",
            OcrText              =
                "FPT Education\nFPT POLYTECHNIC\nTrường Cao đẳng FPT Polytechnic\nTHẺ SINH VIÊN\nStudent Card\n(THẺ TẠM)\nHọ và tên/Name\nNGÔ ĐỨC BÌNH\nChuyên ngành/Specialized\nTHIẾT KẾ ĐỒ HỌA\nMSSV/Student ID\nPS27513\nGiá trị đến/Valid to\n12/2024\ncaodang.fpt.edu.vn",
        },
    };

    // ──────────────────────────────────────────────
    // Entry point
    // ──────────────────────────────────────────────

    public static async Task SeedAsync(
        ApplicationDbContext db,
        ISender mediator,
        ILogger logger,
        CancellationToken ct = default)
    {
        await SeedStudentCardMatchingScenarioAsync(db, mediator, logger, ct);
    }

    // ──────────────────────────────────────────────
    // Scenarios
    // ──────────────────────────────────────────────

    // Scenario: NgoDucBinh lost their student card; LongFpt found it.
    // Used to demonstrate the matching flow end-to-end.
    private static async Task SeedStudentCardMatchingScenarioAsync(
        ApplicationDbContext db, ISender mediator, ILogger logger, CancellationToken ct)
    {
        await SeedPostAsync(db, mediator, logger, UserSeeder.NgoDucBinh.Email, NgoDucBinhLostStudentCard, ct);
        await SeedPostAsync(db, mediator, logger, UserSeeder.LongFpt.Email,    LongFptFoundStudentCard,   ct);
    }

    // ──────────────────────────────────────────────
    // Reusable seed method
    // ──────────────────────────────────────────────

    private static async Task SeedPostAsync(
        ApplicationDbContext db,
        ISender mediator,
        ILogger logger,
        string authorEmail,
        CreatePostCommand postData,
        CancellationToken ct)
    {
        var user = await db.Set<User>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == authorEmail, ct);

        if (user is null)
        {
            logger.LogWarning("PostSeeder: user {Email} not found — skipping post.", authorEmail);
            return;
        }

        if (!Enum.TryParse<PostType>(postData.PostType, ignoreCase: true, out var postType))
        {
            logger.LogWarning("PostSeeder: invalid PostType '{PostType}' for {Email} — skipping.", postData.PostType, authorEmail);
            return;
        }

        var subcategory = await db.Set<Subcategory>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Code == postData.SubcategoryCode, ct);

        if (subcategory is not null)
        {
            var alreadyExists = await db.Set<Post>()
                .AnyAsync(p => p.AuthorId == user.Id && p.SubcategoryId == subcategory.Id, ct);

            if (alreadyExists)
            {
                logger.LogInformation(
                    "PostSeeder: {PostType} post for {Email} in subcategory '{Subcategory}' already exists — skipping.",
                    postType, authorEmail, postData.SubcategoryCode);
                return;
            }
        }

        try
        {
            await mediator.Send(postData with { AuthorId = user.Id }, ct);
            logger.LogInformation("PostSeeder: created {PostType} post for {Email}.", postType, authorEmail);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "PostSeeder: failed to create {PostType} post for {Email} — skipping.", postType, authorEmail);
        }
    }
}
