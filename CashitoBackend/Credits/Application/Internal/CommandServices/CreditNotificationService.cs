using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashitoBackend.Clients.Domain.Repositories;
using CashitoBackend.Credits.Domain.Model.Aggregates;
using CashitoBackend.IAM.Application.Internal.OutboundServices;
using CashitoBackend.IAM.Domain.Model.Queries;
using CashitoBackend.IAM.Domain.Services;
using CashitoBackend.Notifications.Domain.Model.Commands;
using CashitoBackend.Notifications.Domain.Model.ValueObjects;
using CashitoBackend.Notifications.Domain.Services;
using CashitoBackend.Shared.Domain.Model.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CashitoBackend.Credits.Application.Internal.CommandServices;

/// <summary>
/// Handles all notification and email side-effects triggered by credit state changes.
/// Used by both CreditCommandService (authenticated) and CreditPublicService (public token).
/// </summary>
public class CreditNotificationService
{
    private readonly IClientRepository _clientRepository;
    private readonly IUserQueryService _userQueryService;
    private readonly IEmailService _emailService;
    private readonly INotificationCommandService _notificationCommandService;
    private readonly ILogger<CreditNotificationService> _logger;

    public CreditNotificationService(
        IClientRepository clientRepository,
        IUserQueryService userQueryService,
        IEmailService emailService,
        INotificationCommandService notificationCommandService,
        ILogger<CreditNotificationService> logger)
    {
        _clientRepository = clientRepository;
        _userQueryService = userQueryService;
        _emailService = emailService;
        _notificationCommandService = notificationCommandService;
        _logger = logger;
    }

    // =========================
    // PUBLIC TRIGGER METHODS
    // =========================

    public async Task OnCreditApprovedAsync(Credit credit)
    {
        const string clientSubject = "Credit Approved";
        var clientDescription = "Congratulations! Your vehicle credit request has been approved by our financial team. We are excited to help you get on the road.";
        var clientDetails = new Dictionary<string, string>
        {
            { "Credit ID", $"#{credit.Id}" },
            { "Client ID", $"#{credit.ClientId}" },
            { "Financed Amount", FormatCurrency(credit.FinancedAmount, credit.Currency) },
            { "Term", $"{credit.TermMonths} Months" },
            { "Date & Time", FormatDateTime(DateTime.UtcNow.AddHours(-5)) }
        };
        var clientBody = GetHtmlEmailBody(
            "Credit Approved",
            clientDescription,
            "linear-gradient(135deg, #10b981, #059669)", // emerald green
            "🎉",
            clientDetails
        );

        const string advisorSubject = "Credit Approved Notification";
        var advisorDescription = $"Credit #{credit.Id} for client #{credit.ClientId} was approved and has transitioned to Approved status.";
        var advisorDetails = new Dictionary<string, string>
        {
            { "Credit ID", $"#{credit.Id}" },
            { "Client ID", $"#{credit.ClientId}" },
            { "Financed Amount", FormatCurrency(credit.FinancedAmount, credit.Currency) },
            { "Status", "Approved" },
            { "Date & Time", FormatDateTime(DateTime.UtcNow.AddHours(-5)) }
        };
        var advisorBody = GetHtmlEmailBody(
            "Credit Approved",
            advisorDescription,
            "linear-gradient(135deg, #0ea5e9, #0284c7)", // sky/blue
            "✅",
            advisorDetails
        );

        await SendBothEmailsAsync(credit, clientSubject, clientBody, advisorSubject, advisorBody);
        await TryCreateNotificationAsync(credit.UserId, "Credit Approved",
            $"Credit #{credit.Id} was approved.", NotificationType.CreditApproved);
    }

    public async Task OnCreditRejectedAsync(Credit credit)
    {
        const string clientSubject = "Credit Request Declined";
        var clientDescription = "We regret to inform you that your vehicle credit request was not approved at this time. If you have any questions, please contact your financial advisor.";
        var clientDetails = new Dictionary<string, string>
        {
            { "Credit ID", $"#{credit.Id}" },
            { "Client ID", $"#{credit.ClientId}" },
            { "Financed Amount", FormatCurrency(credit.FinancedAmount, credit.Currency) },
            { "Date & Time", FormatDateTime(DateTime.UtcNow.AddHours(-5)) }
        };
        var clientBody = GetHtmlEmailBody(
            "Credit Request Declined",
            clientDescription,
            "linear-gradient(135deg, #ef4444, #dc2626)", // red
            "❌",
            clientDetails
        );

        const string advisorSubject = "Credit Rejected Notification";
        var advisorDescription = $"Credit #{credit.Id} for client #{credit.ClientId} was rejected.";
        var advisorDetails = new Dictionary<string, string>
        {
            { "Credit ID", $"#{credit.Id}" },
            { "Client ID", $"#{credit.ClientId}" },
            { "Status", "Rejected" },
            { "Date & Time", FormatDateTime(DateTime.UtcNow.AddHours(-5)) }
        };
        var advisorBody = GetHtmlEmailBody(
            "Credit Rejected",
            advisorDescription,
            "linear-gradient(135deg, #f97316, #ea580c)", // orange
            "⚠️",
            advisorDetails
        );

        await SendBothEmailsAsync(credit, clientSubject, clientBody, advisorSubject, advisorBody);
        await TryCreateNotificationAsync(credit.UserId, "Credit Rejected",
            $"Credit #{credit.Id} was rejected.", NotificationType.CreditRejected);
    }

    public async Task OnInstallmentPaidAsync(Credit credit, int installmentNumber, decimal amount)
    {
        var clientSubject = "Installment Payment Received";
        var clientDescription = $"We have successfully processed the payment for installment #{installmentNumber} of your credit.";
        var clientDetails = new Dictionary<string, string>
        {
            { "Credit ID", $"#{credit.Id}" },
            { "Installment", $"#{installmentNumber}" },
            { "Amount Paid", FormatCurrency(amount, credit.Currency) },
            { "Credit Status", credit.Status.ToString() },
            { "Date & Time", FormatDateTime(DateTime.UtcNow.AddHours(-5)) }
        };
        var clientBody = GetHtmlEmailBody(
            "Installment Paid",
            clientDescription,
            "linear-gradient(135deg, #3b82f6, #2563eb)", // blue
            "💳",
            clientDetails
        );

        var advisorSubject = "Installment Payment Received";
        var advisorDescription = $"Installment #{installmentNumber} for credit #{credit.Id} was paid by the client.";
        var advisorDetails = new Dictionary<string, string>
        {
            { "Credit ID", $"#{credit.Id}" },
            { "Client ID", $"#{credit.ClientId}" },
            { "Installment", $"#{installmentNumber}" },
            { "Amount Paid", FormatCurrency(amount, credit.Currency) },
            { "Credit Status", credit.Status.ToString() },
            { "Date & Time", FormatDateTime(DateTime.UtcNow.AddHours(-5)) }
        };
        var advisorBody = GetHtmlEmailBody(
            "Installment Paid",
            advisorDescription,
            "linear-gradient(135deg, #6366f1, #4f46e5)", // indigo
            "💰",
            advisorDetails
        );

        await SendBothEmailsAsync(credit, clientSubject, clientBody, advisorSubject, advisorBody);
        await TryCreateNotificationAsync(credit.UserId, "Installment Paid",
            $"Installment #{installmentNumber} for credit #{credit.Id} was paid. Amount: {amount:0.00}",
            NotificationType.InstallmentPaid);
    }

    public async Task OnCreditCompletedAsync(Credit credit)
    {
        const string clientSubject = "Credit Fully Paid!";
        var clientDescription = "Congratulations! Your vehicle credit has been fully paid off. We appreciate your preference and trust in Cashito.";
        var clientDetails = new Dictionary<string, string>
        {
            { "Credit ID", $"#{credit.Id}" },
            { "Client ID", $"#{credit.ClientId}" },
            { "Total Financed", FormatCurrency(credit.FinancedAmount, credit.Currency) },
            { "Status", "Completed" },
            { "Date & Time", FormatDateTime(DateTime.UtcNow.AddHours(-5)) }
        };
        var clientBody = GetHtmlEmailBody(
            "Credit Fully Paid!",
            clientDescription,
            "linear-gradient(135deg, #8b5cf6, #7c3aed)", // purple
            "🏆",
            clientDetails
        );

        const string advisorSubject = "Credit Completed Notification";
        var advisorDescription = $"Credit #{credit.Id} for client #{credit.ClientId} has been fully paid.";
        var advisorDetails = new Dictionary<string, string>
        {
            { "Credit ID", $"#{credit.Id}" },
            { "Client ID", $"#{credit.ClientId}" },
            { "Status", "Completed" },
            { "Date & Time", FormatDateTime(DateTime.UtcNow.AddHours(-5)) }
        };
        var advisorBody = GetHtmlEmailBody(
            "Credit Completed",
            advisorDescription,
            "linear-gradient(135deg, #10b981, #059669)", // emerald green
            "✨",
            advisorDetails
        );

        await SendBothEmailsAsync(credit, clientSubject, clientBody, advisorSubject, advisorBody);
        await TryCreateNotificationAsync(credit.UserId, "Credit Completed",
            $"Credit #{credit.Id} has been fully paid.", NotificationType.CreditCompleted);
    }

    // =========================
    // PRIVATE HELPERS
    // =========================

    private async Task SendBothEmailsAsync(
        Credit credit,
        string clientSubject, string clientBody,
        string advisorSubject, string advisorBody)
    {
        var clientTask = TrySendClientEmailAsync(credit, clientSubject, clientBody);
        var advisorTask = TrySendAdvisorEmailAsync(credit, advisorSubject, advisorBody);
        await Task.WhenAll(clientTask, advisorTask);
    }

    private async Task TrySendClientEmailAsync(Credit credit, string subject, string body)
    {
        try
        {
            _logger.LogInformation("[EMAIL] Looking up client {ClientId} for credit {CreditId}",
                credit.ClientId, credit.Id);

            var client = await _clientRepository.FindByIdAsync(credit.ClientId);
            if (client == null)
            {
                _logger.LogWarning("[EMAIL] Client not found for ClientId {ClientId} — skipping client email",
                    credit.ClientId);
                return;
            }

            var emailAddress = client.Email.ToString();
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                _logger.LogWarning("[EMAIL] Client email is empty for ClientId {ClientId} — skipping client email",
                    credit.ClientId);
                return;
            }

            _logger.LogInformation("[EMAIL] Client email found: {Email}", emailAddress);
            _logger.LogInformation("[EMAIL] Sending client email to {Email} | Subject: {Subject}", emailAddress, subject);
            await _emailService.SendAsync(emailAddress, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EMAIL] Failed to send client email for credit {CreditId}: {Message}",
                credit.Id, ex.Message);
        }
    }

    private async Task TrySendAdvisorEmailAsync(Credit credit, string subject, string body)
    {
        try
        {
            _logger.LogInformation("[EMAIL] Looking up advisor (UserId={UserId}) for credit {CreditId}",
                credit.UserId, credit.Id);

            var user = await _userQueryService.Handle(new GetUserByIdQuery(credit.UserId));
            if (user == null)
            {
                _logger.LogWarning("[EMAIL] Advisor not found for UserId {UserId} — skipping advisor email",
                    credit.UserId);
                return;
            }

            if (!user.Email.HasValue || string.IsNullOrWhiteSpace(user.Email.Value.Value))
            {
                _logger.LogWarning("[EMAIL] Advisor email is empty for UserId {UserId} — skipping advisor email",
                    credit.UserId);
                return;
            }

            var emailAddress = user.Email.Value.Value;
            _logger.LogInformation("[EMAIL] Advisor email found: {Email}", emailAddress);
            _logger.LogInformation("[EMAIL] Sending advisor email to {Email} | Subject: {Subject}", emailAddress, subject);
            await _emailService.SendAsync(emailAddress, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EMAIL] Failed to send advisor email for credit {CreditId}: {Message}",
                credit.Id, ex.Message);
        }
    }

    private async Task TryCreateNotificationAsync(
        int userId, string title, string message, NotificationType type)
    {
        try
        {
            await _notificationCommandService.Handle(
                new CreateNotificationCommand(userId, title, message, type));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[NOTIFICATION] Failed to create notification for UserId {UserId}: {Message}",
                userId, ex.Message);
        }
    }

    private string GetHtmlEmailBody(
        string title,
        string description,
        string headerBgColor,
        string icon,
        Dictionary<string, string> details)
    {
        var detailsRows = string.Join("\n", details.Select(kvp => 
            $@"<tr>
                <td style=""padding: 10px 0; font-size: 14px; font-weight: 500; color: #64748b; width: 40%; text-align: left; border-bottom: 1px solid #f1f5f9;"">{kvp.Key}</td>
                <td style=""padding: 10px 0; font-size: 14px; font-weight: 600; color: #1e293b; text-align: right; border-bottom: 1px solid #f1f5f9;"">{kvp.Value}</td>
            </tr>"
        ));

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{title}</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f8fafc; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; -webkit-font-smoothing: antialiased; color: #1e293b;"">
    <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; margin: 40px auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -4px rgba(0, 0, 0, 0.1); border: 1px solid #e2e8f0;"">
        <!-- Header -->
        <tr>
            <td align=""center"" style=""background: {headerBgColor}; padding: 32px 24px; color: #ffffff;"">
                <div style=""font-size: 28px; font-weight: 800; margin-bottom: 6px; letter-spacing: -0.025em;"">
                    <span style=""font-size: 32px; vertical-align: middle; margin-right: 8px;"">💵</span>Cashito
                </div>
                <div style=""font-size: 12px; font-weight: 600; opacity: 0.9; letter-spacing: 0.05em; text-transform: uppercase;"">Official Banking Notification</div>
            </td>
        </tr>
        <!-- Content Card -->
        <tr>
            <td style=""padding: 40px 32px;"">
                <div style=""background-color: #ffffff; border-radius: 12px;"">
                    <div style=""text-align: center; margin-bottom: 32px;"">
                        <div style=""display: inline-block; padding: 16px; background-color: #f1f5f9; border-radius: 50%; margin-bottom: 16px;"">
                            <span style=""font-size: 40px; display: inline-block; line-height: 1; vertical-align: middle;"">{icon}</span>
                        </div>
                        <h2 style=""margin: 0; color: #0f172a; font-size: 24px; font-weight: 700; letter-spacing: -0.025em;"">{title}</h2>
                    </div>
                    
                    <p style=""font-size: 15px; line-height: 1.6; color: #475569; margin: 0 0 32px 0; text-align: center;"">
                        {description}
                    </p>

                    <!-- Details Table -->
                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #f8fafc; border-radius: 12px; padding: 20px; border: 1px solid #f1f5f9; border-collapse: collapse;"">
                        {detailsRows}
                    </table>
                </div>
            </td>
        </tr>
        <!-- Footer -->
        <tr>
            <td align=""center"" style=""background-color: #f8fafc; padding: 24px; border-top: 1px solid #f1f5f9; color: #94a3b8; font-size: 12px; line-height: 1.5;"">
                <p style=""margin: 0 0 8px 0; font-weight: 500;"">This is a secure automated notification from Cashito. Please do not reply directly to this email.</p>
                <p style=""margin: 0;"">&copy; {DateTime.UtcNow.Year} Cashito. All rights reserved.</p>
            </td>
        </tr>
    </table>
</body>
</html>
";
    }

    private static string FormatCurrency(decimal amount, Currency currency)
    {
        var symbol = currency == Currency.PEN ? "S/." : "$";
        return $"{symbol} {amount:N2}";
    }

    private static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToString("dd/MM/yyyy hh:mm tt");
    }
}
