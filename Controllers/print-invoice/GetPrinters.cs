using Microsoft.AspNetCore.Mvc;
using System.Drawing.Printing;
using System.Runtime.InteropServices;

namespace TemplatePrinting.Controllers;
public partial class PrintInvoiceController {

  [HttpGet("Printers")]
  public ActionResult GetPrinters() {
    List<string> printers = [];

    if (!System.Runtime.InteropServices.RuntimeInformation
                                             .IsOSPlatform(OSPlatform.Windows)) return Ok(printers);

#pragma warning disable CA1416 // Validate platform compatibility
    foreach (string printer in PrinterSettings.InstalledPrinters) {
#pragma warning restore CA1416 // Validate platform compatibility
      printers.Add(printer);
    }
    return Ok(printers);
  }



}
