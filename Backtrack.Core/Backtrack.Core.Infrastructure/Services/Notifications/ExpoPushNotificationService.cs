using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backtrack.Core.Application.Interfaces.PushNotification;
using Backtrack.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Services.Notifications;

file sealed record ExpoPushMessage(
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("sound")] string Sound,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("body")] string? Body,
    [property: JsonPropertyName("data")] object? Data);

file sealed record ExpoPushResult(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("message")] string? Message);

file sealed record ExpoPushTicket(
    [property: JsonPropertyName("data")] ExpoPushResult[] Data);

public sealed class ExpoPushNotificationService : IPushNotificationService
{
    private const string ExpoPushUrl = "https://exp.host/--/api/v2/push/send";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExpoPushNotificationService> _logger;

    public ExpoPushNotificationService(IHttpClientFactory httpClientFactory, ILogger<ExpoPushNotificationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendAsync(IEnumerable<string> tokens, string title, string body, NotificationData? data = null, CancellationToken ct = default)
    {
        var tokenList = tokens.ToList();
        if (tokenList.Count == 0)
            return;

        var dataPayload = data != null
            ? JsonSerializer.SerializeToElement(data, SerializerOptions)
            : (JsonElement?)null;

        var messages = tokenList.Select(token => new ExpoPushMessage(
            To: token,
            Sound: "default",
            Title: title,
            Body: body,
            Data: dataPayload)).ToList();

        _logger.LogInformation("Sending push to {Count} tokens via Expo", tokenList.Count);

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync(ExpoPushUrl, messages, SerializerOptions, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Expo Push API error {Status}: {Body}", response.StatusCode, errorText);
                return;
            }

            var ticket = await response.Content.ReadFromJsonAsync<ExpoPushTicket>(SerializerOptions, ct);
            if (ticket is null)
                return;

            var successCount = ticket.Data.Count(r => r.Status == "ok");
            var failureCount = ticket.Data.Length - successCount;

            _logger.LogInformation("Expo push results: {Success} sent, {Failure} failed", successCount, failureCount);

            foreach (var result in ticket.Data.Where(r => r.Status != "ok"))
                _logger.LogWarning("Expo push failed: {Message}", result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Expo push notification");
        }
    }
}
