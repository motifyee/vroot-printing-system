
#pragma warning disable CA1416 // Validate platform compatibility
using Spire.Xls;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using TemplatePrinting.Models;

namespace TemplatePrinting.Controllers;
public partial class PrintInvoiceController {
  private void PrintExcelBySpire(string filePath, string? printerName) {
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

    var workbook = new Workbook();

    _logger.LogInformation("printing using Spire.XLS");
    var timer = new PerfTimer("time to load excel file");
    workbook.LoadFromFile(filePath);

    // var sheet = workbook.Worksheets[0];

    if (printerName != null)
      workbook.PrintDocument.PrinterSettings.PrinterName = printerName; // "Microsoft Print to PDF";

    // workbook.PrintDocument.PrinterSettings.FromPage = 1;
    // workbook.PrintDocument.PrinterSettings.ToPage = 1;
    workbook.PrintDocument.PrinterSettings.PrintRange = PrintRange.Selection;
    // workbook.PrintDocument.PrinterSettings.DefaultPageSettings.PaperSource.RawKind = (int)PaperSourceKind.Custom;

    // var x = workbook.PrintDocument.PrinterSettings.DefaultPageSettings.PaperSource.SourceName;
    // sheet.PageSetup.PaperSize = PaperSizeType.Custom; // new PaperSize(3.14961M, 10);
    // sheet.PageSetup.SetCustomPaperSize(3150, 100);
    // sheet.PageSetup.IsFitToPage = true;
    // sheet.PageSetup.FitToPagesWide = 1;

    // workbook.PrintDocument.PrintController = new StandardPrintController();
    // var paperSize = new PaperSize("Custom", 3150, 100) { RawKind = (int)PaperKind.Custom };
    // workbook.PrintDocument.DefaultPageSettings.PaperSize = paperSize;

    // workbook.PrintDocument.PrinterSettings.PrintToFile = true;
    // workbook.PrintDocument.PrinterSettings.PrintFileName = "InvoiceTemplate.pdf";

    timer.Print("time to print");

    workbook.PrintDocument.Print();
    timer.End();
  }
}