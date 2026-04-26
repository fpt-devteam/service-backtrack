namespace Backtrack.Core.Application.Interfaces.Email;

public interface IEmailService
{
    Task<bool> SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken ct = default);
}
