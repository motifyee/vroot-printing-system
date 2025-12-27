
using Microsoft.AspNetCore.Mvc;

namespace TemplatePrinting.Controllers;

public partial class PrintInvoiceController {

  [HttpPost("Printers/Test")]
  public async Task<ActionResult> TestPrinter([FromBody] TestPrinterRequest request) {
    if (string.IsNullOrEmpty(request.PrinterName)) {
      return BadRequest("Printer name is required");
    }

    try {

      var templateFile = Path.Combine(
        AssemblyPath,
        "printer",
        "templates",
        "excel",
        "test.xlsx"
      );

      string outputFile = GetOutputFilePath(DateTime.Now.ToString("yyyy-MM-dd"), $"TEST-{request.PrinterName}", "test");

      CreateOutputExcel(outputFile, templateFile, new { request.PrinterName });
      AddLogo(outputFile, "print_stamp.png", "A10");

      if (_util.PrintingSettings?.UseSpireExcelPrinter ?? false)
        PrintExcelBySpire(outputFile, request.PrinterName);
      else PrintExcelByInterop(outputFile, request.PrinterName);

      _logger.LogInformation("Sending test Excel page to printer: {PrinterName}", request.PrinterName);

      return Ok(new { message = $"Test page (Excel) sent to {request.PrinterName}" });
    } catch (Exception e) {
      _logger.LogError(e, "Error sending test page to printer {PrinterName}", request.PrinterName);
      return StatusCode(500, new { error = e.Message });
    }
  }
}

public class TestPrinterRequest {
  public string? PrinterName { get; set; }
}
