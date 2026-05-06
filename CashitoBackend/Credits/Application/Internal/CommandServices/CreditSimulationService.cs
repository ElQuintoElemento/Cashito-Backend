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

        // 🔥 1. TIPO DE TASA (TEA / TNA)
        decimal monthlyRate;

        if (command.RateType == "TEA")
        {
            monthlyRate = (decimal)Math.Pow(
                1 + (double)(command.InterestRate / 100m),
                1.0 / 12.0
            ) - 1;
        }
        else // TNA
        {
            monthlyRate = command.InterestRate / 100m / 12m;
        }

        var n = command.TermMonths;

        // 🔥 2. CUOTA BASE (sin seguro)
        var cuotaBase = financedAmount *
                        (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), n)) /
                        ((decimal)Math.Pow((double)(1 + monthlyRate), n) - 1);

        var schedule = new List<Installment>();

        decimal balance = financedAmount;
        DateTime date = DateTime.UtcNow;

        for (int i = 1; i <= n; i++)
        {
            decimal interest = balance * monthlyRate;
            decimal amortization = 0;
            decimal totalPayment = 0;

            // 🔥 3. PERIODO DE GRACIA (TOTAL)
            if (i <= command.GracePeriod)
            {
                amortization = 0;
                totalPayment = 0;

                // interés se capitaliza
                balance += interest;
            }
            else
            {
                amortization = cuotaBase - interest;
                totalPayment = cuotaBase;

                balance -= amortization;
            }

            // 🔥 4. SEGURO
            totalPayment += command.Insurance;

            schedule.Add(new Installment(
                i,
                date.AddMonths(i),
                totalPayment,
                interest,
                amortization,
                balance < 0 ? 0 : balance
            ));
        }

        // 🔥 5. FLUJOS DE CAJA (DEUDOR)
        var cashFlows = new List<decimal> { -financedAmount };
        cashFlows.AddRange(schedule.Select(s => s.TotalPayment));

        var tir = CalculateIRR(cashFlows);
        var van = CalculateNPV(monthlyRate, cashFlows);

        // 🔥 6. TCEA
        var tcea = (decimal)(Math.Pow(1 + (double)tir, 12) - 1) * 100;

        return new SimulationResult
        {
            Cuota = cuotaBase + command.Insurance,
            Installments = schedule,
            Tir = tir,
            Van = van,
            Tcea = tcea
        };
    }

    // =========================
    // IRR (TIR)
    // =========================
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

    // =========================
    // NPV (VAN)
    // =========================
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