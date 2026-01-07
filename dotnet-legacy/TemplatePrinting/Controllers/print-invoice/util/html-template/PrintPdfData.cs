#pragma warning disable CA1416 // Validate platform compatibility

using PrintingLibrary.SpireUtils;
using TemplatePrinting.Models.Invoice;

namespace TemplatePrinting.Controllers;

public partial class PrintInvoiceController
{

  private void PrintPdfData(Invoice invoice, byte[] data)
  {
    string printerName = "Microsoft Print to PDF"; // invoice.PrinterName;
    string? password = util.EncryptionPassword;
    SpireUtil.PrintPdf(data, password: password, printerName: printerName);

    var outputFilePath = GetOutputFilePath(invoice.Date, invoice.InvoiceNo, invoice.TemplateName, "pdf");
    SpireUtil.SavePdf(data, outputFilePath, password: password);
  }
}