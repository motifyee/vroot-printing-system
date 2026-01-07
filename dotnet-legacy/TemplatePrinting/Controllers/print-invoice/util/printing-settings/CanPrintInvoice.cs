
using PrintingLibrary;
using TemplatePrinting.Models.Invoice;

namespace TemplatePrinting.Controllers;

public partial class PrintInvoiceController {

  private static bool CanPrintInvoice(Invoice invoice, PrintingSettings? settings) {
    if (settings == null) return true;

    if (!settings.PrintReceiptForPendingInvoice &&
          invoice.InvoiceType?.Trim() == "صالة" &&
          invoice.TemplateName == "receipt" &&
          invoice.Status == 1
      )
      return false;

    return true;
  }
}
