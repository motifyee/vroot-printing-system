
using System.Diagnostics;
using PrintingLibrary.Setup;
using PuppeteerSharp;

namespace PrintingLibrary.PuppeteerUtils;

public static class PuppeteerUtils {
  private static IBrowser? _browser;
  private static readonly string _execName = "chrome-headless-shell(template-printing)";

  /// <summary>
  /// Browser instance for printing html formatted documents
  /// </summary>
  public static IBrowser Browser {
    get {
      if (_browser != null && _browser.IsConnected)
        return _browser;
      _browser?.CloseAsync().GetAwaiter().GetResult();

      // close all chrome processes that might be unused
      // only problematic in case of multiple instances of this service
      foreach (var p in Process.GetProcessesByName(_execName))
        p.Kill();

      string execFolder = Path.Combine(
          PrintingSetup.BaseDirectory,
          "lib",
          "chrome-headless-shell-win64"
      );
      string execPath = Path.Combine(execFolder, _execName + ".exe");
      if (File.Exists(execPath)) {
        _browser = Puppeteer
            .LaunchAsync(new LaunchOptions { Headless = true, ExecutablePath = execPath, })
            .GetAwaiter()
            .GetResult();

        Console.WriteLine("Chrome Headless Shell loaded from installation folder" + "\n");
        return _browser;
      }

      Console.WriteLine("Downloading Chrome Headless Shell...");
      var browserFetcher = new BrowserFetcher();
      _ = browserFetcher.DownloadAsync().GetAwaiter().GetResult();
      // _browser = Puppeteer.LaunchAsync(new LaunchOptions { Headless = true, }).GetAwaiter().GetResult();

      // move downloaded files to lib folder & rename executable to _execName
      string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
      string assemblyDirectory = Path.GetDirectoryName(assemblyPath)!;
      var shellPath = Path.Combine(assemblyDirectory, "ChromeHeadlessShell");
      if (Directory.Exists(shellPath)) {
        var versionFolder = Directory.GetDirectories(shellPath)[0];
        File.Move(
            Path.Combine(
                versionFolder,
                "chrome-headless-shell-win64",
                "chrome-headless-shell.exe"
            ),
            Path.Combine(versionFolder, "chrome-headless-shell-win64", _execName + ".exe")
        );

        Directory.Move(
            Path.Combine(versionFolder, "chrome-headless-shell-win64"),
            execFolder
        );

        try {
          Directory.Delete(Path.Combine(assemblyDirectory, "Chrome"), true);
        } catch (System.Exception) { }
        try {
          Directory.Delete(Path.Combine(assemblyDirectory, "ChromeHeadlessShell"), true);
        } catch (System.Exception) { }

        Console.WriteLine("Installed Chrome Headless Shell");
      }

      return Browser;
    }
  }
}