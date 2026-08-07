namespace CashitoBackend.Dashboard.Interfaces.REST.Resources;

public record DashboardVehicleResource(
    int Id,
    string Brand,
    string Model,
    int Year,
    decimal Price,
    string Currency);
