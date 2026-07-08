using CashitoBackend.Credits.Application.Internal.DTOs;
using CashitoBackend.Credits.Domain.Model.Commands;
using CashitoBackend.Credits.Domain.Model.Entities;
using CashitoBackend.Credits.Domain.Model.ValueObjects;
using CashitoBackend.Credits.Domain.Services;
using CashitoBackend.Shared.Domain.Exceptions;

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
            int m = GetCompoundingFrequency(command.Capitalization);
            double tna = (double)(command.InterestRate / 100m);
            double monthlyRateDouble = Math.Pow(1 + tna / m, (double)m / 12.0) - 1;
            monthlyRate = (decimal)monthlyRateDouble;
        }

        if (command.BalloonPaymentPercentage < 40m || command.BalloonPaymentPercentage > 50m)
        {
            throw new BadRequestException("Balloon payment percentage must be between 40% and 50%.");
        }

        var balloonPaymentAmount = command.VehiclePrice * (command.BalloonPaymentPercentage / 100m);
        var capitalAmortizar = financedAmount - balloonPaymentAmount;

        int totalPeriods = command.TermMonths;

        decimal balance = capitalAmortizar;

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
        var indicatorOutflows = new List<decimal>();

        balance = capitalAmortizar;

        // Apply grace capitalization to schedule balance starting point
        for (int i = 1; i <= command.GracePeriod; i++)
        {
            if (command.GraceType == GraceType.Total)
            {
                balance += balance * monthlyRate;
            }
        }

        DateTime date = DateTime.UtcNow;

        // =========================
        // CRONOGRAMA
        // =========================

        for (int i = 1; i <= totalPeriods; i++)
        {
            decimal interest = balance * monthlyRate;
            decimal amortization = 0;
            decimal totalPayment = 0;
            decimal beginningBalance = balance;

            decimal desgravamen = beginningBalance * (command.DesgravamenInsuranceRate / 100m);
            decimal vehicular = command.VehiclePrice * (command.VehicularInsuranceRate / 100m);
            decimal portes = command.Portes;
            decimal otherExpenses = command.OtherExpenses;

            // PERIODO DE GRACIA
            if (i <= command.GracePeriod)
            {
                if (command.GraceType == GraceType.Total)
                {
                    totalPayment = 0;

                    // capitalización
                    balance += interest;

                    desgravamen = 0;
                    vehicular = 0;
                    portes = 0;
                    otherExpenses = 0;
                }
                else if (command.GraceType == GraceType.Partial)
                {
                    totalPayment = interest + desgravamen + vehicular + portes + otherExpenses;

                    // no amortiza
                }
            }
            else
            {
                if (i == totalPeriods)
                {
                    // Rounding residual adjustment
                    amortization = beginningBalance;
                }
                else
                {
                    amortization = cuotaBase - interest;
                }

                totalPayment = (i == totalPeriods ? (amortization + interest) : cuotaBase) + desgravamen + vehicular + portes + otherExpenses;

                balance -= amortization;
            }

            bool isBalloon = false;
            decimal balloonAmount = 0;

            // VAN/TIR/TCEA: outflows exclude balloon (captured before CB is added)
            indicatorOutflows.Add(decimal.Round(-totalPayment, 2));

            if (i == totalPeriods)
            {
                isBalloon = true;
                balloonAmount = balloonPaymentAmount;
                totalPayment += balloonAmount;
            }

            var inst = new Installment(
                i,
                date.AddMonths(i),
                decimal.Round(totalPayment, 2),
                decimal.Round(interest, 2),
                decimal.Round(amortization, 2),
                decimal.Round(balance < 0 ? 0 : balance, 2)
            )
            {
                BaseInstallment = decimal.Round(i <= command.GracePeriod ? (command.GraceType == GraceType.Total ? 0 : interest) : (i == totalPeriods ? (amortization + interest) : cuotaBase), 2),
                BeginningBalance = decimal.Round(beginningBalance, 2),
                DesgravamenInsurance = decimal.Round(desgravamen, 2),
                VehicularInsurance = decimal.Round(vehicular, 2),
                Portes = decimal.Round(portes, 2),
                OtherExpenses = decimal.Round(otherExpenses, 2),
                CashFlow = decimal.Round(-totalPayment, 2),
                IsBalloon = isBalloon,
                BalloonAmount = decimal.Round(balloonAmount, 2)
            };
            schedule.Add(inst);
        }

        // =========================
        // FLUJOS DE CAJA
        // =========================

        // VAN/TIR/TCEA: FC₀ = capitalAmortizar − upfront fees; outflows exclude balloon
        var netDisbursedForIndicators = capitalAmortizar - command.DisbursementFee - command.EvaluationFee - command.NotaryExpenses - command.SoatAmount;

        var cashFlows = new List<decimal>
        {
            netDisbursedForIndicators
        };

        cashFlows.AddRange(indicatorOutflows);

        var tir = CalculateIRR(cashFlows);

        var van = CalculateNPV(monthlyRate, cashFlows);

        var tcea = (decimal)(Math.Pow(1 + (double)tir, 12) - 1) * 100;

        return new SimulationResult
        {
            Cuota = schedule.Count > command.GracePeriod ? schedule[command.GracePeriod].TotalPayment : (schedule.Count > 0 ? schedule[0].TotalPayment : 0),
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
        if (cashFlows.Count < 2)
            return 0;

        var newtonResult = TryNewtonIrr(cashFlows, 0.001);
        if (newtonResult.HasValue && Math.Abs(CalculateNpvDouble(newtonResult.Value, cashFlows)) < 1e-4)
            return (decimal)newtonResult.Value;

        var bisectionResult = TryBisectionIrr(cashFlows);
        if (bisectionResult.HasValue)
            return (decimal)bisectionResult.Value;

        return newtonResult.HasValue ? (decimal)newtonResult.Value : 0;
    }

    private static double? TryNewtonIrr(List<decimal> cashFlows, double guess)
    {
        for (int i = 0; i < 100; i++)
        {
            var (npv, derivative) = EvaluateNpvAndDerivative(cashFlows, guess);

            if (Math.Abs(derivative) < 1e-12)
                break;

            var newGuess = guess - npv / derivative;

            if (newGuess <= -0.999999)
                newGuess = (guess + 0.1) / 2.0;

            if (Math.Abs(newGuess - guess) < 1e-10)
                return newGuess;

            guess = newGuess;
        }

        return Math.Abs(CalculateNpvDouble(guess, cashFlows)) < 1e-4 ? guess : null;
    }

    private static double? TryBisectionIrr(List<decimal> cashFlows)
    {
        double lo = 0;
        double hi = 1;
        double npvLo = CalculateNpvDouble(lo, cashFlows);
        double npvHi = CalculateNpvDouble(hi, cashFlows);

        if (Math.Sign(npvLo) == Math.Sign(npvHi))
        {
            hi = 0.05;
            npvHi = CalculateNpvDouble(hi, cashFlows);
            if (Math.Sign(npvLo) == Math.Sign(npvHi))
                return null;
        }

        for (int i = 0; i < 200; i++)
        {
            double mid = (lo + hi) / 2;
            double npvMid = CalculateNpvDouble(mid, cashFlows);

            if (Math.Abs(npvMid) < 1e-10 || hi - lo < 1e-12)
                return mid;

            if (Math.Sign(npvMid) == Math.Sign(npvLo))
                lo = mid;
            else
                hi = mid;
        }

        return (lo + hi) / 2;
    }

    private static (double Npv, double Derivative) EvaluateNpvAndDerivative(List<decimal> cashFlows, double rate)
    {
        double npv = 0;
        double derivative = 0;

        for (int t = 0; t < cashFlows.Count; t++)
        {
            npv += (double)cashFlows[t] / Math.Pow(1 + rate, t);
            derivative -= t * (double)cashFlows[t] / Math.Pow(1 + rate, t + 1);
        }

        return (npv, derivative);
    }

    private static double CalculateNpvDouble(double rate, List<decimal> cashFlows)
    {
        double npv = 0;

        for (int t = 0; t < cashFlows.Count; t++)
            npv += (double)cashFlows[t] / Math.Pow(1 + rate, t);

        return npv;
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

    private static int GetCompoundingFrequency(string? capitalization)
    {
        if (string.IsNullOrWhiteSpace(capitalization))
            throw new BadRequestException("Capitalization frequency is required for TNA rate type.");

        return capitalization.Trim().ToLowerInvariant() switch
        {
            "daily" or "diaria" or "diario" => 360,
            "monthly" or "mensual" => 12,
            "bimonthly" or "bimestral" => 6,
            "quarterly" or "trimestral" => 4,
            "semi-annual" or "semestral" or "semianual" => 2,
            "annual" or "anual" => 1,
            _ => throw new BadRequestException($"Invalid capitalization frequency: {capitalization}")
        };
    }
}