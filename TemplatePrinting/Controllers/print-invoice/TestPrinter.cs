
using Microsoft.AspNetCore.Mvc;
using PrintingLibrary.ExcelUtils;
using PrintingLibrary.InteropUtils;
using PrintingLibrary.SpireUtils;
using PrintingLibrary.Setup;
using TemplatePrinting.Models;

namespace TemplatePrinting.Controllers;

public partial class PrintInvoiceController {

  [HttpPost("Printers/Test")]
  public async Task<ActionResult> TestPrinter([FromBody] TestPrinterRequest request) {
    if (string.IsNullOrEmpty(request.PrinterName)) {
      return BadRequest("Printer name is required");
    }

    try {

      var templateFile = Path.Combine(
        PrintingSetup.AssemblyPath,
        "printer",
        "templates",
        "excel",
        "test.xlsx"
      );

      string outputFile = GetOutputFilePath(DateTime.Now.ToString("yyyy-MM-dd"), $"TEST-{request.PrinterName}", "test");

      ExcelUtils.CreateOutputExcel(outputFile, templateFile, new { request.PrinterName });

      var (asset, info) = GetPrintStampAssetAndInfo(request.PrinterName);
      if (info != null) {
        ExcelUtils.AddPrintStamp(
          filePath: outputFile,
          imageBytes: resources.GetBytes(asset),
          width: info.Width,
          height: info.Height,
          stampRowIndex: "A10"
        );
      }

      if (util.Settings?.UseSpireExcelPrinter ?? false)
        SpireUtils.PrintExcelFile(outputFile, request.PrinterName);
      else InteropUtils.PrintExcelFile(outputFile, request.PrinterName);

      logger.LogInformation("Sending test Excel page to printer: {PrinterName}", request.PrinterName);

      return Ok(new { message = $"Test page (Excel) sent to {request.PrinterName}" });
    } catch (Exception e) {
      logger.LogError(e, "Error sending test page to printer {PrinterName}", request.PrinterName);
      return StatusCode(500, new { error = e.Message });
    }
  }
}

public class TestPrinterRequest {
  public string? PrinterName { get; set; }
  public string? PrintStampImageName { get; set; }
  public string? PrintStampHash { get; set; }
}
