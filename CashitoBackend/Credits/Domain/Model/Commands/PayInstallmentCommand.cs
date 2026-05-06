namespace CashitoBackend.Credits.Domain.Model.Commands;

public record PayInstallmentCommand(
    int CreditId,
    int InstallmentNumber
);