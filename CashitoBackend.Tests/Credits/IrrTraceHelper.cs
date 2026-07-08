using System.Globalization;
using System.Text;

namespace CashitoBackend.Tests.Credits;

internal static class IrrTraceHelper
{
    internal sealed record IrrTraceResult(
        double ConvergedRate,
        IReadOnlyList<IrrIteration> Iterations,
        double NpvAtConvergedRate,
        double? BisectionRate);

    internal sealed record IrrIteration(
        int Index,
        double Guess,
        double Npv,
        double Derivative,
        double NewGuess,
        double Delta);

    internal static IrrTraceResult TraceNewton(List<decimal> cashFlows, double initialGuess = 0.001)
    {
        var iterations = new List<IrrIteration>();
        double guess = initialGuess;

        for (var i = 0; i < 100; i++)
        {
            var (npv, derivative) = Evaluate(cashFlows, guess);
            var newGuess = derivative == 0 ? guess : guess - npv / derivative;

            if (newGuess <= -0.999999)
                newGuess = (guess + 0.1) / 2.0;

            iterations.Add(new IrrIteration(i, guess, npv, derivative, newGuess, Math.Abs(newGuess - guess)));

            if (Math.Abs(newGuess - guess) < 1e-10)
            {
                guess = newGuess;
                break;
            }

            guess = newGuess;
        }

        var bisection = Bisection(cashFlows);
        return new IrrTraceResult(guess, iterations, CalculateNpv(guess, cashFlows), bisection);
    }

    internal static double? Bisection(List<decimal> cashFlows)
    {
        double lo = 0;
        double hi = 0.05;
        var npvLo = CalculateNpv(lo, cashFlows);
        var npvHi = CalculateNpv(hi, cashFlows);

        if (Math.Sign(npvLo) == Math.Sign(npvHi))
            return null;

        for (var i = 0; i < 200; i++)
        {
            var mid = (lo + hi) / 2;
            var npvMid = CalculateNpv(mid, cashFlows);

            if (Math.Abs(npvMid) < 1e-10 || hi - lo < 1e-12)
                return mid;

            if (Math.Sign(npvMid) == Math.Sign(npvLo))
                lo = mid;
            else
                hi = mid;
        }

        return (lo + hi) / 2;
    }

    internal static string FormatCashFlows(List<decimal> cashFlows)
    {
        var sb = new StringBuilder();
        for (var t = 0; t < cashFlows.Count; t++)
            sb.Append(CultureInfo.InvariantCulture, $"t={t}: {cashFlows[t]:F2}\n");
        return sb.ToString();
    }

    internal static string FormatTrace(IrrTraceResult trace)
    {
        var sb = new StringBuilder();
        foreach (var row in trace.Iterations)
        {
            sb.Append(CultureInfo.InvariantCulture,
                $"i={row.Index} guess={row.Guess:F8} npv={row.Npv:F4} deriv={row.Derivative:F4} newGuess={row.NewGuess:F8} delta={row.Delta:E3}\n");
        }

        sb.Append(CultureInfo.InvariantCulture, $"NPV at converged rate: {trace.NpvAtConvergedRate:F8}\n");
        sb.Append(CultureInfo.InvariantCulture, $"Bisection rate: {(trace.BisectionRate?.ToString("F8", CultureInfo.InvariantCulture) ?? "n/a")}\n");
        return sb.ToString();
    }

    internal static bool WithinRelativeTolerance(decimal actual, decimal expected, decimal relativeTolerance = 0.01m)
        => expected == 0
            ? Math.Abs(actual) <= relativeTolerance
            : Math.Abs((actual - expected) / expected) <= relativeTolerance;

    internal static List<decimal> BuildIndicatorCashFlows(
        decimal capitalAmortizar,
        IEnumerable<(decimal TotalPayment, decimal BalloonAmount)> installments,
        decimal disbursementFee = 0,
        decimal evaluationFee = 0,
        decimal notaryExpenses = 0,
        decimal soatAmount = 0)
    {
        var fc0 = capitalAmortizar - disbursementFee - evaluationFee - notaryExpenses - soatAmount;
        var flows = new List<decimal> { fc0 };
        foreach (var (totalPayment, balloonAmount) in installments)
            flows.Add(-(totalPayment - balloonAmount));
        return flows;
    }

    private static (double Npv, double Derivative) Evaluate(List<decimal> cashFlows, double rate)
    {
        double npv = 0;
        double derivative = 0;

        for (var t = 0; t < cashFlows.Count; t++)
        {
            npv += (double)cashFlows[t] / Math.Pow(1 + rate, t);
            derivative -= t * (double)cashFlows[t] / Math.Pow(1 + rate, t + 1);
        }

        return (npv, derivative);
    }

    private static double CalculateNpv(double rate, List<decimal> cashFlows)
    {
        double npv = 0;
        for (var t = 0; t < cashFlows.Count; t++)
            npv += (double)cashFlows[t] / Math.Pow(1 + rate, t);
        return npv;
    }
}
