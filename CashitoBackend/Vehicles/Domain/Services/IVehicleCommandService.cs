using CashitoBackend.Vehicles.Domain.Model.Aggregates;
using CashitoBackend.Vehicles.Domain.Model.Commands;

namespace CashitoBackend.Vehicles.Domain.Services;

public interface IVehicleCommandService
{
    Task<Vehicle> Handle(CreateVehicleCommand command, int userId);

    Task<Vehicle?> Handle(UpdateVehicleCommand command, int userId);

    Task<bool> Handle(DeleteVehicleCommand command, int userId);
}