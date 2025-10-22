using DBIntegrationHub.Application.Abstractions.Email;
using Microsoft.Extensions.Logging;

namespace DBIntegrationHub.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual email sending logic
        _logger.LogInformation(
            "E-posta gönderiliyor - Kime: {To}, Konu: {Subject}",
            to,
            subject);

        return Task.CompletedTask;
    }
}

