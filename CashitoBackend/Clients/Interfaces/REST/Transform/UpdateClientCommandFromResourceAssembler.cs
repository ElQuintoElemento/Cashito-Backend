using CashitoBackend.Clients.Domain.Model.Commands;
using CashitoBackend.Clients.Interfaces.REST.Resources;

namespace CashitoBackend.Clients.Interfaces.REST.Transform;

public static class UpdateClientCommandFromResourceAssembler
{
    public static UpdateClientCommand ToCommandFromResource(int id, UpdateClientResource resource)
    {
        return new UpdateClientCommand(
            id,
            resource.FirstName,
            resource.LastName,
            resource.MonthlyIncome,
            resource.Phone
        );
    }
}