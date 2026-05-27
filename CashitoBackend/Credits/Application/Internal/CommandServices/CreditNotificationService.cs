using CashitoBackend.Clients.Domain.Repositories;
using CashitoBackend.Credits.Domain.Model.Aggregates;
using CashitoBackend.IAM.Application.Internal.OutboundServices;
using CashitoBackend.IAM.Domain.Model.Queries;
using CashitoBackend.IAM.Domain.Services;
using CashitoBackend.Notifications.Domain.Model.Commands;
using CashitoBackend.Notifications.Domain.Model.ValueObjects;
using CashitoBackend.Notifications.Domain.Services;
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
        const string clientBody = "Your vehicle credit has been approved successfully.";
        const string advisorSubject = "Credit Approved";
        var advisorBody = $"Credit #{credit.Id} for client #{credit.ClientId} was approved.";

        await SendBothEmailsAsync(credit, clientSubject, clientBody, advisorSubject, advisorBody);
        await TryCreateNotificationAsync(credit.UserId, "Credit Approved",
            $"Credit #{credit.Id} was approved.", NotificationType.CreditApproved);
    }

    public async Task OnCreditRejectedAsync(Credit credit)
    {
        const string clientSubject = "Credit Rejected";
        const string clientBody = "Your vehicle credit request has been rejected.";
        const string advisorSubject = "Credit Rejected";
        var advisorBody = $"Credit #{credit.Id} for client #{credit.ClientId} was rejected.";

        await SendBothEmailsAsync(credit, clientSubject, clientBody, advisorSubject, advisorBody);
        await TryCreateNotificationAsync(credit.UserId, "Credit Rejected",
            $"Credit #{credit.Id} was rejected.", NotificationType.CreditRejected);
    }

    public async Task OnInstallmentPaidAsync(Credit credit, int installmentNumber, decimal amount)
    {
        var clientSubject = "Installment Paid";
        var clientBody = $"Installment #{installmentNumber} was paid successfully. Amount: {amount:0.00}";
        var advisorSubject = "Installment Paid";
        var advisorBody = $"Installment #{installmentNumber} for credit #{credit.Id} was paid. Amount: {amount:0.00}";

        await SendBothEmailsAsync(credit, clientSubject, clientBody, advisorSubject, advisorBody);
        await TryCreateNotificationAsync(credit.UserId, "Installment Paid",
            $"Installment #{installmentNumber} for credit #{credit.Id} was paid. Amount: {amount:0.00}",
            NotificationType.InstallmentPaid);
    }

    public async Task OnCreditCompletedAsync(Credit credit)
    {
        const string clientSubject = "Credit Completed";
        const string clientBody = "Your vehicle credit has been fully paid. Congratulations!";
        const string advisorSubject = "Credit Completed";
        var advisorBody = $"Credit #{credit.Id} for client #{credit.ClientId} has been fully paid.";

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
        await TrySendClientEmailAsync(credit, clientSubject, clientBody);
        await TrySendAdvisorEmailAsync(credit, advisorSubject, advisorBody);
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
}
