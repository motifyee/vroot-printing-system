using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PrintingLibrary.Setup;

namespace PrintingLibrary.InteropUtils;

public static class InteropUtils {
  private static readonly ILogger _logger = NullLogger.Instance;

  public static string PrintExcelFile(string filePath, string? printerName) {
    _logger.LogInformation("printing using excel interop");
    var timer = new PerfTimer("time to print");

    string result = "";
    using (var process = new Process()) {
      var path = Path.GetFullPath(
          Path.Combine(PrintingSetup.BaseDirectory, "lib", "interop", "printer.exe")
      );
      process.StartInfo.FileName = path;
      process.StartInfo.Arguments = $"\"{filePath}\" \"{printerName}\"";
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;

      process.OutputDataReceived += (sender, data) => result += data.Data ?? "";
      process.ErrorDataReceived += (sender, data) => result += data.Data ?? "";
      _logger.LogInformation("starting to print");
      process.Start();
      process.BeginOutputReadLine();
      process.BeginErrorReadLine();
      do {
        if (!process.HasExited) {
          // Refresh the current process property values.
          process.Refresh();
          // _logger.LogInformation($"exit {process.HasExited}, result: {result}");
        }
      } while (!process.WaitForExit(100));
    }

    timer.End();

    return result;
  }

  public static string PrintExcelFile(Stream stream, string? printerName) {
    var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".xlsx");
    try {
      using (var fileStream = File.OpenWrite(tempFilePath)) {
        if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);
        stream.CopyTo(fileStream);
      }
      return PrintExcelFile(tempFilePath, printerName);
    } finally {
      if (File.Exists(tempFilePath)) {
        File.Delete(tempFilePath);
      }
    }
  }

}
