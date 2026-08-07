using CashitoBackend.Dashboard.Domain.Model.ReadModels;
using CashitoBackend.Dashboard.Interfaces.REST.Resources;

namespace CashitoBackend.Dashboard.Interfaces.REST.Transform;

public static class DashboardPortfolioSummaryFromStatsAssembler
{
    public static DashboardPortfolioSummaryResource ToResourceFromStats(DashboardPortfolioStats stats) =>
        new DashboardPortfolioSummaryResource(
            stats.TotalCredits,
            stats.TotalVolume,
            stats.AverageInterestRate);
}
