
using Newtonsoft.Json;

namespace PrintingLibrary;

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

  [JsonProperty("print_item_info_note")]
  public bool PrintItemInfoNote { get; set; } = true;

  /// <summary>
  /// Name of the default print stamp image
  /// </summary>
  [JsonProperty("default_print_stamp_image")]
  public string? PrintStampImage { get; set; } = null;

  /// <summary>
  /// Hash code for the default print stamp image
  /// </summary>
  [JsonProperty("default_print_stamp_image_hash")]
  public string? PrintStampHash { get; set; } = null;

  /// <summary>
  /// Enable or disable encryption of generated Excel files
  /// </summary>
  [JsonProperty("encrypt_generated_files")]
  public bool EncryptGeneratedFiles { get; set; } = true;
}