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

  private static void SaveAsJson(dynamic invoice) {
    string date = DateTime.Now.ToString("yyyy-MM-dd");
    var invoiceNo = "0000";

    try {
      date = invoice.GetProperty("createDate")?.ToString() ?? date;
    } catch (Exception) { }
    try {
      invoiceNo = invoice.GetProperty("flagByDateCompany")?.ToString() ?? invoiceNo;
    } catch (Exception) { }

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