using System.Security.Cryptography;
using System.Text;

namespace Backtrack.Core.Application.Utils;

public static class QrCodeUtil
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int CodeLength = 8;
    private const string Prefix = "BTK-";

    /// <summary>Generates a unique QR public code in the format BTK-XXXXXXXX.</summary>
    public static string GeneratePublicCode()
    {
        var sb = new StringBuilder(Prefix.Length + CodeLength);
        sb.Append(Prefix);
        Span<byte> buffer = stackalloc byte[CodeLength];
        RandomNumberGenerator.Fill(buffer);
        foreach (var b in buffer)
            sb.Append(Chars[b % Chars.Length]);
        return sb.ToString();
    }
}
