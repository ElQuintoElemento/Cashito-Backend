namespace CashitoBackend.Dashboard.Domain.Model.ReadModels;

public record DashboardPortfolioStats(
    int TotalCredits,
    decimal TotalVolume,
    decimal AverageInterestRate);
