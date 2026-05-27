using CashitoBackend.Notifications.Domain.Model.ValueObjects;

namespace CashitoBackend.Notifications.Domain.Model.Aggregates;

public class Notification
{
    public int Id { get; private set; }

    public int UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public NotificationType Type { get; private set; }

    public bool IsRead { get; private set; }

    public DateTime CreatedAt { get; private set; }

    protected Notification() { }

    public Notification(int userId, string title, string message, NotificationType type)
    {
        UserId = userId;
        Title = title;
        Message = message;
        Type = type;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
