
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using TemplatePrinting.Models;
using TemplatePrinting.Models.Invoice;

namespace TemplatePrinting.Controllers;
public partial class PrintInvoiceController {

  private void PrintInvoiceByExcel(Invoice invoice, bool useSpirePrinter) {
    var config = new OpenXmlConfiguration() {
      IgnoreTemplateParameterMissing = true,
      FillMergedCells = true,
      EnableWriteNullValueCell = true
    };

    string templateFile = Path.Combine(
        Environment.CurrentDirectory,
        "printer",
        "templates",
        "excel",
        $"{invoice!.TemplateName ?? ""}.xlsx"
    );

    string outputFile = GetOutputFilePath(invoice.Date, invoice.InvoiceNo, invoice.TemplateName);

    _logger.LogInformation("Creating file: {outputFile} \n", outputFile);

    var timer = new PerfTimer("time to create excel file");
    MiniExcel.SaveAsByTemplate(outputFile, templateFile, invoice, configuration: config);
    timer.End();

    if (!_hostEnv.IsProduction()) return;

    if (useSpirePrinter)
      PrintExcelBySpire(outputFile, invoice.PrinterName);
    else PrintExcelByInterop(outputFile, invoice.PrinterName);
  }
}