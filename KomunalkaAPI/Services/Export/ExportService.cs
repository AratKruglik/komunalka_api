using System.Text;
using KomunalkaAPI.DTO.Export;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KomunalkaAPI.Services.Export;

public class ExportService : IExportService
{
    public byte[] GenerateCsv(IEnumerable<MeterReadingExportDto> data)
    {
        var sb = new StringBuilder();
        
        // Header
        sb.AppendLine("Address,Utility Type,Meter Name,Serial Number,Reading Date,Value,Previous Value,Consumption,Unit,Tariff,Is Estimated");

        foreach (var item in data)
        {
            sb.AppendLine(string.Join(",",
                EscapeCsv(item.Address),
                EscapeCsv(item.UtilityType),
                EscapeCsv(item.MeterName),
                EscapeCsv(item.SerialNumber),
                item.ReadingDate.ToString("yyyy-MM-dd"),
                item.ReadingValue,
                item.PreviousValue,
                item.Consumption,
                EscapeCsv(item.Unit),
                EscapeCsv(item.TariffName),
                item.IsEstimated ? "Yes" : "No"
            ));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    public byte[] GeneratePdf(IEnumerable<MeterReadingExportDto> data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Text("Meter Readings Export")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Table(table =>
                    {
                        // Definition
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Address
                            columns.RelativeColumn(2); // Utility
                            columns.RelativeColumn(2); // Meter
                            columns.RelativeColumn(2); // Date
                            columns.RelativeColumn(2); // Value
                            columns.RelativeColumn(2); // Consum.
                            columns.RelativeColumn(1); // Unit
                            columns.RelativeColumn(2); // Tariff
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Address");
                            header.Cell().Element(CellStyle).Text("Utility");
                            header.Cell().Element(CellStyle).Text("Meter");
                            header.Cell().Element(CellStyle).Text("Date");
                            header.Cell().Element(CellStyle).Text("Value");
                            header.Cell().Element(CellStyle).Text("Consum.");
                            header.Cell().Element(CellStyle).Text("Unit");
                            header.Cell().Element(CellStyle).Text("Tariff");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            }
                        });

                        // Content
                        foreach (var item in data)
                        {
                            table.Cell().Element(CellStyle).Text(item.Address);
                            table.Cell().Element(CellStyle).Text(item.UtilityType);
                            table.Cell().Element(CellStyle).Text(item.MeterName + (string.IsNullOrEmpty(item.SerialNumber) ? "" : $"\n({item.SerialNumber})"));
                            table.Cell().Element(CellStyle).Text(item.ReadingDate.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(item.ReadingValue.ToString("F2"));
                            table.Cell().Element(CellStyle).Text(item.Consumption?.ToString("F2") ?? "-");
                            table.Cell().Element(CellStyle).Text(item.Unit);
                            table.Cell().Element(CellStyle).Text(item.TariffName ?? "-");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                            }
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
            });
        });

        return document.GeneratePdf();
    }
}
