using CashitoBackend.Credits.Application.Internal.DTOs;
using CashitoBackend.Credits.Domain.Model.Aggregates;
using CashitoBackend.Credits.Domain.Model.Commands;

namespace CashitoBackend.Credits.Domain.Services;

public interface ICreditCommandService
{
    Task<SimulationResult> Handle(SimulateCreditCommand command, int userId);

    Task<Credit> Handle(CreateCreditCommand command, int userId);

    Task<bool> Delete(int creditId, int userId);
}