using CashitoBackend.Clients.Domain.Model.Commands;
using CashitoBackend.Clients.Interfaces.REST.Resources;
using CashitoBackend.Shared.Domain.Model.ValueObjects;

namespace CashitoBackend.Clients.Interfaces.REST.Transform;

public static class UpdateClientCommandFromResourceAssembler
{
    public static UpdateClientCommand ToCommandFromResource(int id, UpdateClientResource resource)
    {
        if (!Enum.TryParse<Currency>(resource.IncomeCurrency, true, out var currency))
            throw new Exception("Invalid currency");
        
        return new UpdateClientCommand(
            id,
            resource.FirstName,
            resource.LastName,
            resource.MonthlyIncome,
            currency,
            resource.Phone,
            resource.Email
        );
    }
}