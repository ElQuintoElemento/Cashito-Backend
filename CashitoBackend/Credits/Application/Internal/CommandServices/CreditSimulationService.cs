using CashitoBackend.Credits.Application.Internal.DTOs;
using CashitoBackend.Credits.Domain.Model.Commands;
using CashitoBackend.Credits.Domain.Model.Entities;
using CashitoBackend.Credits.Domain.Model.ValueObjects;
using CashitoBackend.Credits.Domain.Services;

namespace CashitoBackend.Credits.Application.Internal.CommandServices;

public class CreditSimulationService : ICreditSimulationService
{
    public SimulationResult Simulate(SimulateCreditCommand command)
    {
        var financedAmount = command.VehiclePrice - command.DownPayment;

        // =========================
        // TASA MENSUAL
        // =========================

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

        int totalPeriods = command.TermMonths;

        decimal balance = financedAmount;

        // =========================
        // APLICAR GRACIA
        // =========================

        for (int i = 1; i <= command.GracePeriod; i++)
        {
            var interest = balance * monthlyRate;

            if (command.GraceType == GraceType.Total)
            {
                // Capitaliza interés
                balance += interest;
            }
        }

        // =========================
        // CUOTA FRANCESA
        // =========================

        int paymentPeriods = totalPeriods - command.GracePeriod;

        decimal cuotaBase = 0;

        if (paymentPeriods > 0)
        {
            cuotaBase = balance *
                        (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), paymentPeriods)) /
                        ((decimal)Math.Pow((double)(1 + monthlyRate), paymentPeriods) - 1);
        }

        var schedule = new List<Installment>();

        balance = financedAmount;

        DateTime date = DateTime.UtcNow;

        // =========================
        // CRONOGRAMA
        // =========================

        for (int i = 1; i <= totalPeriods; i++)
        {
            decimal interest = balance * monthlyRate;
            decimal amortization = 0;
            decimal totalPayment = 0;

            // PERIODO DE GRACIA
            if (i <= command.GracePeriod)
            {
                if (command.GraceType == GraceType.Total)
                {
                    totalPayment = 0;

                    // capitalización
                    balance += interest;
                }
                else if (command.GraceType == GraceType.Partial)
                {
                    totalPayment = interest;

                    // no amortiza
                }
            }
            else
            {
                amortization = cuotaBase - interest;

                totalPayment = cuotaBase;

                balance -= amortization;
            }

            // 🔥 SEGURO
            totalPayment += command.Insurance;

            schedule.Add(new Installment(
                i,
                date.AddMonths(i),
                decimal.Round(totalPayment, 2),
                decimal.Round(interest, 2),
                decimal.Round(amortization, 2),
                decimal.Round(balance < 0 ? 0 : balance, 2)
            ));
        }

        // =========================
        // FLUJOS DE CAJA
        // =========================

        var cashFlows = new List<decimal>
        {
            -financedAmount
        };

        cashFlows.AddRange(schedule.Select(s => s.TotalPayment));

        var tir = CalculateIRR(cashFlows);

        var van = CalculateNPV(monthlyRate, cashFlows);

        var tcea = (decimal)(Math.Pow(1 + (double)tir, 12) - 1) * 100;

        return new SimulationResult
        {
            Cuota = decimal.Round(cuotaBase + command.Insurance, 2),
            Installments = schedule,
            Tir = decimal.Round(tir * 100, 6),
            Van = decimal.Round(van, 2),
            Tcea = decimal.Round(tcea, 4)
        };
    }

    // =========================
    // TIR
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

                derivative -= t *
                              (double)cashFlows[t] /
                              Math.Pow(1 + guess, t + 1);
            }

            var newGuess = guess - npv / derivative;

            if (Math.Abs(newGuess - guess) < 1e-7)
                break;

            guess = newGuess;
        }

        return (decimal)guess;
    }

    // =========================
    // VAN
    // =========================

    private static decimal CalculateNPV(decimal rate, List<decimal> cashFlows)
    {
        decimal npv = 0;

        for (int t = 0; t < cashFlows.Count; t++)
        {
            npv += cashFlows[t] /
                   (decimal)Math.Pow((double)(1 + rate), t);
        }

        return npv;
    }
}