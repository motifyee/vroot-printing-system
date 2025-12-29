using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TemplatePrinting.Models.Invoice;
using PrintingLibrary.Setup;
using PrintingLibrary.EmbeddedResourcesUtils;
using TemplatePrinting.Models;

namespace TemplatePrinting.Controllers;

[ApiController]
[Route("")]
public partial class PrintInvoiceController(
    ILogger<PrintInvoiceController> logger,
    IHostEnvironment hostEnvironment,
    IPrintingSetup util,
    Resources<Assets> resources
) : ControllerBase {
  private readonly ILogger<PrintInvoiceController> _logger = logger;
  private readonly IHostEnvironment _hostEnv = hostEnvironment;
  private readonly IPrintingSetup _util = util;
  private readonly Resources<Assets> _resources = resources;

  [HttpGet("PrintingData", Name = "TestPrintingData")]
  public dynamic TestPrintingData() {
    return Ok("Printing Data Api Works!");
  }

  [HttpPost("PrintingData", Name = "PostPrintingData")]
  public async Task<ActionResult> PrintInvoice([FromBody] Invoice invoice) {
    var settings = _util.Settings;

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
      _logger.LogInformation("exception: {Message}", e.Message);
      var err = $"message = {e.Message}, stack = {e.StackTrace}";
      return StatusCode(StatusCodes.Status500InternalServerError, err);
    }
  }

  private static void SaveAsJson(Invoice invoice) {
    var outputFile = GetOutputFilePath(invoice.Date, invoice.InvoiceNo, invoice.TemplateName, "json");
    var json = JsonConvert.SerializeObject(invoice, Formatting.Indented);

    System.IO.File.WriteAllText(outputFile, json);
  }
}
