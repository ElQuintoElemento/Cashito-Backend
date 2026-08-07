using CashitoBackend.Shared.Domain.Model.ValueObjects;
using CashitoBackend.Vehicles.Domain.Model.ValueObjects;

namespace CashitoBackend.Vehicles.Domain.Model.Commands;

public record CreateVehicleCommand(
    string Brand,
    string Model,
    decimal Price,
    Currency Currency,
    int Year,
    VehicleType Type
);