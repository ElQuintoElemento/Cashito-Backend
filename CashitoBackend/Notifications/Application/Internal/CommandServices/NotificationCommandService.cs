using CashitoBackend.Notifications.Domain.Model.Aggregates;
using CashitoBackend.Notifications.Domain.Model.Commands;
using CashitoBackend.Notifications.Domain.Repositories;
using CashitoBackend.Notifications.Domain.Services;
using CashitoBackend.Shared.Domain.Repositories;

namespace CashitoBackend.Notifications.Application.Internal.CommandServices;

public class NotificationCommandService : INotificationCommandService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationCommandService(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Notification> Handle(CreateNotificationCommand command)
    {
        var notification = new Notification(
            command.UserId,
            command.Title,
            command.Message,
            command.Type);

        await _notificationRepository.AddAsync(notification);
        await _unitOfWork.CompleteAsync();

        return notification;
    }

    public async Task<bool> MarkAsRead(int notificationId, int userId)
    {
        var notification = await _notificationRepository.FindByIdAsync(notificationId);

        if (notification == null || notification.UserId != userId)
            return false;

        notification.MarkAsRead();
        _notificationRepository.Update(notification);
        await _unitOfWork.CompleteAsync();

        return true;
    }

    public async Task MarkAllAsRead(int userId)
    {
        var unread = await _notificationRepository.FindUnreadByUserIdAsync(userId);

        foreach (var notification in unread)
        {
            notification.MarkAsRead();
            _notificationRepository.Update(notification);
        }

        await _unitOfWork.CompleteAsync();
    }
}
