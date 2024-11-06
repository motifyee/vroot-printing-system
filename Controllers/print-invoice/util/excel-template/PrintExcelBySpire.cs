
#pragma warning disable CA1416 // Validate platform compatibility
using Spire.Xls;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using TemplatePrinting.Models;

namespace TemplatePrinting.Controllers;
public partial class PrintInvoiceController {
  public void PrintExcelBySpire(string filePath, string? printerName) {
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

    var workbook = new Workbook();

    _logger.LogInformation("printing using Spire.XLS");
    var timer = new PerfTimer("time to load excel file");
    workbook.LoadFromFile(filePath);

    if (printerName != null)
      workbook.PrintDocument.PrinterSettings.PrinterName = printerName;
    workbook.PrintDocument.PrinterSettings.PrintRange = PrintRange.Selection;
    // workbook.PrintDocument.PrinterSettings.PrintToFile = true;
    // workbook.PrintDocument.PrinterSettings.PrintFileName = "InvoiceTemplate.pdf";

    timer.Print("time to print");
    if (_hostEnv.IsProduction()) workbook.PrintDocument.Print();
    timer.End();
  }
}