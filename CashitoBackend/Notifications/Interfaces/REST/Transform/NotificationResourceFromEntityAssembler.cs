using CashitoBackend.Notifications.Domain.Model.Aggregates;
using CashitoBackend.Notifications.Interfaces.REST.Resources;

namespace CashitoBackend.Notifications.Interfaces.REST.Transform;

public static class NotificationResourceFromEntityAssembler
{
    public static NotificationResource ToResourceFromEntity(Notification entity) =>
        new NotificationResource(
            entity.Id,
            entity.Title,
            entity.Message,
            entity.Type.ToString(),
            entity.IsRead,
            entity.CreatedAt);
}
