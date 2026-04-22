namespace CashitoBackend.Credits.Domain.Model.Aggregates;

using CashitoBackend.Credits.Domain.Model.Entities;
using CashitoBackend.Credits.Domain.Model.Exceptions;

public class Credit
{
    public int Id { get; private set; }

    public int UserId { get; private set; }
    public int ClientId { get; private set; }
    public int VehicleId { get; private set; }

    public decimal VehiclePrice { get; private set; }
    public decimal DownPayment { get; private set; }
    public decimal FinancedAmount { get; private set; }

    public decimal InterestRate { get; private set; }
    public int TermMonths { get; private set; }

    public string RateType { get; private set; } = "TEA";
    public int GracePeriod { get; private set; }

    public decimal Insurance { get; private set; }

    // indicadores
    public decimal Tcea { get; private set; }
    public decimal Van { get; private set; }
    public decimal Tir { get; private set; }

    // cronograma
    public List<Installment> Schedule { get; private set; } = new();

    protected Credit() { }

    public Credit(
        int userId,
        int clientId,
        int vehicleId,
        decimal vehiclePrice,
        decimal downPayment,
        decimal interestRate,
        int termMonths,
        string rateType,
        int gracePeriod,
        decimal insurance)
    {
        if (vehiclePrice <= 0)
            throw new CreditDomainException("Vehicle price must be greater than 0");

        if (downPayment < 0)
            throw new CreditDomainException("Invalid down payment");

        if (interestRate <= 0)
            throw new CreditDomainException("Interest rate must be greater than 0");

        if (termMonths <= 0)
            throw new CreditDomainException("Invalid term");

        UserId = userId;
        ClientId = clientId;
        VehicleId = vehicleId;

        VehiclePrice = vehiclePrice;
        DownPayment = downPayment;
        FinancedAmount = vehiclePrice - downPayment;

        InterestRate = interestRate;
        TermMonths = termMonths;
        RateType = rateType;
        GracePeriod = gracePeriod;
        Insurance = insurance;
    }

    public void SetResults(decimal tcea, decimal van, decimal tir)
    {
        Tcea = tcea;
        Van = van;
        Tir = tir;
    }

    public void SetSchedule(List<Installment> schedule)
    {
        Schedule = schedule;
    }
}