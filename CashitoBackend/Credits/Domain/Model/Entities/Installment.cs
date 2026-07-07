using CashitoBackend.Credits.Domain.Model.Exceptions;
using CashitoBackend.Credits.Domain.Model.ValueObjects;

namespace CashitoBackend.Credits.Domain.Model.Entities;

public class Installment
{
    public int Id { get; private set; }
    
    public int CreditId { get; private set; }

    public int Number { get; private set; }

    public DateTime Date { get; private set; }

    public decimal TotalPayment { get; private set; }

    public decimal Interest { get; private set; }

    public decimal Amortization { get; private set; }

    public decimal RemainingBalance { get; private set; }

    public bool IsPaid { get; private set; } = false;

    public DateTime? PaidAt { get; private set; }

    // Missing fields added in Package 3
    public decimal BaseInstallment { get; set; }
    public decimal BeginningBalance { get; set; }
    public decimal DesgravamenInsurance { get; set; }
    public decimal VehicularInsurance { get; set; }
    public decimal Portes { get; set; }
    public decimal OtherExpenses { get; set; }
    public decimal CashFlow { get; set; }
    public bool IsBalloon { get; set; }
    public decimal BalloonAmount { get; set; }

    protected Installment() { }

    public Installment(
        int number,
        DateTime date,
        decimal totalPayment,
        decimal interest,
        decimal amortization,
        decimal remainingBalance)
    {
        Number = number;
        Date = date;
        TotalPayment = totalPayment;
        Interest = interest;
        Amortization = amortization;
        RemainingBalance = remainingBalance;
    }

    public void MarkAsPaid()
    {
        if (IsPaid)
            throw new CreditDomainException("Installment already paid");

        IsPaid = true;
        PaidAt = DateTime.UtcNow;
    }

    public InstallmentStatus GetStatus()
    {
        if (IsPaid) return InstallmentStatus.Paid;

        if (Date < DateTime.UtcNow)
            return InstallmentStatus.Overdue;

        return InstallmentStatus.Pending;
    }
}