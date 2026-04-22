namespace CashitoBackend.Vehicles.Domain.Model.Commands;

public record UpdateVehicleCommand(
    int Id,
    string Brand,
    string Model,
    decimal Price,
    string Currency,
    int Year,
    string Type
);