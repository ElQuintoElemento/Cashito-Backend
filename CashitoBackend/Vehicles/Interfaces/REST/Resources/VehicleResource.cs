namespace CashitoBackend.Vehicles.Interfaces.REST.Resources;

public record VehicleResource(
    int Id,
    string Brand,
    string Model,
    decimal Price,
    string Currency,
    int Year,
    string Type
);