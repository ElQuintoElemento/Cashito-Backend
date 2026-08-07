using CashitoBackend.Clients.Domain.Model.Commands;
using CashitoBackend.Clients.Interfaces.REST.Resources;
using CashitoBackend.Shared.Domain.Model.ValueObjects;

namespace CashitoBackend.Clients.Interfaces.REST.Transform;

public static class CreateClientCommandFromResourceAssembler
{
    public static CreateClientCommand ToCommandFromResource(CreateClientResource resource)
    {
        if (!Enum.TryParse<Currency>(resource.IncomeCurrency, true, out var currency))
            throw new Exception("Invalid currency");
        
        return new CreateClientCommand(
            resource.Dni,
            resource.FirstName,
            resource.LastName,
            resource.MonthlyIncome,
            currency,
            resource.Phone,
            resource.Email
        );
    }
}