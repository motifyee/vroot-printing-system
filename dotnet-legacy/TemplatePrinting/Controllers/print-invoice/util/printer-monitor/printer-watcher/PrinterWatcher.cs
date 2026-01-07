using System.Runtime.InteropServices;

namespace TemplatePrinting.Controllers;

public interface IJobCountStrategy {
  int GetJobCount(string printerName);
}

public class PrinterWatcher : IDisposable {
  [DllImport("winspool.drv", SetLastError = true)]
  static extern IntPtr FindFirstPrinterChangeNotification(IntPtr hPrinter, int fdwFlags, int fdwOptions, IntPtr pPrinterNotifyOptions);

  [DllImport("winspool.drv", SetLastError = true)]
  static extern bool FindNextPrinterChangeNotification(IntPtr hChange, out int pdwChange, IntPtr pPrinterNotifyOptions, IntPtr ppPrinterNotifyInfo);

  [DllImport("winspool.drv", SetLastError = true)]
  static extern IntPtr OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

  [DllImport("winspool.drv", SetLastError = true)]
  static extern bool FindClosePrinterChangeNotification(IntPtr hChange);

  [DllImport("winspool.drv", SetLastError = true)]
  static extern bool ClosePrinter(IntPtr hPrinter);

  [DllImport("kernel32.dll", SetLastError = true)]
  static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

  private const int PRINTER_CHANGE_JOB = 0x0000FF00; // Monitors all Job-related changes
  private const uint WAIT_TIMEOUT = 0x00000102;
  private IntPtr _hPrinter;
  private IntPtr _hChange;
  private bool _disposed;
  private readonly IJobCountStrategy _strategy;
  private readonly string _printerName;

  public event Action<string, int> OnQueueUpdated;

  public PrinterWatcher(string printerName, IJobCountStrategy strategy) {
    _printerName = printerName;
    _strategy = strategy;

    if (OpenPrinter(printerName, out _hPrinter, IntPtr.Zero) != IntPtr.Zero) {
      _hChange = FindFirstPrinterChangeNotification(_hPrinter, PRINTER_CHANGE_JOB, 0, IntPtr.Zero);
    }

    // Run the watch loop in a dedicated background thread
    Task.Run(WatchLoop);
  }

  private void WatchLoop() {
    while (!_disposed && _hChange != IntPtr.Zero && _hChange != (IntPtr)(-1)) {
      // Wait with a timeout so we can check _disposed
      uint result = WaitForSingleObject(_hChange, 1000); // 1 second timeout

      if (result == 0) { // Success
        int changeOccurred;
        FindNextPrinterChangeNotification(_hChange, out changeOccurred, IntPtr.Zero, IntPtr.Zero);

        // Trigger the strategy and fire the event
        int count = _strategy.GetJobCount(_printerName);
        OnQueueUpdated?.Invoke(_printerName, count);
      } else if (result == WAIT_TIMEOUT) {
        // Just continue and check _disposed
        continue;
      } else {
        // Error or other result
        break;
      }
    }
  }

  public void Dispose() {
    _disposed = true;
    if (_hChange != IntPtr.Zero && _hChange != (IntPtr)(-1)) {
      FindClosePrinterChangeNotification(_hChange);
      _hChange = IntPtr.Zero;
    }
    if (_hPrinter != IntPtr.Zero) {
      ClosePrinter(_hPrinter);
      _hPrinter = IntPtr.Zero;
    }
  }
}