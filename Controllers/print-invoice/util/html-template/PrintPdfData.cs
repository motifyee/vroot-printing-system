
using Spire.Pdf;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using TemplatePrinting.Models.Invoice;

namespace TemplatePrinting.Controllers;
public partial class PrintInvoiceController {

  private void PrintPdfData(Invoice invoice, byte[] data) {
    var pdf = new PdfDocument();
    pdf.LoadFromBytes(data);
    pdf.PrintSettings.PrinterName = invoice.PrinterName;

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
#pragma warning disable CA1416 // Validate platform compatibility
      pdf.PrintSettings.PaperSize = new PaperSize("Custom", 315, 10000);// new PaperSize(3.14961M, 10);

    if (_hostEnv.IsProduction()) pdf.Print();

    var outputFilePath = GetOutputFilePath(invoice.Date, invoice.InvoiceNo, invoice.InvoiceType, "pdf");
    pdf.SaveToFile(outputFilePath, FileFormat.PDF);
  }
}