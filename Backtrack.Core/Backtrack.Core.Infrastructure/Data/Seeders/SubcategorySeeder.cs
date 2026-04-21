using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Data.Seeders;

public static class SubcategorySeeder
{
    private static readonly (ItemCategory Category, string Code, string Name, int Order)[] Definitions =
    [
        (ItemCategory.Electronics, "phone",           "Phone",           1),
        (ItemCategory.Electronics, "laptop",          "Laptop",          2),
        (ItemCategory.Electronics, "smartwatch",      "Smartwatch",      3),
        (ItemCategory.Electronics, "charger_adapter", "Charger Adapter", 4),
        (ItemCategory.Electronics, "mouse",           "Mouse",           5),
        (ItemCategory.Electronics, "keyboard",        "Keyboard",        6),
        (ItemCategory.Electronics, "powerbank",       "Powerbank",       7),
        (ItemCategory.Electronics, "power_outlet",    "Power Outlet",    8),
        (ItemCategory.Electronics, "headphone",       "Headphone",       9),
        (ItemCategory.Electronics, "earphone",        "Earphone",        10),
        (ItemCategory.Electronics, "calculator",      "Calculator",      11),

        (ItemCategory.Cards, "identification_card", "Identification Card", 1),
        (ItemCategory.Cards, "passport",            "Passport",            2),
        (ItemCategory.Cards, "driver_license",      "Driver License",      3),
        (ItemCategory.Cards, "personal_card",       "Personal Card",       4),
        (ItemCategory.Cards, "bank_card",           "Bank Card",           5),
        (ItemCategory.Cards, "student_card",        "Student Card",        6),
        (ItemCategory.Cards, "company_card",        "Company Card",        7),

        (ItemCategory.PersonalBelongings, "wallets",   "Wallets",   1),
        (ItemCategory.PersonalBelongings, "keys",      "Keys",      2),
        (ItemCategory.PersonalBelongings, "suitcases", "Suitcases", 3),
        (ItemCategory.PersonalBelongings, "backpack",  "Backpack",  4),
        (ItemCategory.PersonalBelongings, "clothings", "Clothings", 5),
        (ItemCategory.PersonalBelongings, "jewelry",   "Jewelry",   6),
        (ItemCategory.PersonalBelongings, "bottle",    "Bottle",    7),

        (ItemCategory.Others, "others", "Others", 1),
    ];

    public static async Task SeedAsync(ApplicationDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var existingCodes = (await db.Set<Subcategory>()
            .IgnoreQueryFilters()
            .Select(s => s.Code)
            .ToListAsync(ct))
            .ToHashSet();

        var now = DateTimeOffset.UtcNow;
        var created = new List<string>();
        var skipped = new List<string>();

        foreach (var d in Definitions)
        {
            if (existingCodes.Contains(d.Code)) { skipped.Add(d.Code); continue; }

            db.Set<Subcategory>().Add(new Subcategory
            {
                Id           = Guid.NewGuid(),
                Category     = d.Category,
                Code         = d.Code,
                Name         = d.Name,
                DisplayOrder = d.Order,
                IsActive     = true,
                CreatedAt    = now
            });
            created.Add(d.Code);
        }

        if (created.Count > 0)
            await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Subcategories — created {Created}, skipped {Skipped}. Created: [{CreatedNames}] Skipped: [{SkippedNames}]",
            created.Count, skipped.Count,
            string.Join(", ", created),
            string.Join(", ", skipped));
    }
}
