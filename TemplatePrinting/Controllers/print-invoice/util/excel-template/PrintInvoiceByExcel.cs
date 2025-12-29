
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

    ExcelUtils.CreateOutputExcel(outputFile, templateFile, invoice);

    ExcelUtils.AddPrintStamp(outputFile, _resources.GetBytes(Assets.PrintStamp));

    if (!_hostEnv.IsProduction()) return;

    if (useSpirePrinter)
      SpireUtils.PrintExcelFile(outputFile, invoice.PrinterName);
    else InteropUtils.PrintExcelFile(outputFile, invoice.PrinterName);
  }
}