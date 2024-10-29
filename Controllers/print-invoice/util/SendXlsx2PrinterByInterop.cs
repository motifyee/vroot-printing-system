using System.Diagnostics;

namespace TemplatePrinting.Controllers;
public partial class PrintInvoiceController {
  private static string SendXlsx2PrinterByInterop(string filePath, string? printerName) {
    string result = "";
    using (var process = new Process()) {
      var path = Path.GetFullPath(
          Path.Combine(Environment.CurrentDirectory, "printer", "lib", "printer.exe")
      );
      process.StartInfo.FileName = path;
      process.StartInfo.Arguments = $"\"{filePath}\" \"{printerName}\"";
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;

      process.OutputDataReceived += (sender, data) => result += data.Data ?? "";
      process.ErrorDataReceived += (sender, data) => result += data.Data ?? "";
      Console.WriteLine("starting to print");
      process.Start();
      process.BeginOutputReadLine();
      process.BeginErrorReadLine();
      do {
        if (!process.HasExited) {
          // Refresh the current process property values.
          process.Refresh();
          // Console.WriteLine($"exit {process.HasExited}, result: {result}");
        }
      } while (!process.WaitForExit(500));
    }
    return result;
  }

}
