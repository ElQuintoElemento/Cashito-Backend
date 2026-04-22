using CashitoBackend.Credits.Application.Internal.DTOs;
using CashitoBackend.Credits.Domain.Model.Commands;
using CashitoBackend.Credits.Domain.Model.Entities;
using CashitoBackend.Credits.Domain.Services;

namespace CashitoBackend.Credits.Application.Internal.CommandServices;

public class CreditSimulationService : ICreditSimulationService
{
    public SimulationResult Simulate(SimulateCreditCommand command)
    {
        var financedAmount = command.VehiclePrice - command.DownPayment;

        var monthlyRate = command.InterestRate / 100m / 12m;
        var n = command.TermMonths;

        var cuota = financedAmount *
                    (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), n)) /
                    ((decimal)Math.Pow((double)(1 + monthlyRate), n) - 1);

        var schedule = new List<Installment>();

        decimal balance = financedAmount;
        DateTime date = DateTime.UtcNow;

        for (int i = 1; i <= n; i++)
        {
            var interest = balance * monthlyRate;
            var amortization = cuota - interest;
            balance -= amortization;

            schedule.Add(new Installment(
                i,
                date.AddMonths(i),
                cuota,
                interest,
                amortization,
                balance < 0 ? 0 : balance
            ));
        }

        var cashFlows = new List<decimal> { -financedAmount };
        cashFlows.AddRange(schedule.Select(s => s.TotalPayment));

        var tir = CalculateIRR(cashFlows);
        var van = CalculateNPV(monthlyRate, cashFlows);

        var tcea = (decimal)(Math.Pow((double)(1 + tir), 12) - 1) * 100;

        return new SimulationResult
        {
            Cuota = cuota,
            Installments = schedule,
            Tir = tir,
            Van = van,
            Tcea = tcea
        };
    }
    
    private static decimal CalculateIRR(List<decimal> cashFlows)
    {
        double guess = 0.1;

        for (int i = 0; i < 100; i++)
        {
            double npv = 0;
            double derivative = 0;

            for (int t = 0; t < cashFlows.Count; t++)
            {
                npv += (double)cashFlows[t] / Math.Pow(1 + guess, t);
                derivative -= t * (double)cashFlows[t] / Math.Pow(1 + guess, t + 1);
            }

            var newGuess = guess - npv / derivative;

            if (Math.Abs(newGuess - guess) < 1e-7)
                break;

            guess = newGuess;
        }

        return (decimal)guess;
    }

    private static decimal CalculateNPV(decimal rate, List<decimal> cashFlows)
    {
        decimal npv = 0;

        for (int t = 0; t < cashFlows.Count; t++)
        {
            npv += cashFlows[t] / (decimal)Math.Pow((double)(1 + rate), t);
        }

        return npv;
    }
}