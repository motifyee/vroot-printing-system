
using Newtonsoft.Json;
using PuppeteerSharp;
using TemplatePrinting.Models;

namespace TemplatePrinting.Services;

public class PrintingUtils() : IPrintingUtils {
  private static IBrowser? _browser;
  public void Setup() {
    LoadSpireLicenseKey();
    _ = Browser;
  }
  public void LoadSpireLicenseKey() {
    string ss = "Idd65VXxKpEAgBvZ1nVhUN+w7vpItcbvurq9YsmKuDda+JAEE9qF2G4YzR3o0s96HLaSfKKXq8fmv/VifgjLP/ZHrAKRewKyimE+b1l5tI82tdsWa+W3TgkLfepngT3Ui+LuaUc8pxXYEPd/bacNeg6yvWi7xVPzxDsE/m3D+OyD1ifz4S4lkOhjUS4pJ9gIKv6eIx0aXzRyczi4c+55+yRRBjUsB3AUS5C4sGq4LaSbeVLRq52visiCeMQxIkO6G38uTOyJl3mplKPrB3tpSTpmDc0j1WLuce1KIA9GbtKqOgh5vJwnXnwR3qeVgEBY2Lgrt6Gu0RModahYN6N5ODyj526SSOsz50jUQsrjfnk2JYKq3D3GA+lshknDJsSyHHkqYNxXfha7GQ4e11FhxALPu81LBXLXez4l73XCV9n6cdvHnyOerI18clWh/g6lgfEG+N+ugko2oxET/WEeIVKoIvpEw9YMv5bQrD6oWlN5GthgiXawtPQ6kM41r0MKW75+6ojDqRbOqvyVwC4HNRf2MXjni/Bdo0KBG3SD119bQfa+4zBREiEz6X26Mv7Tc0n8YzGTcK7VZcRGqI06bp4RDiFvAMrn4Y83gJaVRX6MbSJqwpKXKugSrmf0ck6XzLmhQcjsznnLkToXxvBS2jh6Vy3JZXvt4l8JUF8zE9CPix+kpDcGedXA1MmN/dju6Ps4sgGGAnjrfl1YLHvbQR8kii+h9tKrUrjTT88xvjjwz5IXmC4MX2A6HjSqabQwLVm8wfwNF22Pp1nMuX5DVP2pyNMMYMHIewGlJRSQz3j/7gVbw264aeBJPGyVpxrZCRO7byu/Z8cKTk02S+vZTazhIjV4jmn8zLOsxH0wsbcEpDLw1XnrH4tUiIRDQxRO+EBtpPklyFx9Q8AYkIv91osUiQZ14MXfysJ8oHG8gqHa7uidcd+YgFc3FRlFlVXYqqQlABFg5/ZvUHUklZdiRLenTb2yfl3RffnzA1aevJcLy2sBoWUrTxZlAFu0u8D2+swu0V3juiLM8pO9VDB4gHtQh3n/cnvShuv8hls2fi0TTZvpxLdfBw==";

    Spire.License.LicenseProvider.SetLicenseKey(ss);
    Spire.License.LicenseProvider.LoadLicense();

    Console.WriteLine("Spire license loaded");
  }
  public IBrowser Browser {
    get {
      if (_browser != null && _browser.IsConnected) return _browser;

      Console.WriteLine("Loading Chrome Headless Shell...");

      var chromePath = Path.GetFullPath("printer/lib/chrome-headless-shell-win64/chrome-headless-shell.exe");
      if (File.Exists(chromePath)) {
        _browser = Puppeteer.LaunchAsync(
          new LaunchOptions {
            Headless = true,
            ExecutablePath = chromePath,
          }
        ).GetAwaiter().GetResult();

        Console.WriteLine("Chrome Headless Shell loaded from installation folder" + "\n");
        return _browser;
      }

      var browserFetcher = new BrowserFetcher();
      _ = browserFetcher.DownloadAsync().Result;

      _browser = Puppeteer.LaunchAsync(
        new LaunchOptions {
          Headless = true,
        }
      ).GetAwaiter().GetResult();

      Console.WriteLine("Chrome Headless Shell downloaded" + "\n");
      return _browser;
    }
  }
  public PrintingSettings? PrintingSettings {
    get {
      if (!File.Exists("printing-settings.json")) return null;

      PrintingSettings? settings;
      using (StreamReader r = new("printing-settings.json"))
        settings = JsonConvert.DeserializeObject<PrintingSettings>(r.ReadToEnd());

      return settings;
    }
  }

}
public interface IPrintingUtils {
  void LoadSpireLicenseKey();
  void Setup();
  IBrowser Browser { get; }
  PrintingSettings? PrintingSettings { get; }
}