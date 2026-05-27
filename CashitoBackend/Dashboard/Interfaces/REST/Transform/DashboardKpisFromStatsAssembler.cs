using CashitoBackend.Dashboard.Domain.Model.ReadModels;
using CashitoBackend.Dashboard.Interfaces.REST.Resources;

namespace CashitoBackend.Dashboard.Interfaces.REST.Transform;

public static class DashboardKpisFromStatsAssembler
{
    public static DashboardKpisResource ToResourceFromStats(DashboardKpisStats stats) =>
        new DashboardKpisResource(
            stats.TotalClients,
            stats.TotalVehicles,
            stats.ActiveCredits,
            stats.TotalCreditVolume);
}
