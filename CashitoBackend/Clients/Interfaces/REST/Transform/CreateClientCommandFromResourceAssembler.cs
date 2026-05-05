using CashitoBackend.Clients.Domain.Model.Commands;
using CashitoBackend.Clients.Interfaces.REST.Resources;

namespace CashitoBackend.Clients.Interfaces.REST.Transform;

public static class CreateClientCommandFromResourceAssembler
{
    public static CreateClientCommand ToCommandFromResource(CreateClientResource resource)
    {
        return new CreateClientCommand(
            resource.Dni,
            resource.FirstName,
            resource.LastName,
            resource.MonthlyIncome,
            resource.Phone,
            resource.Email
        );
    }
}