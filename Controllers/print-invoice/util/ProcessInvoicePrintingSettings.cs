
namespace PrintingApi.Controllers;
public partial class PrintInvoiceController {

  private static Invoice ProcessInvoicePrintingSettings(Invoice invoice, PrintingSettings? settings) {
    if (settings == null) return invoice;

    if (!invoice.GlobalPrinter &&
        invoice.TemplateName == "kitchen" &&
        settings.OutputClientInfoForGlobalKitchenPrinterOnly
      ) {
      invoice.ClientName = null;
      invoice.ClientPhone1 = null;
      invoice.ClientPhone2 = null;
      invoice.ClientAddress = null;
      invoice.ClientArea = null;
    }

    return invoice;

  }

}
