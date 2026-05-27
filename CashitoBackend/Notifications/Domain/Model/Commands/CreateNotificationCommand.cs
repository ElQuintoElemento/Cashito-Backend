using CashitoBackend.Notifications.Domain.Model.ValueObjects;

namespace CashitoBackend.Notifications.Domain.Model.Commands;

public record CreateNotificationCommand(
    int UserId,
    string Title,
    string Message,
    NotificationType Type);
