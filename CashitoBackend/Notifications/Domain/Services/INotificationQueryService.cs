using CashitoBackend.Notifications.Domain.Model.Aggregates;
using CashitoBackend.Notifications.Domain.Model.Queries;

namespace CashitoBackend.Notifications.Domain.Services;

public interface INotificationQueryService
{
    Task<IReadOnlyList<Notification>> Handle(GetNotificationsByUserIdQuery query);

    Task<int> Handle(GetUnreadCountQuery query);
}
