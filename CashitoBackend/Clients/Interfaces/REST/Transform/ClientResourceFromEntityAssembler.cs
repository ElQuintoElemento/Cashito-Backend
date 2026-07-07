using CashitoBackend.Clients.Domain.Model.Aggregates;
using CashitoBackend.Clients.Interfaces.REST.Resources;

namespace CashitoBackend.Clients.Interfaces.REST.Transform;

public static class ClientResourceFromEntityAssembler
{
    public static ClientResource ToResourceFromEntity(Client entity)
    {
        return new ClientResource(
            entity.Id,
            entity.Dni.Value,
            entity.FirstName,
            entity.LastName,
            entity.MonthlyIncome,
            entity.IncomeCurrency.ToString(),
            entity.Phone?.Value ?? string.Empty,
            entity.Email.ToString()
        );
    }
}