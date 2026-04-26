using Backtrack.Core.Application.Events;
using Backtrack.Core.Application.Interfaces.Email;
using Backtrack.Core.Domain.Constants;
using DotNetCore.CAP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Messaging.Consumers;

public sealed class InvitationCreatedConsumer : ICapSubscribe
{
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InvitationCreatedConsumer> _logger;

    public InvitationCreatedConsumer(IEmailService emailService, IConfiguration configuration, ILogger<InvitationCreatedConsumer> logger)
    {
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    [CapSubscribe(EventTopics.Invitation.Created)]
    public async Task HandleAsync(InvitationCreatedIntegrationEvent @event)
    {
        _logger.LogInformation("Processing InvitationCreatedEvent for {Email}, org {Org}", @event.Email, @event.OrganizationName);

        var consoleDomain = _configuration["BacktrackConsoleWebDomain"]
            ?? throw new InvalidOperationException("BacktrackConsoleWebDomain is required.");

        var joinUrl = $"{consoleDomain}/auth/signin-or-signup?code={@event.HashCode}&email={Uri.EscapeDataString(@event.Email)}";

        var diffHours = Math.Max(0, (int)(@event.ExpiredTime - DateTimeOffset.UtcNow).TotalHours);

        var html = BuildInvitationHtml(@event.OrganizationName, joinUrl, diffHours);
        var text = $"Hello, you have been invited to join {@event.OrganizationName} on Backtrack. Join here: {joinUrl}";

        var sent = await _emailService.SendAsync(
            @event.Email,
            $"Invitation to join {@event.OrganizationName} on Backtrack",
            html,
            text);

        if (sent)
            _logger.LogInformation("Invitation email sent to {Email}", @event.Email);
        else
            _logger.LogWarning("Failed to send invitation email to {Email}", @event.Email);
    }

    private static string BuildInvitationHtml(string organizationName, string joinUrl, int expireHours)
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "invitation-email.html");

        if (!File.Exists(templatePath))
        {
            templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "invitation-email.html");
        }

        if (!File.Exists(templatePath))
            return $"<p>You have been invited to join <strong>{organizationName}</strong>. <a href='{joinUrl}'>Click here to join</a>. Link expires in {expireHours} hours.</p>";

        return File.ReadAllText(templatePath)
            .Replace("{{organizationName}}", organizationName)
            .Replace("{{joinUrl}}", joinUrl)
            .Replace("{{expireHours}}", expireHours.ToString());
    }
}
