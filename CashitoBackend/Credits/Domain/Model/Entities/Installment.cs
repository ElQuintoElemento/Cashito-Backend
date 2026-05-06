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