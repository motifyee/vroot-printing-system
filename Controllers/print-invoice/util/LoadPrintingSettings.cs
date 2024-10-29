

using Newtonsoft.Json;

namespace TemplatePrinting.Controllers;

public partial class PrintInvoiceController {

  private static PrintingSettings? LoadPrintingSettings() {
    if (!System.IO.File.Exists("printing-settings.json")) return null;

    PrintingSettings? settings;
    using (StreamReader r = new("printing-settings.json"))
      settings = JsonConvert.DeserializeObject<PrintingSettings>(r.ReadToEnd());

    return settings;
  }

}
