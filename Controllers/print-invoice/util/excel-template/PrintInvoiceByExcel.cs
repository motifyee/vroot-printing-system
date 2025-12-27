
using TemplatePrinting.Models.Invoice;

namespace TemplatePrinting.Controllers;

public partial class PrintInvoiceController {

  private void PrintInvoiceByExcel(Invoice invoice, bool useSpirePrinter) {

    string templateFile = Path.Combine(
        AssemblyPath,
        "printer",
        "templates",
        "excel",
        $"{invoice!.TemplateName ?? ""}.xlsx"
    );

    string outputFile = GetOutputFilePath(invoice.Date, invoice.InvoiceNo, invoice.TemplateName);

    CreateOutputExcel(outputFile, templateFile, invoice);
    AddLogo(outputFile, invoice.LogoImage ?? "print_stamp.png");

    if (!_hostEnv.IsProduction()) return;

    if (useSpirePrinter)
      PrintExcelBySpire(outputFile, invoice.PrinterName);
    else PrintExcelByInterop(outputFile, invoice.PrinterName);
  }
}