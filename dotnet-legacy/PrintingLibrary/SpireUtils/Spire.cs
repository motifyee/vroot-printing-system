
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PrintingLibrary.Setup;
using Spire.Xls;

namespace PrintingLibrary.SpireUtils;

public static class SpireUtils {

  private static readonly ILogger _logger = NullLogger.Instance;

  /// <summary>
  /// Load spire license key
  /// </summary>
  public static void LoadSpireLicenseKey() {
    string ss =
        "Idd65VXxKpEAgBvZ1nVhUN+w7vpItcbvurq9YsmKuDda+JAEE9qF2G4YzR3o0s96HLaSfKKXq8fmv/VifgjLP/ZHrAKRewKyimE+b1l5tI82tdsWa+W3TgkLfepngT3Ui+LuaUc8pxXYEPd/bacNeg6yvWi7xVPzxDsE/m3D+OyD1ifz4S4lkOhjUS4pJ9gIKv6eIx0aXzRyczi4c+55+yRRBjUsB3AUS5C4sGq4LaSbeVLRq52visiCeMQxIkO6G38uTOyJl3mplKPrB3tpSTpmDc0j1WLuce1KIA9GbtKqOgh5vJwnXnwR3qeVgEBY2Lgrt6Gu0RModahYN6N5ODyj526SSOsz50jUQsrjfnk2JYKq3D3GA+lshknDJsSyHHkqYNxXfha7GQ4e11FhxALPu81LBXLXez4l73XCV9n6cdvHnyOerI18clWh/g6lgfEG+N+ugko2oxET/WEeIVKoIvpEw9YMv5bQrD6oWlN5GthgiXawtPQ6kM41r0MKW75+6ojDqRbOqvyVwC4HNRf2MXjni/Bdo0KBG3SD119bQfa+4zBREiEz6X26Mv7Tc0n8YzGTcK7VZcRGqI06bp4RDiFvAMrn4Y83gJaVRX6MbSJqwpKXKugSrmf0ck6XzLmhQcjsznnLkToXxvBS2jh6Vy3JZXvt4l8JUF8zE9CPix+kpDcGedXA1MmN/dju6Ps4sgGGAnjrfl1YLHvbQR8kii+h9tKrUrjTT88xvjjwz5IXmC4MX2A6HjSqabQwLVm8wfwNF22Pp1nMuX5DVP2pyNMMYMHIewGlJRSQz3j/7gVbw264aeBJPGyVpxrZCRO7byu/Z8cKTk02S+vZTazhIjV4jmn8zLOsxH0wsbcEpDLw1XnrH4tUiIRDQxRO+EBtpPklyFx9Q8AYkIv91osUiQZ14MXfysJ8oHG8gqHa7uidcd+YgFc3FRlFlVXYqqQlABFg5/ZvUHUklZdiRLenTb2yfl3RffnzA1aevJcLy2sBoWUrTxZlAFu0u8D2+swu0V3juiLM8pO9VDB4gHtQh3n/cnvShuv8hls2fi0TTZvpxLdfBw==";

    Spire.License.LicenseProvider.SetLicenseKey(ss);
    Spire.License.LicenseProvider.LoadLicense();

    Console.WriteLine("Spire license loaded");
  }

  public static void PrintExcelFile(string filePath, string? printerName, string? password = null) {
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

    var workbook = new Workbook();

    _logger.LogInformation("printing using Spire.XLS: {file}", filePath);
    var timer = new PerfTimer("time to load excel file");

    if (password != null)
      workbook.OpenPassword = password;

    workbook.LoadFromFile(filePath);

    timer.Print("time to print");
    PrintWorkbook(workbook, printerName);
    timer.End();
  }

  public static void PrintExcelFile(Stream stream, string? printerName, string? password = null) {
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

    var workbook = new Workbook();

    _logger.LogInformation("printing using Spire.XLS from stream");
    var timer = new PerfTimer("time to load excel stream");

    if (password != null)
      workbook.OpenPassword = password;

    workbook.LoadFromStream(stream);

    timer.Print("time to print");
    PrintWorkbook(workbook, printerName);
    timer.End();
  }

  private static void PrintWorkbook(Workbook workbook, string? printerName) {
    if (printerName != null)
      workbook.PrintDocument.PrinterSettings.PrinterName = printerName;

    workbook.PrintDocument.PrinterSettings.PrintRange = PrintRange.Selection;

    workbook.PrintDocument.Print();
  }
}