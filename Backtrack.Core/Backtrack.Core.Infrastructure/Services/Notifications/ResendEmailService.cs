using Backtrack.Core.Application.Interfaces.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Backtrack.Core.Infrastructure.Services.Notifications;

public sealed class ResendEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly string _defaultFrom;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ResendEmailService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Resend");
        _defaultFrom = configuration["Resend:DefaultFrom"]
            ?? throw new InvalidOperationException("Resend:DefaultFrom is required.");
        _logger = logger;
    }

    public async Task<bool> SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken ct = default)
    {
        var payload = new
        {
            from = _defaultFrom,
            to = new[] { to },
            subject,
            html = htmlBody,
            text = textBody,
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("emails", content, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Resend API error {Status}: {Error}", response.StatusCode, error);
                return false;
            }

            _logger.LogInformation("Email sent to {To} via Resend", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return false;
        }
    }
}
