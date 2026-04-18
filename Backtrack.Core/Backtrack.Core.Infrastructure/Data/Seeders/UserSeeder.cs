using Backtrack.Core.Application.Usecases.Users.EnsureUserExist;
using Backtrack.Core.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Data.Seeders;

public static class UserSeeder
{
    private sealed record UserSeedInfo(string Email, string Password, string DisplayName, string AvatarUrl);

    private static string Avatar(int u) => $"https://img.heroui.chat/image/avatar?w=400&h=400&u={u}";

    public static readonly (string Email, string Password, string DisplayName, string AvatarUrl) LongFpt
        = ("longnpse181672@fpt.edu.vn", "123456", "Long Nguyen", Avatar(1));
    public static readonly (string Email, string Password, string DisplayName, string AvatarUrl) PhiLong
        = ("philongnguyen1704@gmail.com", "123456", "Phi Long Nguyen", Avatar(2));
    public static readonly (string Email, string Password, string DisplayName, string AvatarUrl) PhiLongKoko
        = ("philongkokodudu@gmail.com", "123456", "Phi Long Koko", Avatar(3));
    public static readonly (string Email, string Password, string DisplayName, string AvatarUrl) PhiLongTik
        = ("philongtik21@gmail.com", "123456", "Phi Long Tik", Avatar(4));
    public static readonly (string Email, string Password, string DisplayName, string AvatarUrl) NhatThang
        = ("trannhatthang.dev@gmail.com", "123456", "Tran Nhat Thang", Avatar(5));
    public static readonly (string Email, string Password, string DisplayName, string AvatarUrl) ThangFpt
        = ("thangtnse181999@fpt.edu.vn", "123456", "Thang Tran", Avatar(6));
    public static readonly (string Email, string Password, string DisplayName, string AvatarUrl) ThangNguyen
        = ("thang.nguyen2026@gmail.com", "123456", "Thang Nguyen", Avatar(7));
    public static readonly (string Email, string Password, string DisplayName, string AvatarUrl) CatLinh
        = ("catlinh.design@gmail.com", "123456", "Cat Linh", Avatar(8));
    public static readonly (string Email, string Password, string DisplayName, string AvatarUrl) LinhTran
        = ("linh.tran99@gmail.com", "123456", "Linh Tran", Avatar(9));
    public static readonly (string Email, string Password, string DisplayName, string AvatarUrl) HoangLinh
        = ("hoanglinh.business@yahoo.com", "123456", "Hoang Linh", Avatar(10));
    public static readonly (string Email, string Password, string DisplayName, string AvatarUrl) HongPham
        = ("hong.pham88@gmail.com", "123456", "Hong Pham", Avatar(11));
    public static readonly (string Email, string Password, string DisplayName, string AvatarUrl) NhuHong
        = ("nhuhong.hr@gmail.com", "123456", "Nhu Hong", Avatar(12));
    public static readonly (string Email, string Password, string DisplayName, string AvatarUrl) HongVu
        = ("hongvu.marketing@hotmail.com", "123456", "Hong Vu", Avatar(13));

    private static readonly UserSeedInfo[] AllUsers =
    [
        new(LongFpt.Email,     LongFpt.Password,     LongFpt.DisplayName,     LongFpt.AvatarUrl),
        new(PhiLong.Email,     PhiLong.Password,     PhiLong.DisplayName,     PhiLong.AvatarUrl),
        new(PhiLongKoko.Email, PhiLongKoko.Password, PhiLongKoko.DisplayName, PhiLongKoko.AvatarUrl),
        new(PhiLongTik.Email,  PhiLongTik.Password,  PhiLongTik.DisplayName,  PhiLongTik.AvatarUrl),
        new(NhatThang.Email,   NhatThang.Password,   NhatThang.DisplayName,   NhatThang.AvatarUrl),
        new(ThangFpt.Email,    ThangFpt.Password,     ThangFpt.DisplayName,   ThangFpt.AvatarUrl),
        new(ThangNguyen.Email, ThangNguyen.Password, ThangNguyen.DisplayName, ThangNguyen.AvatarUrl),
        new(CatLinh.Email,     CatLinh.Password,     CatLinh.DisplayName,     CatLinh.AvatarUrl),
        new(LinhTran.Email,    LinhTran.Password,    LinhTran.DisplayName,    LinhTran.AvatarUrl),
        new(HoangLinh.Email,   HoangLinh.Password,   HoangLinh.DisplayName,  HoangLinh.AvatarUrl),
        new(HongPham.Email,    HongPham.Password,    HongPham.DisplayName,    HongPham.AvatarUrl),
        new(NhuHong.Email,     NhuHong.Password,     NhuHong.DisplayName,     NhuHong.AvatarUrl),
        new(HongVu.Email,      HongVu.Password,      HongVu.DisplayName,      HongVu.AvatarUrl),
    ];

    public static async Task SeedAsync(ApplicationDbContext db, ISender mediator, ILogger logger, CancellationToken ct = default)
    {
        var created = new List<string>();
        var skipped = new List<string>();

        foreach (var user in AllUsers)
        {
            try
            {
                var result = await DataSeederHelper.SeedUserAsync(
                    db, user.Email, user.Password, user.DisplayName, mediator, logger, user.AvatarUrl, ct);
                (result.WasCreated ? created : skipped).Add(user.Email);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to seed user {Email} — skipping.", user.Email);
                skipped.Add(user.Email);
            }
        }

        logger.LogInformation(
            "Users — created {Created}, skipped {Skipped}. Created: [{CreatedNames}] Skipped: [{SkippedNames}]",
            created.Count, skipped.Count,
            string.Join(", ", created),
            string.Join(", ", skipped));
    }
}
