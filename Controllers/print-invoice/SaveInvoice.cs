using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace TemplatePrinting.Controllers;
public partial class PrintInvoiceController {

  [HttpPost("save-invoice")]
  public ActionResult SaveInvoice([FromBody] dynamic invoice) {
    SaveAsJson(invoice);
    return Ok("Invoice saved successfully!");
  }

  private void SaveAsJson(dynamic invoice) {
    var date = invoice.GetProperty("CreateDate")?.ToString() ?? DateTime.Now.ToString("yyyy-MM-dd");
    var invoiceNo = invoice.GetProperty("InvoiceId")?.ToString() ?? "0000";

    var outputFile = GetOutputFilePath(date, invoiceNo, "invoice", "json");

    // Convert.ToString(invoice);
    var output = JsonSerializer.Serialize(
      invoice,
      new JsonSerializerOptions {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
      }
    );

    System.IO.File.WriteAllText(outputFile, output, Encoding.Unicode);
  }
}