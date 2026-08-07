namespace CashitoBackend.Vehicles.Interfaces.REST.Resources;

public record CreateVehicleResource(
    string Brand,
    string Model,
    decimal Price,
    string Currency,
    int Year,
    string Type
);