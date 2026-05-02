using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Usecases.Posts.CreatePost;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Data.Seeders;

/// <summary>
/// Seeder for 10 test cases used to evaluate matching threshold.
///
/// Important: each lost/found pair shares the SAME category and SAME subcategory,
/// because similarity can only be compared within the same schema (CTI detail table).
///
/// How to use the results:
///   - Tag each pair with GroundTruth = MATCH | NO_MATCH (see comments per pair)
///   - Run similarity search for each lost/found pair
///   - For each threshold t in {0.5, 0.55, ..., 0.9}:
///       Predicted = similarity >= t ? MATCH : NO_MATCH
///       Compute TP / FP / TN / FN -> Precision, Recall, F1
///   - Best threshold = highest F1
///
/// Distribution of 10 pairs:
///   Strong match (4):  P01, P02, P03, P04  -> expected sim ~0.80-0.95
///   Borderline   (3):  P05, P06, P07        -> expected sim ~0.55-0.75
///   No-match     (3):  P08, P09, P10        -> expected sim ~0.30-0.65
///
/// Language distribution (mirrors real usage):
///   Vietnamese: P02, P03, P07
///   English:    P01, P04, P05, P06, P08, P09, P10
///
/// Subcategory coverage:
///   Cards: student_card, bank_card
///   Electronics: phone, laptop, headphone, powerbank, earphone
///   PersonalBelongings: wallets, keys, bottle
/// </summary>
public static class MatchingTestSeeder
{
    // ──────────────────────────────────────────────
    // Locations (only 2 locations to keep variables controlled)
    // ──────────────────────────────────────────────

    private static readonly GeoPoint LocationA = new(10.8417, 106.8100);
    private const string AddressA = "Lô E2a-7, Đường D1, Khu Công nghệ cao, Phường Tăng Nhơn Phú A, TP Thủ Đức";

    private static readonly GeoPoint LocationB = new(10.7726, 106.6981);
    private const string AddressB = "Chợ Bến Thành, Phường Bến Thành, Quận 1, TP Hồ Chí Minh";

    // ──────────────────────────────────────────────
    // Time gaps (2 values: recent and old)
    // ──────────────────────────────────────────────

    private static DateTimeOffset TimeRecent => DateTimeOffset.UtcNow.AddDays(-1);
    private static DateTimeOffset TimeOld => DateTimeOffset.UtcNow.AddDays(-7);

    // ══════════════════════════════════════════════════════════════
    // STRONG MATCH GROUP — Expected sim ~0.80-0.95, Label: MATCH
    // Purpose: the system MUST recognize these. Failure -> low recall.
    // ══════════════════════════════════════════════════════════════

    // ─── P01: student_card | Baseline easy (English) ─────────────
    // Same person, full info, descriptions match closely
    // Expected: similarity ~0.92 | Label: MATCH
    private static readonly CreatePostCommand P01_Lost_StudentCard = new()
    {
        PostType = "Lost",
        PostTitle = "Lost FPT student card",
        Category = "Cards",
        SubcategoryCode = "student_card",
        ImageUrls = [],
        Location = LocationA,
        DisplayAddress = AddressA,
        EventTime = TimeRecent,
        CardDetail = new CardDetailDto
        {
            ItemName = "FPT University student card",
            HolderName = "NGUYEN VAN A",
            HolderNameNormalized = "nguyen van a",
            IssuingAuthority = "FPT University",
            AdditionalDetails = "FPT student card, Software Engineering major",
        },
    };

    private static readonly CreatePostCommand P01_Found_StudentCard = new()
    {
        PostType = "Found",
        PostTitle = "Found FPT student card",
        Category = "Cards",
        SubcategoryCode = "student_card",
        ImageUrls = ["https://img.heroui.chat/image/img?w=800&h=600&u=101", "https://img.heroui.chat/image/img?w=800&h=600&u=102"],
        Location = LocationA,
        DisplayAddress = AddressA,
        EventTime = TimeRecent,
        CardDetail = new CardDetailDto
        {
            ItemName = "FPT University student card",
            CardNumber = "SE161234",
            HolderName = "NGUYEN VAN A",
            HolderNameNormalized = "nguyen van a",
            IssuingAuthority = "FPT University",
            AdditionalDetails = "Student card from FPT University, major in Software Engineering",
        },
    };

    // ─── P02: wallets | Recall test — different description language (Vietnamese vs English) ──
    // Same wallet; loser writes Vietnamese with emotion, finder writes terse English
    // Expected: similarity ~0.78-0.85 | Label: MATCH
    private static readonly CreatePostCommand P02_Lost_Wallet = new()
    {
        PostType = "Lost",
        PostTitle = "Mất ví da nâu, ai nhặt được liên hệ giúp",
        Category = "PersonalBelongings",
        SubcategoryCode = "wallets",
        ImageUrls = [],
        Location = LocationB,
        DisplayAddress = AddressB,
        EventTime = TimeRecent,
        PersonalBelongingDetail = new PersonalBelongingDetailDto
        {
            ItemName = "Ví da nâu",
            Color = "nâu",
            Material = "da bò thật",
            Size = "vừa",
            DistinctiveMarks = "Vết xước nhỏ ở góc dưới, bên trong có ảnh con gái nhỏ",
            AiDescription = "Ví da nam màu nâu sẫm, chất liệu da bò thật, kích thước vừa phải gập đôi. Mặt ngoài có vết xước nhẹ ở góc dưới bên phải. Bên trong có nhiều ngăn đựng thẻ và một ngăn ảnh nhỏ chứa hình một bé gái.",
        },
    };

    private static readonly CreatePostCommand P02_Found_Wallet = new()
    {
        PostType = "Found",
        PostTitle = "Found brown leather wallet",
        Category = "PersonalBelongings",
        SubcategoryCode = "wallets",
        ImageUrls = ["https://img.heroui.chat/image/img?w=800&h=600&u=201", "https://img.heroui.chat/image/img?w=800&h=600&u=202"],
        Location = LocationB,
        DisplayAddress = AddressB,
        EventTime = TimeRecent,
        PersonalBelongingDetail = new PersonalBelongingDetailDto
        {
            ItemName = "Brown leather wallet",
            Color = "brown",
            Material = "leather",
            Size = "medium",
            DistinctiveMarks = "Scratch on bottom corner, contains a small photo of a young girl",
            AiDescription = "Men's bifold wallet in dark brown leather. Has visible wear and a scratch on the bottom-right corner. Multiple card slots inside, with a small photo insert containing a picture of a young girl.",
        },
    };

    // ─── P03: phone | Purple iPhone with distinctive sticker (Vietnamese) ──
    // Same iPhone; descriptions match on model + color + unique mark (Koi fish sticker)
    // Expected: similarity ~0.88 | Label: MATCH
    private static readonly CreatePostCommand P03_Lost_IPhone = new()
    {
        PostType = "Lost",
        PostTitle = "Mất iPhone 14 Pro Max màu tím",
        Category = "Electronics",
        SubcategoryCode = "phone",
        ImageUrls = [],
        Location = LocationA,
        DisplayAddress = AddressA,
        EventTime = TimeRecent,
        ElectronicDetail = new ElectronicDetailDto
        {
            ItemName = "iPhone 14 Pro Max",
            Brand = "Apple",
            Model = "iPhone 14 Pro Max",
            Color = "Deep Purple",
            HasCase = true,
            DistinguishingFeatures = "Ốp lưng trong suốt có dán sticker hình con cá Koi màu cam ở mặt sau, màn hình có dán PPF",
            AiDescription = "iPhone 14 Pro Max màu Deep Purple (tím đậm), 256GB. Dùng ốp lưng trong suốt, mặt sau có sticker hình cá Koi cam. Màn hình dán PPF, không nứt vỡ.",
        },
    };

    private static readonly CreatePostCommand P03_Found_IPhone = new()
    {
        PostType = "Found",
        PostTitle = "Nhặt được iPhone tím trong khuôn viên",
        Category = "Electronics",
        SubcategoryCode = "phone",
        ImageUrls = ["https://img.heroui.chat/image/img?w=800&h=600&u=301", "https://img.heroui.chat/image/img?w=800&h=600&u=302"],
        Location = LocationA,
        DisplayAddress = AddressA,
        EventTime = TimeRecent,
        ElectronicDetail = new ElectronicDetailDto
        {
            ItemName = "iPhone 14 Pro Max tím",
            Brand = "Apple",
            Model = "iPhone 14 Pro Max",
            Color = "Tím",
            HasCase = true,
            DistinguishingFeatures = "Có ốp trong, mặt sau có sticker hình cá vàng/cam",
            AiDescription = "iPhone 14 Pro Max màu tím, ốp lưng trong suốt. Mặt lưng máy có dán sticker hình con cá màu cam khá nổi bật.",
        },
    };

    // ─── P04: keys | Keychain with distinctive charm (English) ────
    // Same keychain; descriptions agree on key count + charm
    // Expected: similarity ~0.82 | Label: MATCH
    private static readonly CreatePostCommand P04_Lost_Keys = new()
    {
        PostType = "Lost",
        PostTitle = "Lost keychain with Pikachu charm",
        Category = "PersonalBelongings",
        SubcategoryCode = "keys",
        ImageUrls = [],
        Location = LocationA,
        DisplayAddress = AddressA,
        EventTime = TimeOld,
        PersonalBelongingDetail = new PersonalBelongingDetailDto
        {
            ItemName = "Keychain",
            Color = "yellow",
            Material = "metal",
            Size = "small",
            DistinctiveMarks = "3 keys: 1 house key, 1 motorbike key, 1 small key. Has yellow plastic Pikachu charm",
            AiDescription = "Keychain with 3 keys: a large house door key, a Honda SH motorbike key, and a small cabinet key. All attached to a yellow plastic Pikachu keychain charm.",
        },
    };

    private static readonly CreatePostCommand P04_Found_Keys = new()
    {
        PostType = "Found",
        PostTitle = "Found a set of keys",
        Category = "PersonalBelongings",
        SubcategoryCode = "keys",
        ImageUrls = ["https://img.heroui.chat/image/img?w=800&h=600&u=401"],
        Location = LocationA,
        DisplayAddress = AddressA,
        EventTime = TimeOld,
        PersonalBelongingDetail = new PersonalBelongingDetailDto
        {
            ItemName = "Set of 3 keys",
            Color = "silver",
            Material = "metal",
            Size = "small",
            DistinctiveMarks = "Yellow Pokemon keychain (Pikachu shape), 3 keys",
            AiDescription = "A set of 3 keys hanging on a yellow Pikachu-shaped keychain charm. One large key (house), one motorbike key, one small key.",
        },
    };

    // ══════════════════════════════════════════════════════════════
    // BORDERLINE GROUP — Expected sim ~0.55-0.75
    // This is the critical zone — threshold determines behavior here.
    // ══════════════════════════════════════════════════════════════

    // ─── P05: laptop | Same MacBook family but DIFFERENT GENERATION (English) ──
    // Same brand + family + color, different model year. Likely two different units.
    // Expected: similarity ~0.65-0.72 | Label: NO_MATCH
    private static readonly CreatePostCommand P05_Lost_Macbook = new()
    {
        PostType = "Lost",
        PostTitle = "Lost MacBook Air M2 in Space Gray",
        Category = "Electronics",
        SubcategoryCode = "laptop",
        ImageUrls = [],
        Location = LocationB,
        DisplayAddress = AddressB,
        EventTime = TimeRecent,
        ElectronicDetail = new ElectronicDetailDto
        {
            ItemName = "MacBook Air M2",
            Brand = "Apple",
            Model = "MacBook Air M2 13 inch",
            Color = "Space Gray",
            HasCase = false,
            DistinguishingFeatures = "",
            AiDescription = "MacBook Air M2 13 inch in Space Gray, no case or stickers. Device is in normal condition with no distinguishing marks.",
        },
    };

    private static readonly CreatePostCommand P05_Found_Macbook = new()
    {
        PostType = "Found",
        PostTitle = "Found gray MacBook Air",
        Category = "Electronics",
        SubcategoryCode = "laptop",
        ImageUrls = ["https://img.heroui.chat/image/img?w=800&h=600&u=501", "https://img.heroui.chat/image/img?w=800&h=600&u=502"],
        Location = LocationB,
        DisplayAddress = AddressB,
        EventTime = TimeRecent,
        ElectronicDetail = new ElectronicDetailDto
        {
            ItemName = "MacBook Air M3",
            Brand = "Apple",
            Model = "MacBook Air M3 13 inch",
            Color = "Space Gray",
            HasCase = false,
            DistinguishingFeatures = "",
            AiDescription = "MacBook Air M3 13 inch in Space Gray. Device looks fairly new, no case or distinctive stickers.",
        },
    };

    // ─── P06: headphone | Skewed detail level — detailed vs sparse (English) ──
    // Same headphones; lost post is detailed, found post is generic
    // Expected: similarity ~0.60-0.70 | Label: MATCH
    private static readonly CreatePostCommand P06_Lost_Headphone = new()
    {
        PostType = "Lost",
        PostTitle = "Lost Sony WH-1000XM4 black",
        Category = "Electronics",
        SubcategoryCode = "headphone",
        ImageUrls = [],
        Location = LocationA,
        DisplayAddress = AddressA,
        EventTime = TimeRecent,
        ElectronicDetail = new ElectronicDetailDto
        {
            ItemName = "Sony WH-1000XM4",
            Brand = "Sony",
            Model = "WH-1000XM4",
            Color = "Black",
            HasCase = true,
            DistinguishingFeatures = "Small scratch on left earpad, 3.5mm cable still inside the case",
            AiDescription = "Sony WH-1000XM4 over-ear noise-cancelling headphones in black, stored in matching black hard case. Left earpad has a small scratch from impact. The case still contains the 3.5mm cable and airplane adapter.",
        },
    };

    private static readonly CreatePostCommand P06_Found_Headphone = new()
    {
        PostType = "Found",
        PostTitle = "Found black Sony headphones",
        Category = "Electronics",
        SubcategoryCode = "headphone",
        ImageUrls = ["https://img.heroui.chat/image/img?w=800&h=600&u=601", "https://img.heroui.chat/image/img?w=800&h=600&u=602"],
        Location = LocationA,
        DisplayAddress = AddressA,
        EventTime = TimeRecent,
        ElectronicDetail = new ElectronicDetailDto
        {
            ItemName = "Sony headphones",
            Brand = "Sony",
            Model = "",
            Color = "black",
            HasCase = true,
            DistinguishingFeatures = "has a case",
            AiDescription = "A pair of Sony over-ear headphones in black. Comes with a case.",
        },
    };

    // ─── P07: bank_card | Different abbreviations of bank name (Vietnamese) ──
    // Same card; one post writes the full name, the other uses an abbreviation
    // Expected: similarity ~0.58-0.70 | Label: MATCH
    private static readonly CreatePostCommand P07_Lost_BankCard = new()
    {
        PostType = "Lost",
        PostTitle = "Mất thẻ ATM Vietcombank",
        Category = "Cards",
        SubcategoryCode = "bank_card",
        ImageUrls = [],
        Location = LocationB,
        DisplayAddress = AddressB,
        EventTime = TimeOld,
        CardDetail = new CardDetailDto
        {
            ItemName = "Thẻ ATM Vietcombank",
            HolderName = "TRAN THI B",
            HolderNameNormalized = "tran thi b",
            IssuingAuthority = "Vietcombank",
            AdditionalDetails = "Thẻ ghi nợ nội địa Vietcombank, mặt trước có logo VCB màu xanh lá đặc trưng",
        },
    };

    private static readonly CreatePostCommand P07_Found_BankCard = new()
    {
        PostType = "Found",
        PostTitle = "Nhặt được 1 thẻ ngân hàng",
        Category = "Cards",
        SubcategoryCode = "bank_card",
        ImageUrls = ["https://img.heroui.chat/image/img?w=800&h=600&u=701"],
        Location = LocationB,
        DisplayAddress = AddressB,
        EventTime = TimeOld,
        CardDetail = new CardDetailDto
        {
            ItemName = "Thẻ ngân hàng",
            HolderName = "TRAN THI B",
            HolderNameNormalized = "tran thi b",
            IssuingAuthority = "VCB",
            AdditionalDetails = "Thẻ ghi nợ màu xanh lá",
        },
    };

    // ══════════════════════════════════════════════════════════════
    // CLEAR NO-MATCH GROUP — Expected sim ~0.30-0.65, Label: NO_MATCH
    // Purpose: the system MUST reject these. Failure -> low precision.
    // ══════════════════════════════════════════════════════════════

    // ─── P08: powerbank | Different brand + capacity, clearly distinct (English) ──
    // Same subcategory, completely different brand and specs
    // Expected: similarity ~0.40-0.55 | Label: NO_MATCH
    private static readonly CreatePostCommand P08_Lost_Powerbank = new()
    {
        PostType = "Lost",
        PostTitle = "Lost Anker powerbank",
        Category = "Electronics",
        SubcategoryCode = "powerbank",
        ImageUrls = [],
        Location = LocationA,
        DisplayAddress = AddressA,
        EventTime = TimeRecent,
        ElectronicDetail = new ElectronicDetailDto
        {
            ItemName = "Anker PowerCore 20000mAh",
            Brand = "Anker",
            Model = "PowerCore 20000",
            Color = "Black",
            HasCase = false,
            DistinguishingFeatures = "Has a label with the name 'NGUYEN' on the bottom, 20000mAh capacity, 2 USB ports",
            AiDescription = "Anker PowerCore 20000mAh powerbank in black, large and heavy form factor. Two USB-A output ports. Bottom has a handwritten label saying 'NGUYEN' in marker.",
        },
    };

    private static readonly CreatePostCommand P08_Found_Powerbank = new()
    {
        PostType = "Found",
        PostTitle = "Found a small Xiaomi powerbank",
        Category = "Electronics",
        SubcategoryCode = "powerbank",
        ImageUrls = ["https://img.heroui.chat/image/img?w=800&h=600&u=801", "https://img.heroui.chat/image/img?w=800&h=600&u=802"],
        Location = LocationA,
        DisplayAddress = AddressA,
        EventTime = TimeRecent,
        ElectronicDetail = new ElectronicDetailDto
        {
            ItemName = "Xiaomi Mi Powerbank 10000mAh",
            Brand = "Xiaomi",
            Model = "Mi Powerbank 3",
            Color = "White",
            HasCase = false,
            DistinguishingFeatures = "Slim, lightweight, 10000mAh capacity, single USB-C port",
            AiDescription = "Xiaomi Mi 10000mAh powerbank in white, slim and lightweight design. Single USB-C input/output port. Embossed MI logo on the back.",
        },
    };

    // ─── P09: bottle | Different material, different color entirely (English) ──
    // Same subcategory, attributes diverge across the board
    // Expected: similarity ~0.30-0.45 | Label: NO_MATCH
    private static readonly CreatePostCommand P09_Lost_Bottle = new()
    {
        PostType = "Lost",
        PostTitle = "Lost Lock&Lock thermos bottle",
        Category = "PersonalBelongings",
        SubcategoryCode = "bottle",
        ImageUrls = [],
        Location = LocationB,
        DisplayAddress = AddressB,
        EventTime = TimeRecent,
        PersonalBelongingDetail = new PersonalBelongingDetailDto
        {
            ItemName = "Lock&Lock thermos bottle",
            Color = "black",
            Material = "stainless steel",
            Size = "500ml",
            DistinctiveMarks = "Name 'MINH' engraved on the body",
            AiDescription = "Lock&Lock thermos bottle, 500ml capacity, solid black color, stainless steel construction. The name 'MINH' is laser-engraved on the body.",
        },
    };

    private static readonly CreatePostCommand P09_Found_Bottle = new()
    {
        PostType = "Found",
        PostTitle = "Found a pink plastic water bottle",
        Category = "PersonalBelongings",
        SubcategoryCode = "bottle",
        ImageUrls = ["https://img.heroui.chat/image/img?w=800&h=600&u=901", "https://img.heroui.chat/image/img?w=800&h=600&u=902"],
        Location = LocationB,
        DisplayAddress = AddressB,
        EventTime = TimeRecent,
        PersonalBelongingDetail = new PersonalBelongingDetailDto
        {
            ItemName = "Lotus water bottle",
            Color = "pastel pink",
            Material = "Tritan plastic",
            Size = "750ml",
            DistinctiveMarks = "Hello Kitty stickers on the body",
            AiDescription = "Tritan plastic water bottle in pastel pink, 750ml capacity. Body is covered with multiple Hello Kitty and other Sanrio character stickers.",
        },
    };

    // ─── P10: earphone | TRICKY — same white wireless earbuds look, different brand (English) ──
    // Precision test: same color + same form factor (white TWS earbuds with case),
    // but completely different brand and model (AirPods Pro vs Samsung Galaxy Buds2).
    // If the system matches -> false positive due to generic shared appearance.
    // Expected: similarity ~0.55-0.70 | Label: NO_MATCH
    private static readonly CreatePostCommand P10_Lost_Earphone = new()
    {
        PostType = "Lost",
        PostTitle = "Lost AirPods Pro white earbuds",
        Category = "Electronics",
        SubcategoryCode = "earphone",
        ImageUrls = [],
        Location = LocationA,
        DisplayAddress = AddressA,
        EventTime = TimeRecent,
        ElectronicDetail = new ElectronicDetailDto
        {
            ItemName = "Apple AirPods Pro (2nd gen)",
            Brand = "Apple",
            Model = "AirPods Pro 2nd generation",
            Color = "White",
            HasCase = true,
            DistinguishingFeatures = "MagSafe charging case with a small dent on the lid, left earbud tip is medium size",
            AiDescription = "Apple AirPods Pro 2nd generation in white, stored in MagSafe charging case. Case has a small dent on the upper lid. Left earbud has a medium silicone ear tip attached.",
        },
    };

    private static readonly CreatePostCommand P10_Found_Earphone = new()
    {
        PostType = "Found",
        PostTitle = "Found white wireless earbuds with case",
        Category = "Electronics",
        SubcategoryCode = "earphone",
        ImageUrls = ["https://img.heroui.chat/image/img?w=800&h=600&u=1001"],
        Location = LocationA,
        DisplayAddress = AddressA,
        EventTime = TimeRecent,
        ElectronicDetail = new ElectronicDetailDto
        {
            ItemName = "Samsung Galaxy Buds2",
            Brand = "Samsung",
            Model = "Galaxy Buds2",
            Color = "White",
            HasCase = true,
            DistinguishingFeatures = "Compact oval charging case, no visible damage",
            AiDescription = "Samsung Galaxy Buds2 in white, stored in a compact oval charging case. Case and earbuds are in clean condition with no visible scratches or damage.",
        },
    };

    // ──────────────────────────────────────────────
    private const int TimeDelay = 3000;

    // ──────────────────────────────────────────────
    // Entry point
    // ──────────────────────────────────────────────

    public static async Task SeedAsync(
        ApplicationDbContext db,
        ISender mediator,
        ILogger logger,
        CancellationToken ct = default)
    {
        // // Strong match group
        // await SeedPairAsync(db, mediator, logger, "P01", UserSeeder.NgoDucBinh.Email, P01_Lost_StudentCard, UserSeeder.LongFpt.Email, P01_Found_StudentCard, ct);
        // await SeedPairAsync(db, mediator, logger, "P02", UserSeeder.CatLinh.Email, P02_Lost_Wallet, UserSeeder.LongFpt.Email, P02_Found_Wallet, ct);
        // await SeedPairAsync(db, mediator, logger, "P03", UserSeeder.ThangFpt.Email, P03_Lost_IPhone, UserSeeder.LongFpt.Email, P03_Found_IPhone, ct);
        // await SeedPairAsync(db, mediator, logger, "P04", UserSeeder.NgoDucBinh.Email, P04_Lost_Keys, UserSeeder.LongFpt.Email, P04_Found_Keys, ct);

        // // // Borderline group
        // await SeedPairAsync(db, mediator, logger, "P05", UserSeeder.CatLinh.Email, P05_Lost_Macbook, UserSeeder.LongFpt.Email, P05_Found_Macbook, ct);
        // await SeedPairAsync(db, mediator, logger, "P06", UserSeeder.ThangFpt.Email, P06_Lost_Headphone, UserSeeder.LongFpt.Email, P06_Found_Headphone, ct);
        // await SeedPairAsync(db, mediator, logger, "P07", UserSeeder.CatLinh.Email, P07_Lost_BankCard, UserSeeder.LongFpt.Email, P07_Found_BankCard, ct);

        // // // No-match group
        // await SeedPairAsync(db, mediator, logger, "P08", UserSeeder.NgoDucBinh.Email, P08_Lost_Powerbank, UserSeeder.LongFpt.Email, P08_Found_Powerbank, ct);
        // await SeedPairAsync(db, mediator, logger, "P09", UserSeeder.ThangFpt.Email, P09_Lost_Bottle, UserSeeder.LongFpt.Email, P09_Found_Bottle, ct);
        await SeedPairAsync(db, mediator, logger, "P10", UserSeeder.CatLinh.Email, P10_Lost_Earphone, UserSeeder.LongFpt.Email, P10_Found_Earphone, ct);
    }

    // ──────────────────────────────────────────────
    // Reusable seed methods
    // ──────────────────────────────────────────────

    private static async Task SeedPairAsync(
        ApplicationDbContext db,
        ISender mediator,
        ILogger logger,
        string pairId,
        string ownerEmail,
        CreatePostCommand lostPost,
        string finderEmail,
        CreatePostCommand foundPost,
        CancellationToken ct)
    {
        logger.LogInformation("MatchingTestSeeder: seeding pair {PairId}...", pairId);
        await SeedPostAsync(db, mediator, logger, ownerEmail, lostPost, ct);
        await Task.Delay(TimeDelay, ct);
        await SeedPostAsync(db, mediator, logger, finderEmail, foundPost, ct);
        await Task.Delay(TimeDelay, ct);
    }

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
            logger.LogWarning("MatchingTestSeeder: user {Email} not found — skipping post.", authorEmail);
            return null;
        }

        if (!Enum.TryParse<PostType>(postData.PostType, ignoreCase: true, out var postType))
        {
            logger.LogWarning("MatchingTestSeeder: invalid PostType '{PostType}' — skipping.", postData.PostType);
            return null;
        }

        try
        {
            var result = await mediator.Send(postData with { AuthorId = user.Id }, ct);
            logger.LogInformation("MatchingTestSeeder: created {PostType} post '{Title}' for {Email}.",
                postType, postData.PostTitle, authorEmail);
            return result.Id;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "MatchingTestSeeder: failed to create post '{Title}' for {Email}.",
                postData.PostTitle, authorEmail);
            return null;
        }
    }
}