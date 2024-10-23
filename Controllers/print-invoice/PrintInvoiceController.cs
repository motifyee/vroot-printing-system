using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;

namespace PrintingApi.Controllers;

[ApiController]
[Route("")]
public partial class PrintInvoiceController(
    ILogger<PrintInvoiceController> logger,
    IHostEnvironment hostEnvironment
) : ControllerBase {
  private readonly ILogger<PrintInvoiceController> _logger = logger;
  private readonly IHostEnvironment _hostEnv = hostEnvironment;

  [HttpGet("PrintingData", Name = "TestPrintingData")]
  public dynamic TestPrintingData() => Ok("Printing Data Api Works!");

  [HttpPost("PrintingData", Name = "PostPrintingData")]
  public dynamic PrintInvoice([FromBody] Invoice invoice) {
    var settings = LoadPrintingSettings();
    // invoice.TemplateName;
    // invoice.GlobalPrinter;
    // invoice.PrinterName;
    if (!CanPrintInvoice(invoice, settings))
      return Ok("Receipt for pending invoice not printed");
    invoice = ProcessInvoicePrintingSettings(invoice, settings);

    var culture = new CultureInfo("ar-EG");
    DateTime date;
    DateTime.TryParse(invoice.Date ?? DateTime.Now.ToString(), out date);

    string year = date.Year.ToString();
    string month = $"{date.Month.ToString()} — {date.ToString("MMMM", culture)}";
    string day = $"{date.Day.ToString()} — {date.ToString("dddd", culture)}";

    string outputfile = $"{invoice.Date ?? ""} #{invoice.InvoiceNo} — {Guid.NewGuid()}.xlsx";

    string folderPath = Path.Combine(
        Environment.CurrentDirectory,
        "printer",
        "out",
        year,
        month,
        day,
        $"فاتورة — {invoice?.InvoiceNo}"
    );
    if (!Directory.Exists(folderPath)) {
      Directory.CreateDirectory(folderPath);
    }
    string outputPath = Path.Combine(folderPath, outputfile);

    string inputPath = Path.Combine(
        Environment.CurrentDirectory,
        "printer",
        "templates",
        $"{invoice!.TemplateName ?? ""}.xlsx"
    );

    var config = new OpenXmlConfiguration() {
      IgnoreTemplateParameterMissing = true,
      FillMergedCells = true,
      EnableWriteNullValueCell = true
    };

    try {
      _logger.LogInformation("creating file: " + @outputfile);

      MiniExcel.SaveAsByTemplate(@outputPath, @inputPath, invoice, configuration: config);

      // TODO: check if required printer exists otherwise it sends to default
      // TODO: clean up files after sending to printer
      // TODO: check if template && lib files exists


      if (_hostEnv.IsProduction())
        SendXlsx2PrinterByInterop(outputPath, invoice.PrinterName);

      return Ok();
    } catch (Exception e) {
      Console.WriteLine(e.Message);
      var err = $"message = {e.Message}, stack = {e.StackTrace}";
      return StatusCode(StatusCodes.Status500InternalServerError, err);
    }
  }
}
