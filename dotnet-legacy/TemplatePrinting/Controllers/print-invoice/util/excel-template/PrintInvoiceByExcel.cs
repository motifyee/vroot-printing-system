
using PrintingLibrary.ExcelUtils;
using PrintingLibrary.EncryptUtils;
using PrintingLibrary.InteropUtils;
using PrintingLibrary.SpireUtils;
using PrintingLibrary.Setup;
using TemplatePrinting.Models.Invoice;

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

    string outPath = GetOutputFilePath(invoice.Date, invoice.InvoiceNo, invoice.TemplateName);

    var outBytes = ExcelUtils.CreateOutputExcel(templateFile, invoice);

    var (asset, info) = GetPrintStampAssetAndInfo(invoice.PrinterName);
    if (info != null) {
      ExcelUtils.AddPrintStamp(
        fileStream: outBytes,
        imageBytes: resources.GetBytes(asset),
        width: info.Width,
        height: info.Height
      );
    }

    // Get encryption password if encryption is enabled
    string? encryptionPassword = util.EncryptionPassword;

    outBytes.Seek(0, SeekOrigin.Begin);
    if (!string.IsNullOrEmpty(encryptionPassword)) {
      EncryptUtil.EncryptStreamToFile(outBytes, outPath, encryptionPassword);
    } else {
      using var fs = new FileStream(outPath, FileMode.Create, FileAccess.Write);
      outBytes.CopyTo(fs);
    }

    if (!hostEnvironment.IsProduction()) return;

    if (useSpirePrinter)
      SpireUtils.PrintExcelFile(outBytes, invoice.PrinterName, encryptionPassword);
    else InteropUtils.PrintExcelFile(outBytes, invoice.PrinterName);
  }
}