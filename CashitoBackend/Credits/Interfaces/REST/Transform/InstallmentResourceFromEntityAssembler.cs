using CashitoBackend.Credits.Domain.Model.Entities;
using CashitoBackend.Credits.Interfaces.REST.Resources;

namespace CashitoBackend.Credits.Interfaces.REST.Transform;

public static class InstallmentResourceFromEntityAssembler
{
    public static InstallmentResource ToResourceFromEntity(Installment i)
    {
        return new InstallmentResource(
            i.Number,
            i.Date,
            i.TotalPayment,
            i.Interest,
            i.Amortization,
            i.RemainingBalance,
            i.IsPaid,
            i.PaidAt,
            i.GetStatus().ToString()
        );
    }
}