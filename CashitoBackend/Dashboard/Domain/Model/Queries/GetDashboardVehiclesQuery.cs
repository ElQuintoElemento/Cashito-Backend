namespace CashitoBackend.Dashboard.Domain.Model.Queries;

public record GetDashboardVehiclesQuery(int UserId, int Limit = 5);
