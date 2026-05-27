namespace CashitoBackend.Dashboard.Domain.Model.Queries;

public record GetDashboardRecentClientsQuery(int UserId, int Limit = 5);
