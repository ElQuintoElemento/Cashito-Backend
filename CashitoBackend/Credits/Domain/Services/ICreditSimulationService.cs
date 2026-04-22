using CashitoBackend.Credits.Application.Internal.DTOs;
using CashitoBackend.Credits.Domain.Model.Commands;

namespace CashitoBackend.Credits.Domain.Services;

public interface ICreditSimulationService
{
    SimulationResult Simulate(SimulateCreditCommand command);
}