using System.Drawing.Printing;
using System.Runtime.Versioning;
using TemplatePrinting.Models;

namespace TemplatePrinting.Controllers;

[SupportedOSPlatform("windows")]
public class PrinterBackgroundService : IDisposable {
  private readonly IJobCountStrategy _strategy;
  private readonly List<PrinterWatcher> _watchers = new();
  private bool _isRunning;
  private readonly object _lock = new();
  private int _subscriberCount = 0;
  private List<PrinterInfo> _printerCache = new();

  public event Action<List<PrinterInfo>>? OnPrintersChanged;

  public PrinterBackgroundService(IJobCountStrategy strategy) {
    _strategy = strategy;
  }

  public List<PrinterInfo> GetCachedPrinters() => _printerCache;

  public List<PrinterInfo> Subscribe(Action<List<PrinterInfo>> handler) {
    lock (_lock) {
      OnPrintersChanged += handler;
      _subscriberCount++;
      if (_subscriberCount == 1) {
        Start();
      }
      return [.. _printerCache];
    }
  }

  public void Unsubscribe(Action<List<PrinterInfo>> handler) {
    lock (_lock) {
      OnPrintersChanged -= handler;
      _subscriberCount--;
      if (_subscriberCount <= 0) {
        _subscriberCount = 0;
        Stop();
      }
    }
  }

  public bool IsRunning => _isRunning;

  public void Start() {
    lock (_lock) {
      if (_isRunning) return;
      _isRunning = true;
    }

    StopInternal(); // Ensure clean state

    try {
      _printerCache = FetchInitialPrinters();
      foreach (var info in _printerCache) {
        var watcher = new PrinterWatcher(info.Name, _strategy);
        watcher.OnQueueUpdated += HandleQueueUpdated;
        _watchers.Add(watcher);
      }
    } catch (Exception ex) {
      Console.WriteLine($"Error starting printer watchers: {ex.Message}");
      Stop();
    }
  }

  private List<PrinterInfo> FetchInitialPrinters() {
    List<PrinterInfo> printers = [];
    Dictionary<string, System.Management.ManagementObject>? wmiPrinters = null;

    // Try to get WMI printer data with culture-safe initialization
    try {
      // Save current culture and switch to invariant culture for WMI operations
      var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
      var currentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;

      try {
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

        var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_Printer");
        wmiPrinters = searcher.Get().Cast<System.Management.ManagementObject>().ToDictionary(
            x => x["Name"]?.ToString() ?? "",
            x => x
        );
        Console.WriteLine($"Successfully loaded WMI printer data for {wmiPrinters.Count} printers");
      } finally {
        // Restore original culture
        System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
        System.Threading.Thread.CurrentThread.CurrentUICulture = currentUICulture;
      }
    } catch (System.TypeInitializationException ex) {
      Console.WriteLine($"WMI initialization failed (possibly due to locale/Arabic characters): {ex.Message}");
      Console.WriteLine("Falling back to basic printer enumeration without WMI data");
    } catch (Exception ex) {
      Console.WriteLine($"Error loading WMI printer data: {ex.Message}");
      Console.WriteLine("Falling back to basic printer enumeration without WMI data");
    }

    // Enumerate printers using PrinterSettings (always works)
    try {
      foreach (string printerName in PrinterSettings.InstalledPrinters) {
        var settings = new PrinterSettings { PrinterName = printerName };
        var info = new PrinterInfo {
          Name = printerName,
          FullName = printerName, // Default to Name if WMI unavailable
          IsDefault = settings.IsDefaultPrinter,
          IsValid = settings.IsValid,
          SupportsColor = settings.SupportsColor,
          MaxCopies = settings.MaximumCopies,
          Duplex = settings.Duplex.ToString(),
          PaperSizes = [.. settings.PaperSizes.Cast<PaperSize>().Select(x => new PaperSizeInfo {
            Name = x.PaperName,
            Width = x.Width,
            Height = x.Height,
            Kind = x.Kind.ToString(),
            RawKind = x.RawKind
          })],
          DefaultPaperSize = new PaperSizeInfo {
            Name = settings.DefaultPageSettings.PaperSize.PaperName,
            Width = settings.DefaultPageSettings.PaperSize.Width,
            Height = settings.DefaultPageSettings.PaperSize.Height,
            Kind = settings.DefaultPageSettings.PaperSize.Kind.ToString(),
            RawKind = settings.DefaultPageSettings.PaperSize.RawKind
          },
          Resolutions = settings.PrinterResolutions.Cast<PrinterResolution>().Select(x => x.ToString()).ToList(),
          JobCount = _strategy.GetJobCount(printerName)
        };

        // Enhance with WMI data if available
        if (wmiPrinters?.TryGetValue(printerName, out var mo) == true) {
          info.FullName = mo["Caption"]?.ToString() ?? printerName;
          info.PortName = mo["PortName"]?.ToString() ?? "";
          info.DriverName = mo["DriverName"]?.ToString() ?? "";
          info.Location = mo["Location"]?.ToString() ?? "";
          info.IsShared = (bool)(mo["Shared"] ?? false);
          info.ShareName = mo["ShareName"]?.ToString() ?? "";
          info.IsNetwork = (bool)(mo["Network"] ?? false);
          info.IsOffline = (bool)(mo["WorkOffline"] ?? false);
          info.Status = mo["Status"]?.ToString() ?? "";
        }

        printers.Add(info);
      }
      Console.WriteLine($"Successfully enumerated {printers.Count} printers");
    } catch (Exception ex) {
      Console.WriteLine($"Error enumerating printers: {ex.Message}");
    }

    return printers;
  }

  private void HandleQueueUpdated(string name, int count) {
    lock (_lock) {
      var printer = _printerCache.FirstOrDefault(p => p.Name == name);
      if (printer != null) {
        printer.JobCount = count;
        OnPrintersChanged?.Invoke([.. _printerCache]);
      }
    }
    Console.WriteLine($"Printer {name} now has {count} jobs.");
  }

  public void Stop() {
    lock (_lock) {
      if (!_isRunning) return;
      _isRunning = false;
    }
    StopInternal();
  }

  private void StopInternal() {
    foreach (var watcher in _watchers) {
      watcher.OnQueueUpdated -= HandleQueueUpdated;
      watcher.Dispose();
    }
    _watchers.Clear();
  }

  public void Restart() {
    Stop();
    Start();
  }

  public void Dispose() {
    Stop();
  }
}
