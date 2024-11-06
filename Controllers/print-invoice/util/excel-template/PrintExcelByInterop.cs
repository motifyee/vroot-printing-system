using System.Diagnostics;
using TemplatePrinting.Models;

namespace TemplatePrinting.Controllers;
public partial class PrintInvoiceController {
  private string PrintExcelByInterop(string filePath, string? printerName) {
    _logger.LogInformation("printing using excel interop");
    var timer = new PerfTimer("time to print");

    string result = "";
    using (var process = new Process()) {
      var path = Path.GetFullPath(
          Path.Combine(Environment.CurrentDirectory, "printer", "lib", "interop", "printer.exe")
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

}
