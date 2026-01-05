
using PrintingLibrary.ExcelUtils;
using PrintingLibrary.InteropUtils;
using PrintingLibrary.SpireUtils;
using PrintingLibrary.Setup;
using TemplatePrinting.Models.Invoice;
using TemplatePrinting.Models;

namespace TemplatePrinting.Controllers;

public partial class PrintInvoiceController {

  private void PrintInvoiceByExcel(Invoice invoice, bool useSpirePrinter) {

    string templateFile = Path.Combine(
        PrintingSetup.AssemblyPath,
        "printer",
        "templates",
        "excel",
        $"{invoice!.TemplateName ?? ""}.xlsx"
    );

    string outputFile = GetOutputFilePath(invoice.Date, invoice.InvoiceNo, invoice.TemplateName);

    // Get encryption password if encryption is enabled
    string? encryptionPassword = util.EncryptionPassword;

    ExcelUtils.CreateOutputExcel(outputFile, templateFile, invoice, encryptionPassword);

    var (asset, info) = GetPrintStampAssetAndInfo(invoice.PrinterName);
    if (info == null) return;

    ExcelUtils.AddPrintStamp(
      filePath: outputFile,
      imageBytes: resources.GetBytes(asset),
      width: info.Width,
      height: info.Height,
      encryptionPassword: encryptionPassword
    );

    if (!hostEnvironment.IsProduction()) return;

    if (useSpirePrinter)
      SpireUtils.PrintExcelFile(outputFile, invoice.PrinterName);
    else InteropUtils.PrintExcelFile(outputFile, invoice.PrinterName);
  }
}