
using PrintingLibrary.Setup;
using Scriban;
using TemplatePrinting.Models.Invoice;

namespace TemplatePrinting.Controllers;

public partial class PrintInvoiceController {

  private static string ParseHtmlTemplate(Invoice invoice) {
    var templatePath = Path.Combine(
      PrintingSetup.AssemblyPath,
      "printer",
      "templates",
      "html",
      $"{invoice.TemplateName}.html"
    );
    var templateData = System.IO.File.ReadAllText(templatePath);

    var template = Template.Parse(templateData);
    var result = template.Render(invoice);
    return result;
  }

}