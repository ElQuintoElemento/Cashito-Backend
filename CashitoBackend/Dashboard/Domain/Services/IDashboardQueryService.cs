using CashitoBackend.Clients.Domain.Model.Aggregates;
using CashitoBackend.Dashboard.Domain.Model.Queries;
using CashitoBackend.Dashboard.Domain.Model.ReadModels;
using CashitoBackend.Vehicles.Domain.Model.Aggregates;

namespace CashitoBackend.Dashboard.Domain.Services;

public interface IDashboardQueryService
{
    Task<DashboardKpisStats> Handle(GetDashboardKpisQuery query);

    Task<IReadOnlyList<Client>> Handle(GetDashboardRecentClientsQuery query);

    Task<IReadOnlyList<Vehicle>> Handle(GetDashboardVehiclesQuery query);

    Task<DashboardPortfolioStats> Handle(GetDashboardPortfolioSummaryQuery query);
}
