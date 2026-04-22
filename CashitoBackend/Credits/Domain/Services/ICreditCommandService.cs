using CashitoBackend.Credits.Domain.Model.Aggregates;
using CashitoBackend.Credits.Domain.Model.Commands;

namespace CashitoBackend.Credits.Domain.Services;

public interface ICreditCommandService
{
    // 🔥 SIMULACIÓN (NO GUARDA)
    Task<(decimal cuota,
            decimal tcea,
            decimal van,
            decimal tir,
            IEnumerable<object> schedule)>
        Handle(SimulateCreditCommand command, int userId);

    // 💾 CREAR CRÉDITO
    Task<Credit> Handle(CreateCreditCommand command, int userId);

    // 🗑️ OPCIONAL
    Task<bool> Delete(int creditId, int userId);
}