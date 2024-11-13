#pragma warning disable CA1416 // Validate platform compatibility

using Spire.Pdf;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using TemplatePrinting.Models.Invoice;

namespace TemplatePrinting.Controllers;
public partial class PrintInvoiceController {

  private void PrintPdfData(Invoice invoice, byte[] data) {
    var pdf = new PdfDocument();
    pdf.LoadFromBytes(data);
    pdf.PrintSettings.PrinterName = "Microsoft Print to PDF";// invoice.PrinterName;

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
      pdf.PrintSettings.PrintController = new StandardPrintController();

      var paperSize = new PaperSize("Custom", 315, 10000) {
        RawKind = (int)PaperKind.Custom
      };// new PaperSize(3.14961M, 10);
      pdf.PrintSettings.PaperSize = paperSize;

      pdf.PrintSettings.SelectPageRange(1, 1);
      pdf.PrintSettings.SelectSinglePageLayout(Spire.Pdf.Print.PdfSinglePageScalingMode.FitSize, true);
    }


    pdf.Print();

    var outputFilePath = GetOutputFilePath(invoice.Date, invoice.InvoiceNo, invoice.TemplateName, "pdf");
    pdf.SaveToFile(outputFilePath, FileFormat.PDF);
  }
}