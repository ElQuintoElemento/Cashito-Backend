namespace CashitoBackend.Dashboard.Domain.Model.ReadModels;

public record DashboardKpisStats(
    int TotalClients,
    int TotalVehicles,
    int ActiveCredits,
    decimal TotalCreditVolume);
