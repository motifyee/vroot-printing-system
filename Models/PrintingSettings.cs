
using Newtonsoft.Json;

namespace TemplatePrinting.Models;

public class PrintingSettings {
  [JsonProperty("print_receipt_for_pending_invoice")]
  public bool PrintReceiptForPendingInvoice { get; set; } = true;

  [JsonProperty("output_client_info_for_global_kitchen_printer_only")]
  public bool OutputClientInfoForGlobalKitchenPrinterOnly { get; set; } = false;

  [JsonProperty("print_items_if_edited_for_local_kitchen_printer")]
  public bool PrintItemsIfEditedForLocalKitchenPrinter { get; set; } = false;

  [JsonProperty("use_html_template")]
  public bool UseHtmlTemplate { get; set; } = false;

  [JsonProperty("use_spire_excel_printer")]
  public bool UseSpireExcelPrinter { get; set; } = false;
}