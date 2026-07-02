using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CashitoBackend.Clients.Domain.Repositories;
using CashitoBackend.Credits.Domain.Model.Aggregates;
using CashitoBackend.Credits.Domain.Repositories;
using CashitoBackend.Credits.Domain.Services;
using CashitoBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using CashitoBackend.Vehicles.Domain.Repositories;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CashitoBackend.Credits.Application.Internal.QueryServices;

public class CreditExportService : ICreditExportService
{
    private readonly ICreditRepository _creditRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly AppDbContext _context;

    public CreditExportService(
        ICreditRepository creditRepository,
        IClientRepository clientRepository,
        IVehicleRepository vehicleRepository,
        AppDbContext context)
    {
        _creditRepository = creditRepository;
        _clientRepository = clientRepository;
        _vehicleRepository = vehicleRepository;
        _context = context;
    }

    public async Task<byte[]> GeneratePdfAsync(int creditId, int userId)
    {
        var credit = await _creditRepository.FindByIdWithScheduleAsync(creditId);
        if (credit == null || credit.UserId != userId)
            throw new UnauthorizedAccessException("You do not have access to this credit");

        return await BuildPdfBytesAsync(credit);
    }

    public async Task<byte[]> GeneratePdfPublicAsync(int creditId, string token)
    {
        var credit = await _creditRepository.FindByIdWithScheduleAsync(creditId);
        if (credit == null || !string.Equals(credit.PublicToken, token, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Invalid credit access token");

        return await BuildPdfBytesAsync(credit);
    }

    public async Task<byte[]> GenerateExcelAsync(int creditId, int userId)
    {
        var credit = await _creditRepository.FindByIdWithScheduleAsync(creditId);
        if (credit == null || credit.UserId != userId)
            throw new UnauthorizedAccessException("You do not have access to this credit");

        return BuildExcelBytes(credit);
    }

    public async Task<byte[]> GenerateExcelPublicAsync(int creditId, string token)
    {
        var credit = await _creditRepository.FindByIdWithScheduleAsync(creditId);
        if (credit == null || !string.Equals(credit.PublicToken, token, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Invalid credit access token");

        return BuildExcelBytes(credit);
    }

    // ==========================================
    // PDF BUILDER
    // ==========================================
    private async Task<byte[]> BuildPdfBytesAsync(Credit credit)
    {
        var clientName = "Unknown Client";
        var clientDni = "N/A";
        var vehicleName = "Unknown Vehicle";

        var client = await _clientRepository.FindByIdAsync(credit.ClientId);
        if (client != null)
        {
            clientName = $"{client.FirstName} {client.LastName}";
            clientDni = client.Dni;
        }

        var vehicle = await _vehicleRepository.FindByIdAsync(credit.VehicleId);
        if (vehicle != null)
        {
            vehicleName = $"{vehicle.Brand} {vehicle.Model}";
        }

        var issueDate = GetCreditIssueDate(credit);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                // --- HEADER ---
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("CASHITO").FontSize(24).Bold().FontColor("#1e3a8a");
                        col.Item().Text("Financial Credit Report").FontSize(12).Italic().FontColor(Colors.Grey.Darken2);
                    });
                    row.ConstantItem(120).AlignRight().AlignMiddle().Text($"Credit #{credit.Id}").FontSize(14).Bold().FontColor(Colors.Grey.Darken3);
                });

                // --- CONTENT ---
                page.Content().Column(col =>
                {
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().Height(12);

                    // Overview Summary Grid
                    col.Item().Grid(grid =>
                    {
                        grid.Columns(2);
                        grid.Spacing(12);

                        // Left: Client & Vehicle Information
                        grid.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                        {
                            c.Item().Text("General Information").Bold().FontSize(11).FontColor("#1e3a8a");
                            c.Item().PaddingTop(4);

                            c.Item().Text(t => { t.Span("Client: ").Bold(); t.Span(clientName); });
                            c.Item().Text(t => { t.Span("DNI: ").Bold(); t.Span(clientDni); });
                            c.Item().Text(t => { t.Span("Vehicle: ").Bold(); t.Span(vehicleName); });
                            c.Item().Text(t => { t.Span("Vehicle Price: ").Bold(); t.Span(FormatCurrency(credit.VehiclePrice, credit.Currency.ToString())); });
                            c.Item().Text(t => { t.Span("Down Payment: ").Bold(); t.Span(FormatCurrency(credit.DownPayment, credit.Currency.ToString())); });
                            c.Item().Text(t => { t.Span("Financed Amount: ").Bold(); t.Span(FormatCurrency(credit.FinancedAmount, credit.Currency.ToString())); });
                        });

                        // Right: Credit Terms & Settings
                        grid.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                        {
                            c.Item().Text("Credit Terms").Bold().FontSize(11).FontColor("#1e3a8a");
                            c.Item().PaddingTop(4);

                            c.Item().Text(t => { t.Span("Interest Rate: ").Bold(); t.Span($"{credit.InterestRate:F4}%"); });
                            c.Item().Text(t => { t.Span("Rate Type: ").Bold(); t.Span(credit.RateType); });
                            c.Item().Text(t => { t.Span("Term: ").Bold(); t.Span($"{credit.TermMonths} Months"); });
                            c.Item().Text(t => { t.Span("Grace Period: ").Bold(); t.Span($"{credit.GracePeriod} Months ({credit.GraceType})"); });
                            c.Item().Text(t => { t.Span("Insurance: ").Bold(); t.Span(FormatCurrency(credit.Insurance, credit.Currency.ToString())); });
                            c.Item().Text(t => { t.Span("Status: ").Bold(); t.Span(credit.Status.ToString()); });
                        });
                    });

                    col.Item().Height(12);

                    // Outcomes Box (TCEA, TIR, VAN)
                    col.Item().Background(Colors.Grey.Lighten4).Padding(12).Grid(grid =>
                    {
                        grid.Columns(3);
                        grid.Spacing(10);

                        grid.Item().AlignCenter().Column(c =>
                        {
                            c.Item().Text("TCEA").Bold().FontSize(11).FontColor("#1e3a8a");
                            c.Item().Text($"{credit.Tcea:F4}%").FontSize(14).Bold();
                        });

                        grid.Item().AlignCenter().Column(c =>
                        {
                            c.Item().Text("TIR").Bold().FontSize(11).FontColor("#1e3a8a");
                            c.Item().Text($"{credit.Tir:F6}%").FontSize(14).Bold();
                        });

                        grid.Item().AlignCenter().Column(c =>
                        {
                            c.Item().Text("VAN").Bold().FontSize(11).FontColor("#1e3a8a");
                            c.Item().Text(FormatCurrency(credit.Van, credit.Currency.ToString())).FontSize(14).Bold();
                        });
                    });

                    col.Item().Height(12);

                    // Date & Token Details
                    col.Item().Column(c =>
                    {
                        c.Item().Text(t => { t.Span("Issue Date: ").Bold(); t.Span(issueDate.ToString("dd/MM/yyyy")); });
                        if (!string.IsNullOrEmpty(credit.PublicToken))
                        {
                            c.Item().Text(t => { t.Span("Public Token: ").Bold(); t.Span(credit.PublicToken).FontSize(9).Italic(); });
                        }
                    });

                    // Force Page Break for Amortization Schedule
                    col.Item().PageBreak();

                    // --- PAGE 2 ---
                    col.Item().Text("Amortization Schedule").FontSize(18).Bold().FontColor("#1e3a8a");
                    col.Item().PaddingTop(6);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40); // Installment
                            columns.RelativeColumn();   // Date
                            columns.RelativeColumn();   // Payment
                            columns.RelativeColumn();   // Interest
                            columns.RelativeColumn();   // Amortization
                            columns.RelativeColumn();   // Balance
                            columns.ConstantColumn(50); // Paid
                            columns.RelativeColumn();   // Paid Date
                        });

                        // Headers
                        table.Header(header =>
                        {
                            header.Cell().Background("#1e3a8a").Padding(5).Text("N°").Bold().FontColor(Colors.White).AlignCenter();
                            header.Cell().Background("#1e3a8a").Padding(5).Text("Date").Bold().FontColor(Colors.White);
                            header.Cell().Background("#1e3a8a").Padding(5).Text("Payment").Bold().FontColor(Colors.White).AlignRight();
                            header.Cell().Background("#1e3a8a").Padding(5).Text("Interest").Bold().FontColor(Colors.White).AlignRight();
                            header.Cell().Background("#1e3a8a").Padding(5).Text("Amortization").Bold().FontColor(Colors.White).AlignRight();
                            header.Cell().Background("#1e3a8a").Padding(5).Text("Balance").Bold().FontColor(Colors.White).AlignRight();
                            header.Cell().Background("#1e3a8a").Padding(5).Text("Paid").Bold().FontColor(Colors.White).AlignCenter();
                            header.Cell().Background("#1e3a8a").Padding(5).Text("Paid Date").Bold().FontColor(Colors.White);
                        });

                        // Rows
                        foreach (var inst in credit.Schedule.OrderBy(i => i.Number))
                        {
                            var bgColor = inst.Number % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;

                            table.Cell().Background(bgColor).Padding(5).Text(inst.Number.ToString()).AlignCenter();
                            table.Cell().Background(bgColor).Padding(5).Text(inst.Date.ToString("dd/MM/yyyy"));
                            table.Cell().Background(bgColor).Padding(5).Text(FormatCurrency(inst.TotalPayment, credit.Currency.ToString())).AlignRight();
                            table.Cell().Background(bgColor).Padding(5).Text(FormatCurrency(inst.Interest, credit.Currency.ToString())).AlignRight();
                            table.Cell().Background(bgColor).Padding(5).Text(FormatCurrency(inst.Amortization, credit.Currency.ToString())).AlignRight();
                            table.Cell().Background(bgColor).Padding(5).Text(FormatCurrency(inst.RemainingBalance, credit.Currency.ToString())).AlignRight();
                            table.Cell().Background(bgColor).Padding(5).Text(inst.IsPaid ? "Yes" : "No").AlignCenter();
                            table.Cell().Background(bgColor).Padding(5).Text(inst.PaidAt?.ToString("dd/MM/yyyy") ?? "-");
                        }
                    });
                });

                // Footer
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        using (var ms = new MemoryStream())
        {
            document.GeneratePdf(ms);
            return ms.ToArray();
        }
    }

    // ==========================================
    // EXCEL BUILDER
    // ==========================================
    private byte[] BuildExcelBytes(Credit credit)
    {
        var clientName = "Unknown Client";
        var clientDni = "N/A";
        var vehicleName = "Unknown Vehicle";

        var client = _clientRepository.FindByIdAsync(credit.ClientId).GetAwaiter().GetResult();
        if (client != null)
        {
            clientName = $"{client.FirstName} {client.LastName}";
            clientDni = client.Dni;
        }

        var vehicle = _vehicleRepository.FindByIdAsync(credit.VehicleId).GetAwaiter().GetResult();
        if (vehicle != null)
        {
            vehicleName = $"{vehicle.Brand} {vehicle.Model}";
        }

        var issueDate = GetCreditIssueDate(credit);
        var currencyStr = credit.Currency.ToString();
        var currencyFormat = GetClosedXmlCurrencyFormat(currencyStr);

        using (var workbook = new XLWorkbook())
        {
            // SHEET 1: General Information
            var ws1 = workbook.Worksheets.Add("General Information");
            ws1.Cell("A1").Value = "Cashito - Credit Report";
            ws1.Cell("A1").Style.Font.Bold = true;
            ws1.Cell("A1").Style.Font.FontSize = 16;
            ws1.Cell("A1").Style.Font.FontColor = XLColor.FromHtml("#1e3a8a");

            var headers1 = new[] { "Parameter", "Value" };
            for (int i = 0; i < headers1.Length; i++)
            {
                var cell = ws1.Cell(3, i + 1);
                cell.Value = headers1[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a8a");
                cell.Style.Font.FontColor = XLColor.White;
            }

            var generalData = new (string Param, object Value, string? Format)[]
            {
                ("Credit ID", credit.Id, null),
                ("Client Name", clientName, null),
                ("Client DNI", clientDni, null),
                ("Vehicle", vehicleName, null),
                ("Currency", currencyStr, null),
                ("Vehicle Price", credit.VehiclePrice, currencyFormat),
                ("Down Payment", credit.DownPayment, currencyFormat),
                ("Financed Amount", credit.FinancedAmount, currencyFormat),
                ("Interest Rate", credit.InterestRate / 100m, "0.0000%"),
                ("Rate Type", credit.RateType, null),
                ("Grace Period (Months)", credit.GracePeriod, null),
                ("Grace Type", credit.GraceType.ToString(), null),
                ("Insurance", credit.Insurance, currencyFormat),
                ("Status", credit.Status.ToString(), null),
                ("TCEA", credit.Tcea / 100m, "0.0000%"),
                ("TIR", credit.Tir / 100m, "0.000000%"),
                ("VAN", credit.Van, currencyFormat),
                ("Issue Date", issueDate, "yyyy-mm-dd"),
                ("Public Token", credit.PublicToken, null)
            };

            for (int i = 0; i < generalData.Length; i++)
            {
                int rowIdx = i + 4;
                ws1.Cell(rowIdx, 1).Value = generalData[i].Param;
                ws1.Cell(rowIdx, 1).Style.Font.Bold = true;

                var valCell = ws1.Cell(rowIdx, 2);
                if (generalData[i].Value is DateTime dt)
                {
                    valCell.Value = dt;
                }
                else if (generalData[i].Value is decimal dec)
                {
                    valCell.Value = dec;
                }
                else if (generalData[i].Value is int intVal)
                {
                    valCell.Value = intVal;
                }
                else
                {
                    valCell.Value = generalData[i].Value?.ToString();
                }

                if (!string.IsNullOrEmpty(generalData[i].Format))
                {
                    valCell.Style.NumberFormat.Format = generalData[i].Format;
                }
            }

            ws1.Columns().AdjustToContents();

            // SHEET 2: Schedule
            var ws2 = workbook.Worksheets.Add("Schedule");
            var headers2 = new[] { "Installment", "Date", "Payment", "Interest", "Amortization", "Remaining Balance", "Paid", "Paid Date" };

            for (int i = 0; i < headers2.Length; i++)
            {
                var cell = ws2.Cell(1, i + 1);
                cell.Value = headers2[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a8a");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = i is 0 or 6 ? XLAlignmentHorizontalValues.Center : (i is 2 or 3 or 4 or 5 ? XLAlignmentHorizontalValues.Right : XLAlignmentHorizontalValues.Left);
            }

            var scheduleList = credit.Schedule.OrderBy(x => x.Number).ToList();
            for (int i = 0; i < scheduleList.Count; i++)
            {
                int rowIdx = i + 2;
                var inst = scheduleList[i];

                ws2.Cell(rowIdx, 1).Value = inst.Number;
                ws2.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                var dateCell = ws2.Cell(rowIdx, 2);
                dateCell.Value = inst.Date;
                dateCell.Style.NumberFormat.Format = "yyyy-mm-dd";

                var payCell = ws2.Cell(rowIdx, 3);
                payCell.Value = inst.TotalPayment;
                payCell.Style.NumberFormat.Format = currencyFormat;

                var intCell = ws2.Cell(rowIdx, 4);
                intCell.Value = inst.Interest;
                intCell.Style.NumberFormat.Format = currencyFormat;

                var amortCell = ws2.Cell(rowIdx, 5);
                amortCell.Value = inst.Amortization;
                amortCell.Style.NumberFormat.Format = currencyFormat;

                var balCell = ws2.Cell(rowIdx, 6);
                balCell.Value = inst.RemainingBalance;
                balCell.Style.NumberFormat.Format = currencyFormat;

                ws2.Cell(rowIdx, 7).Value = inst.IsPaid ? "Yes" : "No";
                ws2.Cell(rowIdx, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                var paidDateCell = ws2.Cell(rowIdx, 8);
                if (inst.PaidAt.HasValue)
                {
                    paidDateCell.Value = inst.PaidAt.Value;
                    paidDateCell.Style.NumberFormat.Format = "yyyy-mm-dd";
                }
                else
                {
                    paidDateCell.Value = "-";
                }
            }

            ws2.Columns().AdjustToContents();

            using (var ms = new MemoryStream())
            {
                workbook.SaveAs(ms);
                return ms.ToArray();
            }
        }
    }

    // ==========================================
    // UTILITIES
    // ==========================================
    private DateTime GetCreditIssueDate(Credit credit)
    {
        try
        {
            var entry = _context.Entry(credit);
            var prop = entry.Property<DateTime>("CreatedDate");
            if (prop != null && prop.CurrentValue != default)
            {
                return prop.CurrentValue;
            }
        }
        catch
        {
            // Ignored
        }
        return DateTime.UtcNow.AddHours(-5); // Fallback to Peru time (UTC-5)
    }

    private string FormatCurrency(decimal amount, string currency)
    {
        var symbol = currency == "PEN" ? "S/." : "$";
        return $"{symbol} {amount:N2}";
    }

    private string GetClosedXmlCurrencyFormat(string currency)
    {
        return currency == "PEN" ? "\"S/.\" #,##0.00" : "$#,##0.00";
    }
}
