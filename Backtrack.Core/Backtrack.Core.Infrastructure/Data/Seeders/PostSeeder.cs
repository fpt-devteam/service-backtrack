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

    private static readonly GeoPoint TonThatTungLocation = new(10.7601, 106.6846);
    private const string TonThatTungAddress = "22 Tôn Thất Tùng, Phường Phạm Ngũ Lão, Quận 1, TP Hồ Chí Minh";

    private static readonly GeoPoint BenThanhLocation = new(10.7726, 106.6981);
    private const string BenThanhAddress = "Chợ Bến Thành, Phường Bến Thành, Quận 1, TP Hồ Chí Minh";

    private static readonly GeoPoint VnuhcmLocation = new(10.8700, 106.8033);
    private const string VnuhcmAddress = "Nhà Văn hóa Sinh viên, Đại học Quốc gia TP.HCM, Đường Võ Trường Toản, Phường Linh Trung, TP. Thủ Đức";

    // ──────────────────────────────────────────────
    // Post data records
    // ──────────────────────────────────────────────

    private static readonly CreatePostCommand NgoDucBinhLostStudentCard = new()
    {
        PostType        = "Lost",
        PostTitle       = "Thẻ sinh viên FPT Polytechnic",
        Category        = "Cards",
        SubcategoryCode = "student_card",
        ImageUrls       = [],
        Location        = FptUniversityLocation,
        DisplayAddress  = FptUniversityAddress,
        EventTime       = DateTimeOffset.UtcNow.AddDays(-3),
        CardDetail      = new CardDetailDto
        {
            ItemName              = "Thẻ sinh viên FPT Polytechnic",
            HolderName           = "NGÔ ĐỨC BÌNH",
            HolderNameNormalized = "ngo duc binh",
            IssuingAuthority     = "FPT Polytechnic",
            AdditionalDetails    = "Chuyên ngành thiết kế đồ họa",
        },
    };

    private static readonly CreatePostCommand LongFptFoundStudentCard = new()
    {
        PostType        = "Found",
        PostTitle       = "Thẻ sinh viên FPT",
        Category        = "Cards",
        SubcategoryCode = "student_card",
        ImageUrls       =
        [
            "https://firebasestorage.googleapis.com/v0/b/backtrack-sep490.firebasestorage.app/o/posts%2Fimages%2Ffpt_student_card.jpg?alt=media&token=3b1b2d39-a758-43d5-9325-72e643d99849",
        ],
        Location       = FptUniversityLocation,
        DisplayAddress = FptUniversityAddress,
        EventTime      = DateTimeOffset.UtcNow.AddDays(-2),
        CardDetail     = new CardDetailDto
        {
            ItemName             = "Thẻ sinh viên FPT",
            CardNumber           = "PS27513",
            HolderName           = "NGÔ ĐỨC BÌNH",
            HolderNameNormalized = "ngo duc binh",
            ExpiryDate           = new DateOnly(2024, 12, 1),
            IssuingAuthority     = "FPT Polytechnic",
            OcrText              =
                "FPT Education\nFPT POLYTECHNIC\nTrường Cao đẳng FPT Polytechnic\nTHẺ SINH VIÊN\nStudent Card\n(THẺ TẠM)\nHọ và tên/Name\nNGÔ ĐỨC BÌNH\nChuyên ngành/Specialized\nTHIẾT KẾ ĐỒ HỌA\nMSSV/Student ID\nPS27513\nGiá trị đến/Valid to\n12/2024\ncaodang.fpt.edu.vn",
        },
    };

    private static readonly CreatePostCommand LongFptFoundWallet = new()
    {
        PostType        = "Found",
        PostTitle       = "Ví da đen",
        Category        = "PersonalBelongings",
        SubcategoryCode = "wallets",
        ImageUrls       =
        [
            "https://firebasestorage.googleapis.com/v0/b/backtrack-sep490.firebasestorage.app/o/posts%2Fimages%2Fvi_tonthattung.png?alt=media&token=2ab89a31-343b-46bc-b88e-d598b85b9cb0",
        ],
        Location       = TonThatTungLocation,
        DisplayAddress = TonThatTungAddress,
        EventTime      = DateTimeOffset.UtcNow.AddDays(-1),
        PersonalBelongingDetail = new PersonalBelongingDetailDto
        {
            ItemName         = "Ví da đen",
            Color            = "black",
            Material         = "leather",
            Size             = "small",
            DistinctiveMarks = "quilted pattern, gold-colored interlocking circle logo",
            AiDescription    = "Black leather wallet with a quilted pattern. The wallet features a gold-colored interlocking circle logo on the front flap. It has a zipper closure and multiple card slots.",
        },
    };

    private static readonly CreatePostCommand CatLinhLostWallet = new()
    {
        PostType        = "Lost",
        PostTitle       = "Card Wallet",
        Category        = "PersonalBelongings",
        SubcategoryCode = "wallets",
        ImageUrls       =
        [
            "https://firebasestorage.googleapis.com/v0/b/backtrack-sep490.firebasestorage.app/o/posts%2Fimages%2Fvi_lost_chobenthanh.jpeg?alt=media&token=1b525f5f-9cfb-459a-9638-170862399559",
        ],
        Location       = BenThanhLocation,
        DisplayAddress = BenThanhAddress,
        EventTime      = DateTimeOffset.UtcNow.AddDays(-2),
        PersonalBelongingDetail = new PersonalBelongingDetailDto
        {
            ItemName         = "Card Wallet",
            Color            = "black",
            Material         = "leather",
            Size             = "small",
            DistinctiveMarks = "quilted pattern, gold clasp",
            AiDescription    = "This is a small black leather wallet. It features a quilted pattern and a gold clasp.",
        },
    };

    private static readonly CreatePostCommand LongFptFoundCharger = new()
    {
        PostType        = "Found",
        PostTitle       = "Found Apple MagSafe Charger",
        Category        = "Electronics",
        SubcategoryCode = "charger_adapter",
        ImageUrls       =
        [
            "https://firebasestorage.googleapis.com/v0/b/backtrack-sep490.firebasestorage.app/o/posts%2Fimages%2Fwhite_macbook_charger_2.jpg?alt=media&token=a96f55d8-7976-4a05-8820-b0d79927b11b",
        ],
        Location       = VnuhcmLocation,
        DisplayAddress = VnuhcmAddress,
        EventTime      = DateTimeOffset.UtcNow.AddDays(-1),
        ElectronicDetail = new ElectronicDetailDto
        {
            ItemName               = "White Apple MagSafe Power Adapter",
            Brand                  = "Apple",
            Model                  = "MagSafe Power Adapter",
            Color                  = "White",
            HasCase                = false,
            DistinguishingFeatures = "The power cable is discolored, appearing yellowish-brown, and the MagSafe connector has a distinct blue plastic piece attached.",
            AiDescription          = "This is a white Apple MagSafe power adapter, identifiable by the Apple logo on its surface and its characteristic L-shaped MagSafe connector. The power cable is notably discolored to a yellowish-brown, and the MagSafe connector features a unique blue plastic attachment.",
        },
    };

    private static readonly CreatePostCommand ThangFptLostCharger = new()
    {
        PostType        = "Lost",
        PostTitle       = "Lost Apple MagSafe Charger",
        Category        = "Electronics",
        SubcategoryCode = "charger_adapter",
        ImageUrls       = [],
        Location       = VnuhcmLocation,
        DisplayAddress = VnuhcmAddress,
        EventTime      = DateTimeOffset.UtcNow.AddDays(-1),
        ElectronicDetail = new ElectronicDetailDto
        {
            ItemName               = "White Apple MagSafe Power Adapter",
            Brand                  = "Apple",
            Model                  = "MagSafe Power Adapter",
            Color                  = "White",
            HasCase                = false,
            DistinguishingFeatures = "Cable has yellowed over time, MagSafe connector end has a small blue plastic cap that I added myself to protect the pins.",
            AiDescription          = "White Apple MagSafe power adapter with a yellowed power cable and a custom blue plastic cap on the MagSafe connector. Has an Apple logo on the brick.",
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
        await SeedWalletDeliveredScenarioAsync(db, mediator, logger, ct);
        await SeedChargerConfirmedScenarioAsync(db, mediator, logger, ct);
    }

    // ──────────────────────────────────────────────
    // Scenarios
    // ──────────────────────────────────────────────

    // Scenario: NgoDucBinh lost their student card; LongFpt found it.
    // Used to demonstrate the matching flow end-to-end.
    private static async Task SeedStudentCardMatchingScenarioAsync(
        ApplicationDbContext db, ISender mediator, ILogger logger, CancellationToken ct)
    {
        var ownerPostId  = await SeedPostAsync(db, mediator, logger, UserSeeder.NgoDucBinh.Email, NgoDucBinhLostStudentCard, ct);
        var finderPostId = await SeedPostAsync(db, mediator, logger, UserSeeder.LongFpt.Email,    LongFptFoundStudentCard,   ct);

        if (finderPostId.HasValue && ownerPostId.HasValue)
        {
            await HandoverSeeder.SeedOngoingAsync(
                db, mediator, logger,
                finderEmail:  UserSeeder.LongFpt.Email,
                ownerEmail:   UserSeeder.NgoDucBinh.Email,
                finderPostId: finderPostId.Value,
                ownerPostId:  ownerPostId.Value,
                ct);
        }
    }

    // Scenario: LongFpt found CatLinh's wallet near Ben Thanh; Long initiates and delivers.
    private static async Task SeedWalletDeliveredScenarioAsync(
        ApplicationDbContext db, ISender mediator, ILogger logger, CancellationToken ct)
    {
        var ownerPostId  = await SeedPostAsync(db, mediator, logger, UserSeeder.CatLinh.Email,  CatLinhLostWallet,    ct);
        var finderPostId = await SeedPostAsync(db, mediator, logger, UserSeeder.LongFpt.Email,  LongFptFoundWallet,   ct);

        if (finderPostId.HasValue && ownerPostId.HasValue)
        {
            await HandoverSeeder.SeedDeliveredAsync(
                db, mediator, logger,
                finderEmail:  UserSeeder.LongFpt.Email,
                ownerEmail:   UserSeeder.CatLinh.Email,
                finderPostId: finderPostId.Value,
                ownerPostId:  ownerPostId.Value,
                ct);
        }
    }

    // Scenario: LongFpt found ThangFpt's MagSafe charger; Thang initiates, Long delivers, Thang confirms.
    private static async Task SeedChargerConfirmedScenarioAsync(
        ApplicationDbContext db, ISender mediator, ILogger logger, CancellationToken ct)
    {
        var ownerPostId  = await SeedPostAsync(db, mediator, logger, UserSeeder.ThangFpt.Email, ThangFptLostCharger,  ct);
        var finderPostId = await SeedPostAsync(db, mediator, logger, UserSeeder.LongFpt.Email,  LongFptFoundCharger,  ct);

        if (finderPostId.HasValue && ownerPostId.HasValue)
        {
            await HandoverSeeder.SeedConfirmedAsync(
                db, mediator, logger,
                finderEmail:  UserSeeder.LongFpt.Email,
                ownerEmail:   UserSeeder.ThangFpt.Email,
                finderPostId: finderPostId.Value,
                ownerPostId:  ownerPostId.Value,
                ct);
        }
    }

    // ──────────────────────────────────────────────
    // Reusable seed method
    // ──────────────────────────────────────────────

    private static async Task<Guid?> SeedPostAsync(
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
            return null;
        }

        if (!Enum.TryParse<PostType>(postData.PostType, ignoreCase: true, out var postType))
        {
            logger.LogWarning("PostSeeder: invalid PostType '{PostType}' for {Email} — skipping.", postData.PostType, authorEmail);
            return null;
        }

        var subcategory = await db.Set<Subcategory>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Code == postData.SubcategoryCode, ct);

        if (subcategory is not null)
        {
            var existing = await db.Set<Post>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.AuthorId == user.Id && p.SubcategoryId == subcategory.Id, ct);

            if (existing is not null)
            {
                logger.LogInformation(
                    "PostSeeder: {PostType} post for {Email} in subcategory '{Subcategory}' already exists — skipping.",
                    postType, authorEmail, postData.SubcategoryCode);
                return existing.Id;
            }
        }

        try
        {
            var result = await mediator.Send(postData with { AuthorId = user.Id }, ct);
            logger.LogInformation("PostSeeder: created {PostType} post for {Email}.", postType, authorEmail);
            return result.Id;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "PostSeeder: failed to create {PostType} post for {Email} — skipping.", postType, authorEmail);
            return null;
        }
    }
}
