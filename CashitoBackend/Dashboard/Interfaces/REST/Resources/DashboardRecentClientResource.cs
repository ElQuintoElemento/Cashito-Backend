namespace CashitoBackend.Dashboard.Interfaces.REST.Resources;

public record DashboardRecentClientResource(
    int Id,
    string FullName,
    string Dni,
    string Phone,
    string Email);
