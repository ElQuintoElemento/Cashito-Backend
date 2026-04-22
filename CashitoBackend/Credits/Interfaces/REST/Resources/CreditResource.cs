namespace CashitoBackend.Credits.Interfaces.REST.Resources;

public record CreditResource(
    int Id,
    int ClientId,
    int VehicleId,
    decimal VehiclePrice,
    decimal DownPayment,
    decimal FinancedAmount,
    decimal Tcea,
    decimal Van,
    decimal Tir
);