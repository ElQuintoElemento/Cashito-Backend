namespace CashitoBackend.Vehicles.Domain.Model.Commands;

public record CreateVehicleCommand(
    string Brand,
    string Model,
    decimal Price,
    string Currency,
    int Year,
    string Type
);