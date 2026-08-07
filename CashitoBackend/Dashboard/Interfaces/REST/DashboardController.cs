using CashitoBackend.Dashboard.Domain.Model.Queries;
using CashitoBackend.Dashboard.Domain.Services;
using CashitoBackend.Dashboard.Interfaces.REST.Transform;
using CashitoBackend.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using CashitoBackend.Shared.Infrastructure.Interfaces.ASP.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CashitoBackend.Dashboard.Interfaces.REST;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardQueryService _dashboardQueryService;

    public DashboardController(IDashboardQueryService dashboardQueryService)
    {
        _dashboardQueryService = dashboardQueryService;
    }

    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis()
    {
        var userId = User.GetUserId();
        var stats = await _dashboardQueryService.Handle(new GetDashboardKpisQuery(userId));
        return Ok(DashboardKpisFromStatsAssembler.ToResourceFromStats(stats));
    }

    [HttpGet("recent-clients")]
    public async Task<IActionResult> GetRecentClients()
    {
        var userId = User.GetUserId();
        var clients = await _dashboardQueryService.Handle(
            new GetDashboardRecentClientsQuery(userId, Limit: 5));
        var response = clients.Select(DashboardRecentClientResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(response);
    }

    [HttpGet("vehicles")]
    public async Task<IActionResult> GetVehicles()
    {
        var userId = User.GetUserId();
        var vehicles = await _dashboardQueryService.Handle(
            new GetDashboardVehiclesQuery(userId, Limit: 5));
        var response = vehicles.Select(DashboardVehicleResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(response);
    }

    [HttpGet("portfolio-summary")]
    public async Task<IActionResult> GetPortfolioSummary()
    {
        var userId = User.GetUserId();
        var stats = await _dashboardQueryService.Handle(new GetDashboardPortfolioSummaryQuery(userId));
        return Ok(DashboardPortfolioSummaryFromStatsAssembler.ToResourceFromStats(stats));
    }
}
