namespace CashitoBackend.Dashboard.Interfaces.REST.Resources;

public record DashboardPortfolioSummaryResource(
    int TotalCredits,
    decimal TotalVolume,
    decimal AverageInterestRate);
