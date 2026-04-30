using Backtrack.Core.Application.Usecases.OrganizationInventory.CreateInventoryItem;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Data.Seeders;

public static class OrgInventorySeeder
{
    private sealed record InventorySeedData(
        string ItemName, string Category, string SubcategoryCode,
        string[] ImageUrls, DateTimeOffset? EventTime,
        string InternalLocation = "Lost & Found Office",
        PersonalBelongingDetailDto? PersonalBelongingDetail = null,
        ElectronicDetailDto? ElectronicDetail = null,
        OtherDetailDto? OtherDetail = null);

    private static readonly InventorySeedData[] InventoryItems =
    [
        new(
            ItemName: "White YETI Water Bottle", Category: "PersonalBelongings", SubcategoryCode: "bottle",
            ImageUrls: ["https://firebasestorage.googleapis.com/v0/b/backtrack-sep490.firebasestorage.app/o/posts%2Fimages%2Fyetti_watter_bottle_2.jpg?alt=media&token=be35b597-a322-41b1-8334-c699980e5734"],
            EventTime: DateTimeOffset.Parse("2026-03-15T08:00:00.000Z"),
            PersonalBelongingDetail: new PersonalBelongingDetailDto
            {
                ItemName = "White YETI Water Bottle",
                Color = "White",
                Brand = "YETI",
                Material = "Metal",
                Size = "Medium",
                AiDescription = "A white YETI branded metal water bottle, medium size, appearing in new condition."
            }),
        new(
            ItemName: "White Yeti Water Bottle", Category: "PersonalBelongings", SubcategoryCode: "bottle",
            ImageUrls: ["https://firebasestorage.googleapis.com/v0/b/backtrack-sep490.firebasestorage.app/o/posts%2Fimages%2Fyetti_watter_bottle.jpg?alt=media&token=00b0451d-12fd-4a54-9c41-dc962277c15a"],
            EventTime: DateTimeOffset.Parse("2026-03-15T09:30:00.000Z"),
            PersonalBelongingDetail: new PersonalBelongingDetailDto
            {
                ItemName = "White Yeti Water Bottle",
                Color = "White",
                Brand = "Yeti",
                Material = "Metal",
                Size = "Large",
                AiDescription = "A large white metal water bottle by Yeti, designed for high capacity and durability."
            }),
        new(
            ItemName: "Casio fx-570ES PLUS Calculator", Category: "Electronics", SubcategoryCode: "calculator",
            ImageUrls: ["https://firebasestorage.googleapis.com/v0/b/backtrack-sep490.firebasestorage.app/o/posts%2Fimages%2Fcasio-1.jpg?alt=media&token=cde38457-c722-492c-b96b-b34c47c3371b"],
            EventTime: DateTimeOffset.Parse("2026-03-14T14:00:00.000Z"),
            ElectronicDetail: new ElectronicDetailDto
            {
                ItemName = "Casio fx-570ES PLUS Calculator",
                Brand = "Casio",
                Color = "Silver, Blue",
                DistinguishingFeatures = "Scientific calculator",
                AiDescription = "A silver and blue Casio fx-570ES PLUS scientific calculator, showing signs of light use."
            }),
        new(
            ItemName: "Blue and grey Casio calculator", Category: "Electronics", SubcategoryCode: "calculator",
            ImageUrls: ["https://firebasestorage.googleapis.com/v0/b/backtrack-sep490.firebasestorage.app/o/posts%2Fimages%2Fcasio-2.jpg?alt=media&token=9497f7df-d640-4bd7-bf88-6bbca36f27aa"],
            EventTime: DateTimeOffset.Parse("2026-03-14T15:15:00.000Z"),
            ElectronicDetail: new ElectronicDetailDto
            {
                ItemName = "Blue and grey Casio calculator",
                Brand = "Casio",
                Color = "Blue and grey",
                AdditionalDetails = "Model: 570ES",
                AiDescription = "A blue and grey Casio scientific calculator, model 570ES, in good aesthetic condition."
            }),
        new(
            ItemName: "Black Leather Card Holder Wallet", Category: "PersonalBelongings", SubcategoryCode: "wallets",
            ImageUrls: ["https://firebasestorage.googleapis.com/v0/b/backtrack-sep490.firebasestorage.app/o/posts%2Fimages%2Fblack_leather_wallet_with_cards.jpg?alt=media&token=7e501790-c12d-487b-b9b0-5804acb81827"],
            EventTime: DateTimeOffset.Parse("2026-03-16T09:00:00.000Z"),
            PersonalBelongingDetail: new PersonalBelongingDetailDto
            {
                ItemName = "Black Leather Card Holder Wallet",
                Color = "Black, Brown",
                Brand = "Leo Phenom",
                Material = "Leather",
                Size = "Small",
                DistinctiveMarks = "Brown stitching, brand logo",
                AdditionalDetails = "Contains credit cards",
                AiDescription = "A small black leather card holder wallet with brown stitching and a Leo Phenom logo, containing several cards."
            }),
        new(
            ItemName: "Burgundy Leather Bi-Fold Wallet", Category: "PersonalBelongings", SubcategoryCode: "wallets",
            ImageUrls: ["https://firebasestorage.googleapis.com/v0/b/backtrack-sep490.firebasestorage.app/o/posts%2Fimages%2Fburberry_bifold_wallet_1.jpg?alt=media&token=e609fe50-cc99-4089-9044-1890a2a5d7e3"],
            EventTime: DateTimeOffset.Parse("2026-03-16T11:14:28.832Z"),
            PersonalBelongingDetail: new PersonalBelongingDetailDto
            {
                ItemName = "Burgundy Leather Bi-Fold Wallet",
                Color = "Burgundy",
                Brand = "Burberry",
                Material = "Leather",
                Size = "Small",
                DistinctiveMarks = "Gold colored brand plate",
                AiDescription = "A small burgundy leather bi-fold wallet by Burberry, featuring a prominent gold-colored brand plate."
            }),
        new(
            ItemName: "White Apple Laptop Power Adapter", Category: "Electronics", SubcategoryCode: "charger_adapter",
            ImageUrls: ["https://firebasestorage.googleapis.com/v0/b/backtrack-sep490.firebasestorage.app/o/posts%2Fimages%2Fwhite_macbook_charger_2.jpg?alt=media&token=a96f55d8-7976-4a05-8820-b0d79927b11b"],
            EventTime: DateTimeOffset.Parse("2026-03-16T10:00:00.000Z"),
            ElectronicDetail: new ElectronicDetailDto
            {
                ItemName = "White Apple Laptop Power Adapter",
                Brand = "Apple",
                Color = "White",
                DistinguishingFeatures = "Apple logo",
                AiDescription = "A standard white Apple laptop power adapter with the iconic Apple logo on the side."
            }),
        new(
            ItemName: "White Apple Laptop Charger", Category: "Electronics", SubcategoryCode: "charger_adapter",
            ImageUrls: ["https://firebasestorage.googleapis.com/v0/b/backtrack-sep490.firebasestorage.app/o/posts%2Fimages%2Fwhite_macbook_charger_3.jpg?alt=media&token=a5d6ae1f-f499-46e1-9560-4aec8fb931f7"],
            EventTime: DateTimeOffset.Parse("2026-03-16T14:30:00.000Z"),
            ElectronicDetail: new ElectronicDetailDto
            {
                ItemName = "White Apple Laptop Charger",
                Brand = "Apple",
                Color = "White",
                DistinguishingFeatures = "Dirty cable",
                AiDescription = "A white Apple laptop charger, similar to the power adapter, but with a notably discolored or dirty cable."
            }),
        new(
            ItemName: "Rimowa Blue Cabin Suitcase", Category: "PersonalBelongings", SubcategoryCode: "suitcases",
            ImageUrls: ["https://firebasestorage.googleapis.com/v0/b/backtrack-sep490.firebasestorage.app/o/posts%2Fimages%2Frimowa_suicase_1.jpg?alt=media&token=6eb6255f-4639-42d7-a62b-4a8fe7880044"],
            EventTime: DateTimeOffset.Parse("2026-03-16T09:30:00.000Z"),
            PersonalBelongingDetail: new PersonalBelongingDetailDto
            {
                ItemName = "Rimowa Blue Cabin Suitcase",
                Color = "Blue",
                Brand = "Rimowa",
                Material = "Metal",
                Size = "Small",
                DistinctiveMarks = "Vertical ridges, Rimowa tag",
                AiDescription = "A small blue Rimowa cabin suitcase with characteristic vertical ridges and a Rimowa identification tag."
            }),
        new(
            ItemName: "Blue Hardshell Carry-on Suitcase", Category: "PersonalBelongings", SubcategoryCode: "suitcases",
            ImageUrls: ["https://firebasestorage.googleapis.com/v0/b/backtrack-sep490.firebasestorage.app/o/posts%2Fimages%2Fsuicase_2.jpg?alt=media&token=5291c0ae-2204-4104-b49c-995319a024b6"],
            EventTime: DateTimeOffset.Parse("2026-03-16T11:14:28.832Z"),
            PersonalBelongingDetail: new PersonalBelongingDetailDto
            {
                ItemName = "Blue Hardshell Carry-on Suitcase",
                Color = "Blue",
                Material = "Plastic",
                Size = "Small",
                DistinctiveMarks = "Combination lock",
                AiDescription = "A small blue hardshell carry-on suitcase made of plastic, featuring a built-in combination lock."
            }),
        new(
            ItemName: "White Converse High-Top Sneakers", Category: "PersonalBelongings", SubcategoryCode: "clothings",
            ImageUrls: ["https://firebasestorage.googleapis.com/v0/b/backtrack-sep490.firebasestorage.app/o/posts%2Fimages%2Fold_shoes_1.jpg?alt=media&token=81f62efe-a6ed-4e1f-ba77-44796005b1f6"],
            EventTime: DateTimeOffset.UtcNow.AddDays(-5),
            PersonalBelongingDetail: new PersonalBelongingDetailDto
            {
                ItemName = "White Converse High-Top Sneakers",
                Color = "White",
                Brand = "Converse",
                Material = "Canvas",
                AdditionalDetails = "This is a pair of used white Converse high-top sneakers. The shoes have white laces and a grey interior lining. The shoes appear to be worn and dirty.",
                AiDescription = "A pair of used white Converse high-top canvas sneakers with white laces, showing significant wear and dirt."
            }),
        new(
            ItemName: "Keys with EGYPT Keychain and Hyundai Fob", Category: "PersonalBelongings", SubcategoryCode: "keys",
            ImageUrls: ["https://firebasestorage.googleapis.com/v0/b/backtrack-sep490.firebasestorage.app/o/posts%2Fimages%2Fbrown_key_1.jpg?alt=media&token=77f05992-7f9e-46f8-acc1-8767022374e5"],
            EventTime: DateTimeOffset.UtcNow.AddDays(-4),
            PersonalBelongingDetail: new PersonalBelongingDetailDto
            {
                ItemName = "Keys with EGYPT Keychain and Hyundai Fob",
                Color = "Silver, Black, Brown, Gold",
                Brand = "Hyundai",
                Material = "Metal, Leather, Plastic",
                DistinctiveMarks = "Keychain with 'EGYPT' inscription and pharaoh head, Hyundai car key fob",
                AdditionalDetails = "Keys with a brown leather keychain with a silver plate that says 'EGYPT' and has a gold pharaoh head. The keys include a Hyundai car key fob.",
                AiDescription = "A set of keys featuring a Hyundai fob and a brown leather keychain with an 'EGYPT' silver plate and a pharaoh head."
            }),
    ];

    private const string TargetOrgSlug = "fpt-qn";

    public static async Task SeedAsync(
        ApplicationDbContext db,
        ISender mediator,
        ILogger logger,
        CancellationToken ct = default)
    {
        var org = await db.Set<Organization>()
            .Include(o => o.Memberships.Where(m => m.Role == MembershipRole.OrgStaff))
            .FirstOrDefaultAsync(o => o.Slug == TargetOrgSlug, ct);

        if (org == null)
        {
            logger.LogWarning("OrgInventorySeeder: org '{Slug}' not found — skipping.", TargetOrgSlug);
            return;
        }

        var inventoryCount = await db.Set<Post>()
            .Where(p => p.OrganizationId == org.Id)
            .CountAsync(ct);

        if (inventoryCount >= InventoryItems.Length)
        {
            logger.LogInformation("OrgInventorySeeder: org '{Slug}' already has inventory — skipping.", TargetOrgSlug);
            return;
        }

        var staff = org.Memberships.FirstOrDefault();
        if (staff == null)
        {
            logger.LogWarning("OrgInventorySeeder: no staff found for org '{Slug}' — skipping.", TargetOrgSlug);
            return;
        }

        logger.LogInformation("OrgInventorySeeder: seeding inventory for org '{Slug}'.", TargetOrgSlug);

        for (int i = inventoryCount; i < InventoryItems.Length; i++)
        {
            var data = InventoryItems[i];

            try
            {
                var command = new CreateInventoryItemCommand
                {
                    StaffId = staff.UserId,
                    OrgId = org.Id,
                    PostTitle = data.ItemName,
                    Category = data.Category,
                    SubcategoryCode = data.SubcategoryCode,
                    ImageUrls = data.ImageUrls,
                    EventTime = data.EventTime ?? DateTimeOffset.UtcNow,
                    InternalLocation = data.InternalLocation,
                    PersonalBelongingDetail = data.PersonalBelongingDetail,
                    ElectronicDetail = data.ElectronicDetail,
                    OtherDetail = data.OtherDetail,
                    FinderInfo = new FinderInfo { FinderName = "Anonymous Seeder", Phone = "0123456789", Email = "anonymous@seeder.com", NationalId = "1234567890" }
                };

                await mediator.Send(command, ct);
                logger.LogInformation("OrgInventorySeeder: created '{ItemName}' for org '{Slug}'.", data.ItemName, TargetOrgSlug);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OrgInventorySeeder: failed to create '{ItemName}' for org '{Slug}'.", data.ItemName, TargetOrgSlug);
            }
        }
    }
}
