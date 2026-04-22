namespace CashitoBackend.Credits.Interfaces.REST.Resources;

public record SimulationResponseResource(
    decimal Cuota,
    decimal Tcea,
    decimal Van,
    decimal Tir,
    IEnumerable<InstallmentResource> Schedule
);