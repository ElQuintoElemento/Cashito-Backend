namespace CashitoBackend.Notifications.Interfaces.REST.Resources;

public record NotificationResource(
    int Id,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTime CreatedAt);
