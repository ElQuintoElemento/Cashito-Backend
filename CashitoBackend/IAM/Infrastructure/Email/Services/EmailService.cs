using CashitoBackend.IAM.Application.Internal.OutboundServices;
using CashitoBackend.IAM.Infrastructure.Email.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CashitoBackend.IAM.Infrastructure.Email.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            _logger.LogWarning("[EMAIL] Recipient address is empty — skipping send for subject: {Subject}", subject);
            return;
        }

        _logger.LogInformation("[EMAIL] Preparing email to {To} | Subject: {Subject}", to, subject);
        _logger.LogDebug("[EMAIL] SMTP server: {Server}:{Port} | Sender: {Sender}",
            _emailSettings.SmtpServer, _emailSettings.Port, _emailSettings.SenderEmail);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var smtpClient = new SmtpClient();
        smtpClient.Timeout = 5000; // 5 seconds timeout
        try
        {
            _logger.LogInformation("[EMAIL] Connecting to SMTP {Server}:{Port}...", _emailSettings.SmtpServer, _emailSettings.Port);
            await smtpClient.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);
            _logger.LogInformation("[EMAIL] SMTP connected. Authenticating as {Sender}...", _emailSettings.SenderEmail);

            await smtpClient.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
            _logger.LogInformation("[EMAIL] Authentication successful. Sending message...");

            await smtpClient.SendAsync(message);
            _logger.LogInformation("[EMAIL] Email sent successfully to {To}", to);
        }
        finally
        {
            if (smtpClient.IsConnected)
                await smtpClient.DisconnectAsync(true);
        }
    }
}
