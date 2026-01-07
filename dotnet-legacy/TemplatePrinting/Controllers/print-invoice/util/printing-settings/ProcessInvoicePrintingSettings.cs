
using PrintingLibrary;
using TemplatePrinting.Models.Invoice;

namespace TemplatePrinting.Controllers;

public partial class PrintInvoiceController {
  private static Invoice ProcessInvoicePrintingSettings(Invoice invoice, PrintingSettings settings) {
    if (!(invoice.GlobalPrinter ?? false) &&
        invoice.TemplateName == "kitchen" &&
        settings.OutputClientInfoForGlobalKitchenPrinterOnly
      ) {
      invoice.ClientName = null;
      invoice.ClientPhone1 = null;
      invoice.ClientPhone2 = null;
      invoice.ClientAddress = null;
      invoice.ClientArea = null;
    }

    if (
      invoice.EditedItems.Count > 0 &&
      invoice.GlobalPrinter != true &&
      invoice.TemplateName == "kitchen" &&
      !settings.PrintItemsIfEditedForLocalKitchenPrinter
    )
      invoice.Items = [];

    return invoice;

  }

}
