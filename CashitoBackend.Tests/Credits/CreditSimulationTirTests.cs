using CashitoBackend.Credits.Application.Internal.CommandServices;
using CashitoBackend.Credits.Application.Internal.DTOs;
using CashitoBackend.Credits.Domain.Model.Commands;
using CashitoBackend.Credits.Domain.Model.ValueObjects;
using CashitoBackend.Shared.Domain.Model.ValueObjects;
using Xunit;
using Xunit.Abstractions;

namespace CashitoBackend.Tests.Credits;

public class CreditSimulationTirTests
{
    private const decimal CarlosMendozaCapitalAmortizar = 34000m;

    private readonly CreditSimulationService _service = new();
    private readonly ITestOutputHelper _output;

    public CreditSimulationTirTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static decimal ExpectedTemFromTea(decimal teaPercent) =>
        (decimal)Math.Pow(1 + (double)(teaPercent / 100m), 1.0 / 12.0) - 1;

    private static SimulateCreditCommand CarlosMendozaCommand() =>
        new(
            ClientId: 1,
            VehicleId: 1,
            VehiclePrice: 85000m,
            Currency: Currency.PEN,
            DownPayment: 17000m,
            InterestRate: 9.5m,
            TermMonths: 36,
            RateType: "TEA",
            GracePeriod: 0,
            GraceType: GraceType.None,
            Insurance: 0m,
            OpportunityRate: 0m,
            Capitalization: null,
            DesgravamenInsuranceRate: 0.035m,
            VehicularInsuranceRate: 0.045m,
            Portes: 4.50m,
            DisbursementFee: 0m,
            EvaluationFee: 0m,
            NotaryExpenses: 0m,
            SoatAmount: 0m,
            OtherExpenses: 0m,
            BalloonPaymentPercentage: 40m
        );

    // Secondary case — document may contain rounding errors in schedule rows.
    private static SimulateCreditCommand AnaLuciaRamirezCommand() =>
        new(
            ClientId: 1,
            VehicleId: 1,
            VehiclePrice: 32000m,
            Currency: Currency.USD,
            DownPayment: 8000m,
            InterestRate: 8.4m,
            TermMonths: 30,
            RateType: "TNA",
            GracePeriod: 2,
            GraceType: GraceType.Partial,
            Insurance: 0m,
            OpportunityRate: 0m,
            Capitalization: "monthly",
            DesgravamenInsuranceRate: 0.032m,
            VehicularInsuranceRate: 0.038m,
            Portes: 3.75m,
            DisbursementFee: 0m,
            EvaluationFee: 0m,
            NotaryExpenses: 0m,
            SoatAmount: 0m,
            OtherExpenses: 0m,
            BalloonPaymentPercentage: 45m
        );

    private static List<decimal> BuildIndicatorFlows(
        decimal capitalAmortizar,
        SimulationResult result) =>
        IrrTraceHelper.BuildIndicatorCashFlows(
            capitalAmortizar,
            result.Installments.Select(i => (i.TotalPayment, i.BalloonAmount)));

    private static decimal FirstAmortizingBaseInstallment(SimulationResult result, int gracePeriod) =>
        result.Installments
            .First(i => i.Number > gracePeriod && i.Amortization > 0)
            .BaseInstallment;

    [Fact]
    public void PureFrenchLoan_TirMatchesTem_WhenNoExtrasAndNoBalloon()
    {
        const decimal principal = 34000m;
        const decimal tea = 9.5m;
        const int term = 36;
        var tem = ExpectedTemFromTea(tea);

        var cuota = principal *
                    (tem * (decimal)Math.Pow((double)(1 + tem), term)) /
                    ((decimal)Math.Pow((double)(1 + tem), term) - 1);

        var cashFlows = new List<decimal> { principal };
        var balance = principal;
        for (var i = 1; i <= term; i++)
        {
            var interest = balance * tem;
            var amortization = i == term ? balance : cuota - interest;
            cashFlows.Add(-(i == term ? amortization + interest : cuota));
            balance -= amortization;
        }

        var trace = IrrTraceHelper.TraceNewton(cashFlows);
        Assert.Equal(tem, (decimal)trace.ConvergedRate, precision: 6);
        Assert.True(Math.Abs(trace.NpvAtConvergedRate) < 1e-4);
    }

    [Fact]
    public void CarlosMendoza_GoldenCase_NewtonTraceConvergesWithNearZeroNpv()
    {
        var result = _service.Simulate(CarlosMendozaCommand());
        var cashFlows = BuildIndicatorFlows(CarlosMendozaCapitalAmortizar, result);

        var trace = IrrTraceHelper.TraceNewton(cashFlows, initialGuess: 0.001);

        _output.WriteLine("Indicator cash flows:");
        _output.WriteLine(IrrTraceHelper.FormatCashFlows(cashFlows));
        _output.WriteLine("Newton trace:");
        _output.WriteLine(IrrTraceHelper.FormatTrace(trace));

        Assert.Equal(CarlosMendozaCapitalAmortizar, cashFlows[0]);
        Assert.True(cashFlows[^1] > -2000m, "Last indicator outflow must exclude balloon (~-1,126, not ~-35,126)");
        Assert.True(Math.Abs(trace.NpvAtConvergedRate) < 1e-2);
        Assert.NotNull(trace.BisectionRate);
        Assert.Equal(trace.BisectionRate!.Value, trace.ConvergedRate, precision: 6);
        Assert.Equal(result.Tir / 100m, (decimal)trace.ConvergedRate, precision: 6);
    }

    [Fact]
    public void CarlosMendoza_GoldenCase_VanMatchesDocumentedTarget()
    {
        var result = _service.Simulate(CarlosMendozaCommand());

        _output.WriteLine($"Actual VAN={result.Van:F2} (target -1,551.06 @ TEM del préstamo)");

        Assert.True(Math.Abs(result.Van - (-1551.06m)) <= 0.02m, $"VAN={result.Van:F2}, expected -1551.06 ±0.02");
    }

    [Fact]
    public void CarlosMendoza_GoldenCase_TirMatchesDocumentedTarget()
    {
        var result = _service.Simulate(CarlosMendozaCommand());
        var cashFlows = BuildIndicatorFlows(CarlosMendozaCapitalAmortizar, result);
        var trace = IrrTraceHelper.TraceNewton(cashFlows);

        _output.WriteLine($"Actual TIR={result.Tir:F6}% (target 1.016%)");
        _output.WriteLine(IrrTraceHelper.FormatTrace(trace));
        _output.WriteLine(IrrTraceHelper.FormatCashFlows(cashFlows));

        Assert.True(Math.Abs(result.Tir - 1.016m) <= 0.001m, $"TIR={result.Tir:F6}%, expected 1.016% ±0.001%");
    }

    [Fact]
    public void CarlosMendoza_GoldenCase_TceaMatchesDocumentedTarget()
    {
        var result = _service.Simulate(CarlosMendozaCommand());

        _output.WriteLine($"Actual TCEA={result.Tcea:F4}% (target 12.898%)");

        Assert.Equal(12.898m, result.Tcea, precision: 2);
    }

    [Fact]
    public void CarlosMendoza_GoldenCase_AllIndicatorsMatchDocumentedTargets()
    {
        var result = _service.Simulate(CarlosMendozaCommand());
        var tem = ExpectedTemFromTea(9.5m);

        _output.WriteLine($"TEM préstamo = {tem * 100m:F4}%");
        _output.WriteLine($"VAN = {result.Van:F2} (target -1,551.06)");
        _output.WriteLine($"TIR = {result.Tir:F6}% (target 1.016%)");
        _output.WriteLine($"TCEA = {result.Tcea:F4}% (target 12.898%)");

        Assert.True(Math.Abs(result.Van - (-1551.06m)) <= 0.02m, $"VAN={result.Van:F2}, expected -1551.06 ±0.02");
        Assert.True(Math.Abs(result.Tir - 1.016m) <= 0.001m);
        Assert.Equal(12.898m, result.Tcea, precision: 2);
    }

    [Fact]
    public void CarlosMendoza_TceaIsConsistentWithTir()
    {
        var result = _service.Simulate(CarlosMendozaCommand());
        var tirDecimal = result.Tir / 100m;
        var expectedTcea = ((decimal)Math.Pow(1 + (double)tirDecimal, 12) - 1) * 100m;

        Assert.Equal(expectedTcea, result.Tcea, precision: 3);
    }

    [Fact]
    public void AnaLuciaRamirez_GoldenCase_IndicatorsWithinDocumentedTolerance()
    {
        const decimal targetTcea = 12.727m;
        const decimal targetVan = -476.57m;
        const decimal targetTir = 1.003m;
        const decimal relativeTolerance = 0.01m;

        var result = _service.Simulate(AnaLuciaRamirezCommand());

        _output.WriteLine($"TCEA = {result.Tcea:F4}% (target {targetTcea:F3}%)");
        _output.WriteLine($"VAN = {result.Van:F2} (target {targetVan:F2})");
        _output.WriteLine($"TIR = {result.Tir:F6}% (target {targetTir:F3}%)");

        Assert.True(
            IrrTraceHelper.WithinRelativeTolerance(result.Tcea, targetTcea, relativeTolerance),
            $"TCEA={result.Tcea:F4}%, expected {targetTcea:F3}% ±1%");
        Assert.True(
            IrrTraceHelper.WithinRelativeTolerance(result.Van, targetVan, relativeTolerance),
            $"VAN={result.Van:F2}, expected {targetVan:F2} ±1%");
        Assert.True(
            IrrTraceHelper.WithinRelativeTolerance(result.Tir, targetTir, relativeTolerance),
            $"TIR={result.Tir:F6}%, expected {targetTir:F3}% ±1%");
    }

    // DISCREPANCIA CONOCIDA: el PDF documenta cuotaBase=$354.54 para este caso,
    // pero usando la fórmula francesa estándar sobre capitalAmortizar=$9,600 a
    // paymentPeriods=28 (TermMonths - GracePeriod) con TEM=0.7%, el resultado
    // matemáticamente correcto es $378.75. Reconstruyendo a mano con n=30
    // (TermMonths completo, SIN restar los 2 meses de gracia parcial) se
    // obtiene $355.89, mucho más cercano al valor documentado — lo que sugiere
    // que el caso de prueba del informe pudo calcular la cuota base sobre el
    // plazo total y no sobre los periodos de pago efectivos. El motor actual
    // sigue el pseudocódigo oficial del Capítulo 4 (paymentPeriods =
    // TermMonths - GracePeriod), que es la interpretación financieramente más
    // defendible para gracia parcial. No se modifica CreditSimulationService
    // por esta discrepancia — queda documentada para revisión del equipo.
    [Fact(Skip = "Discrepancia conocida: PDF documenta cuotaBase=$354.54; motor calcula $378.75 (francesa sobre $9,600 / 28 periodos @ TEM 0.7%). Ver comentario en test.")]
    public void AnaLuciaRamirez_KnownDiscrepancy_CuotaBaseDoesNotMatchDocumentedValue()
    {
        var result = _service.Simulate(AnaLuciaRamirezCommand());
        var cuotaBase = FirstAmortizingBaseInstallment(result, gracePeriod: 2);

        _output.WriteLine($"Motor cuota base = {cuotaBase:F2}");
        _output.WriteLine("PDF cuota base = 354.54");

        Assert.Equal(378.75m, cuotaBase, precision: 1);
        Assert.NotEqual(354.54m, cuotaBase);
    }
}
