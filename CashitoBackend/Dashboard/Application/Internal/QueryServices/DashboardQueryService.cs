using CashitoBackend.Clients.Domain.Model.Aggregates;
using CashitoBackend.Clients.Domain.Repositories;
using CashitoBackend.Credits.Domain.Repositories;
using CashitoBackend.Dashboard.Domain.Model.Queries;
using CashitoBackend.Dashboard.Domain.Model.ReadModels;
using CashitoBackend.Dashboard.Domain.Services;
using CashitoBackend.Vehicles.Domain.Model.Aggregates;
using CashitoBackend.Vehicles.Domain.Repositories;

namespace CashitoBackend.Dashboard.Application.Internal.QueryServices;

public class DashboardQueryService : IDashboardQueryService
{
    private readonly IClientRepository _clientRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ICreditRepository _creditRepository;

    public DashboardQueryService(
        IClientRepository clientRepository,
        IVehicleRepository vehicleRepository,
        ICreditRepository creditRepository)
    {
        _clientRepository = clientRepository;
        _vehicleRepository = vehicleRepository;
        _creditRepository = creditRepository;
    }

    public async Task<DashboardKpisStats> Handle(GetDashboardKpisQuery query)
    {
        var totalClients = await _clientRepository.CountByUserIdAsync(query.UserId);
        var totalVehicles = await _vehicleRepository.CountByUserIdAsync(query.UserId);
        var activeCredits = await _creditRepository.CountActiveByUserIdAsync(query.UserId);
        var totalCreditVolume = await _creditRepository.SumFinancedAmountByUserIdAsync(query.UserId);

        return new DashboardKpisStats(
            totalClients,
            totalVehicles,
            activeCredits,
            totalCreditVolume);
    }

    public async Task<IReadOnlyList<Client>> Handle(GetDashboardRecentClientsQuery query)
    {
        return await _clientRepository.FindRecentByUserIdAsync(query.UserId, query.Limit);
    }

    public async Task<IReadOnlyList<Vehicle>> Handle(GetDashboardVehiclesQuery query)
    {
        return await _vehicleRepository.FindRecentByUserIdAsync(query.UserId, query.Limit);
    }

    public async Task<DashboardPortfolioStats> Handle(GetDashboardPortfolioSummaryQuery query)
    {
        var totalCredits = await _creditRepository.CountByUserIdAsync(query.UserId);
        var totalVolume = await _creditRepository.SumFinancedAmountByUserIdAsync(query.UserId);
        var averageInterestRate = await _creditRepository.AverageInterestRateByUserIdAsync(query.UserId);

        return new DashboardPortfolioStats(
            totalCredits,
            totalVolume,
            averageInterestRate);
    }
}
