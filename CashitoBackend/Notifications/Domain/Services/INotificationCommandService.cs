using CashitoBackend.Notifications.Domain.Model.Aggregates;
using CashitoBackend.Notifications.Domain.Model.Commands;

namespace CashitoBackend.Notifications.Domain.Services;

public interface INotificationCommandService
{
    Task<Notification> Handle(CreateNotificationCommand command);

    Task<bool> MarkAsRead(int notificationId, int userId);

    Task MarkAllAsRead(int userId);
}
