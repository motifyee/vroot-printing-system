using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TemplatePrinting.Models.Invoice;
using PrintingLibrary.Setup;
using PrintingLibrary.EmbeddedResourcesUtils;
using TemplatePrinting.Models;
using TemplatePrinting.services;

namespace TemplatePrinting.Controllers;



[ApiController]
[Route("")]
public partial class PrintInvoiceController(
    ILogger<PrintInvoiceController> @logger,
    IWebHostEnvironment @hostEnvironment,
    IPrintingSetup @util,
    Resources<Asset> @resources,
    DataService @data
) : ControllerBase {

  [HttpGet("", Name = "Index")]
  public IActionResult Index() {
    var filePath = Path.Combine(hostEnvironment.WebRootPath, "printers/index.html");
    if (!System.IO.File.Exists(filePath)) return NotFound("Dashboard not found");
    return PhysicalFile(filePath, "text/html");
  }

  [HttpPost("", Name = "PrintInvoice")]
  [HttpPost("PrintingData", Name = "PostPrintingData")]
  public async Task<ActionResult> PrintInvoice([FromBody] Invoice invoice) {
    var settings = util.Settings;

    if (!CanPrintInvoice(invoice, settings))
      return Ok("Receipt for pending invoice not printed");

    invoice = ProcessInvoicePrintingSettings(invoice, settings);

    try {
      // TODO: check if required printer exists otherwise it sends to default
      // TODO: clean up files after sending to printer
      // TODO: check if template && lib files exists

      if (settings.UseHtmlTemplate) await PrintInvoiceByHtml(invoice);
      else PrintInvoiceByExcel(invoice, settings.UseSpireExcelPrinter);

      // SaveAsJson(invoice);

      return Ok();

    } catch (Exception e) {
      logger.LogInformation("exception: {Message}", e.Message);
      var err = $"message = {e.Message}, stack = {e.StackTrace}";
      return StatusCode(StatusCodes.Status500InternalServerError, err);
    }
  }

  private static void SaveAsJson(Invoice invoice) {
    var outputFile = GetOutputFilePath(invoice.Date, invoice.InvoiceNo, invoice.TemplateName, "json");
    var json = JsonConvert.SerializeObject(invoice, Formatting.Indented);

    System.IO.File.WriteAllText(outputFile, json);
  }

  private (Asset, PrintStampInfo?) GetPrintStampAssetAndInfo(string? printerName) {
    var stampAsset = Asset.PrintStamp;
    var stampInfo = data.Assets[stampAsset];
    if (string.IsNullOrEmpty(printerName)) return (stampAsset, stampInfo);

    try {
      var printerSettings = new System.Drawing.Printing.PrinterSettings { PrinterName = printerName };
      if (!printerSettings.IsValid) return (stampAsset, stampInfo);

      // width unit is in 1/1000 of an inch = 0.254mm
      // 1mm = 3.7795275591px
      var width = printerSettings.DefaultPageSettings.PaperSize.Width;
      // 80mm is ~315 units, 72mm is ~283 units. Using 290 as threshold.
      if (width > 0 && width <= 290) {
        stampAsset = Asset.PrintStamp72;
        stampInfo = data.Assets[stampAsset];
        logger.LogInformation("Selected 72mm stamp for printer {PrinterName} (Width: {Width})", printerName, width);
      }
    } catch (Exception ex) {
      logger.LogWarning(ex, "Failed to determine printer width for {PrinterName}, defaulting to 80mm stamp", printerName);
    }

    return (stampAsset, stampInfo);
  }

}
