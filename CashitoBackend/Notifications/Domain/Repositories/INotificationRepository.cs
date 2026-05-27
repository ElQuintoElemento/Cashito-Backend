using CashitoBackend.Notifications.Domain.Model.Aggregates;

namespace CashitoBackend.Notifications.Domain.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);

    Task<Notification?> FindByIdAsync(int id);

    Task<IReadOnlyList<Notification>> FindByUserIdAsync(int userId);

    Task<int> CountUnreadByUserIdAsync(int userId);

    Task<IReadOnlyList<Notification>> FindUnreadByUserIdAsync(int userId);

    void Update(Notification notification);
}
