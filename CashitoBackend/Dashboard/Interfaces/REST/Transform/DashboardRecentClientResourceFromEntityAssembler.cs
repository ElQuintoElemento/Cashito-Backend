using CashitoBackend.Clients.Domain.Model.Aggregates;
using CashitoBackend.Dashboard.Interfaces.REST.Resources;

namespace CashitoBackend.Dashboard.Interfaces.REST.Transform;

public static class DashboardRecentClientResourceFromEntityAssembler
{
    public static DashboardRecentClientResource ToResourceFromEntity(Client entity) =>
        new DashboardRecentClientResource(
            entity.Id,
            $"{entity.FirstName} {entity.LastName}".Trim(),
            entity.Dni.Value,
            entity.Phone?.Value ?? string.Empty,
            entity.Email.ToString());
}
