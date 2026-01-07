
using PrintingLibrary.PuppeteerUtils;
using PuppeteerSharp;

namespace TemplatePrinting.Controllers;

public partial class PrintInvoiceController {
  private static async Task<byte[]> HtmlToPdfData(string templateName, string html) {

    var browser = PuppeteerUtils.Browser;
    using var page = await browser.NewPageAsync();

    var o = new NavigationOptions {
      WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded },
    };

    // Load fonts and images
    var path = Path.GetFullPath($"printer/templates/html/{templateName}.html");
    await page.GoToAsync($"file://{path}", o);
    await page.SetContentAsync(html);
    var pdfData = await page.PdfDataAsync(new PdfOptions {
      Format = new PuppeteerSharp.Media.PaperFormat(3.14961M, 100),
      PrintBackground = true,
    });

    await page.CloseAsync();

    return pdfData;
  }
}