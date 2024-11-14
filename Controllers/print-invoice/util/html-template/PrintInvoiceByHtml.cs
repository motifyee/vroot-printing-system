
using Microsoft.AspNetCore.Mvc;
using TemplatePrinting.Models;
using TemplatePrinting.Models.Invoice;

namespace TemplatePrinting.Controllers;
public partial class PrintInvoiceController {
  private async Task<ActionResult> PrintInvoiceByHtml(Invoice invoice) {
    var outputFile = GetOutputFilePath(invoice.Date, invoice.InvoiceNo, invoice.TemplateName, "pdf");
    _logger.LogInformation("Creating file: {outputFile} \n", outputFile);

    _logger.LogInformation("printing using Html template");
    var pt = new PerfTimer("time to parse invoice");
    var html = ParseHtmlTemplate(invoice);

    pt.Print("time to create pdf");
    var pdfData = await HtmlToPdfData(invoice.TemplateName ?? "receipt", html);

    pt.Print("time to print pdf");
    PrintPdfData(invoice, pdfData);

    pt.End();
    return Ok();
  }
}