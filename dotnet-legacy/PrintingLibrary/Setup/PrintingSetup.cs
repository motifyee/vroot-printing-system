using Newtonsoft.Json;

namespace PrintingLibrary.Setup;

public partial class PrintingSetup() : IPrintingSetup, IDisposable {

  /// <summary>
  /// Current working directory - where the application is launched from.
  /// Use this for: output files (printer/out), templates, and images that should be relative to the working directory.
  /// Debug: TemplatePrinting/ (project root)
  /// Publish: publish/ (or wherever the .exe is run from)
  /// </summary>
  public static readonly string AssemblyPath = Environment.CurrentDirectory;

  /// <summary>
  /// Base directory where assemblies are loaded - always points to the executable's location.
  /// Use this for: binary dependencies in lib/ folder (printer.exe, Chrome, DLLs).
  /// Debug: bin/Debug/net8.0/
  /// Publish: publish/
  /// </summary>
  public static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

  public void Setup() {
    SpireUtils.SpireUtils.LoadSpireLicenseKey();
    if (!Settings?.UseHtmlTemplate ?? false) return;
    _ = PuppeteerUtils.PuppeteerUtils.Browser;
  }


  public async void Dispose() {
    await PuppeteerUtils.PuppeteerUtils.Browser.CloseAsync();

    GC.Collect();
    GC.SuppressFinalize(this);
  }


  public PrintingSettings Settings {
    get {
      var settingsPath = Path.Combine(BaseDirectory, "lib", "printing-settings.json");
      if (!File.Exists(settingsPath))
        return new PrintingSettings();

      try {
        PrintingSettings? settings;
        using (StreamReader r = new(settingsPath))
          settings = JsonConvert.DeserializeObject<PrintingSettings>(r.ReadToEnd());

        return settings ?? new PrintingSettings();
      } catch (System.Exception) {
        return new PrintingSettings();
      }
    }
  }

  public string? PrintStampSecret {
    get { return null; }
  }
  public string? TemplateEncOutputSecret {
    get { return null; }
  }

  /// <summary>
  /// Encryption password for generated Excel files.
  /// This password is compiled into the assembly and not visible in settings files.
  /// Change this value before building for production.
  /// </summary>
  public string? EncryptionPassword {
    get { return Settings.EncryptGeneratedFiles ? "5PfY^484J@p9P&Ys%&Ya" : null; }
  }


}
