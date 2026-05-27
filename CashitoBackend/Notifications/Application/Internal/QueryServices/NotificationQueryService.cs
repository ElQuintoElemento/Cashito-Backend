using CashitoBackend.Notifications.Domain.Model.Aggregates;
using CashitoBackend.Notifications.Domain.Model.Queries;
using CashitoBackend.Notifications.Domain.Repositories;
using CashitoBackend.Notifications.Domain.Services;

namespace CashitoBackend.Notifications.Application.Internal.QueryServices;

public class NotificationQueryService : INotificationQueryService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationQueryService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IReadOnlyList<Notification>> Handle(GetNotificationsByUserIdQuery query)
    {
        return await _notificationRepository.FindByUserIdAsync(query.UserId);
    }

    public async Task<int> Handle(GetUnreadCountQuery query)
    {
        return await _notificationRepository.CountUnreadByUserIdAsync(query.UserId);
    }
}
