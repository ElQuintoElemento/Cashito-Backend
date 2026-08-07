namespace CashitoBackend.Dashboard.Interfaces.REST.Resources;

public record DashboardKpisResource(
    int TotalClients,
    int TotalVehicles,
    int ActiveCredits,
    decimal TotalCreditVolume);
