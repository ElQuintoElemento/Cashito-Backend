namespace CashitoBackend.Vehicles.Interfaces.REST.Resources;

public record UpdateVehicleResource(
    string Brand,
    string Model,
    decimal Price,
    string Currency,
    int Year,
    string Type
);