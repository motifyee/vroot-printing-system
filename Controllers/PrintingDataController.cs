using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using Newtonsoft.Json;
namespace PrintingApi.Controllers {

  [ApiController]
  [Route("[controller]")]
  public class PrintingDataController : ControllerBase {
    private readonly ILogger<PrintingDataController> _logger;
    private readonly IHostEnvironment _hostEnv;
    public PrintingDataController(ILogger<PrintingDataController> logger, IHostEnvironment hostEnvironment) {
      _logger = logger;
      _hostEnv = hostEnvironment;
    }

    [HttpGet(Name = "TestPrintingData")]
    public dynamic Get() => Ok("Printing Data Api Works!");

    [HttpPost(Name = "PostPrintingData")]
    public dynamic Post([FromBody] Invoice invoice) {
      if (!CanPrintInvoice(invoice)) return Ok("Receipt for pending invoice not printed");

      var culture = new CultureInfo("ar-EG");
      var date = DateTime.Parse(invoice.Date ?? "");

      string year = date.Year.ToString();
      string month = $"{date.Month.ToString()} — {date.ToString("MMMM", culture)}";
      string day = $"{date.Day.ToString()} — {date.ToString("dddd", culture)}";

      string outputfile = $"{invoice.Date ?? ""} #{invoice.InvoiceNo} — {Guid.NewGuid()}.xlsx";

      string folderPath = Path.Combine(
          Environment.CurrentDirectory,
          "printer",
          "out",
          year,
          month,
          day,
          $"فاتورة — {invoice?.InvoiceNo}"
      );
      if (!Directory.Exists(folderPath)) {
        Directory.CreateDirectory(folderPath);
      }
      string outputPath = Path.Combine(folderPath, outputfile);

      string inputPath = Path.Combine(
          Environment.CurrentDirectory,
          "printer",
          "templates",
          $"{invoice!.TemplateName ?? ""}.xlsx"
      );

      var config = new OpenXmlConfiguration() {
        IgnoreTemplateParameterMissing = true,
        FillMergedCells = true,
        EnableWriteNullValueCell = true
      };

      try {
        Console.WriteLine("creating file: " + @outputfile);

        MiniExcel.SaveAsByTemplate(@outputPath, @inputPath, invoice, configuration: config);

        // TODO: check if required printer exists otherwise it sends to default
        // TODO: clean up files after sending to printer
        // TODO: check if template && lib files exists


        if (_hostEnv.IsProduction())
          SendXlsx2PrinterByInterop(outputPath, invoice.PrinterName);

        return Ok();
      } catch (Exception e) {
        Console.WriteLine(e.Message);
        var err = $"message = {e.Message}, stack = {e.StackTrace}";
        return StatusCode(StatusCodes.Status500InternalServerError, err);
      }
    }

    public bool CanPrintInvoice(Invoice invoice) {
      if (!System.IO.File.Exists("printing-settings.json")) return true;

      PrintingSettings? settings;
      using (StreamReader r = new("printing-settings.json"))
        settings = JsonConvert.DeserializeObject<PrintingSettings>(r.ReadToEnd());

      if (settings == null) return true;

      if (!settings.PrintReceiptForPendingInvoice && 
            invoice.InvoiceType.Trim() == "صالة" &&
            invoice.TemplateName == "receipt" && 
            invoice.Status == 1
        )
        return false;

      return true;
    }

    public class PrintingSettings {
      [JsonProperty("print_receipt_for_pending_invoice")]
      public bool PrintReceiptForPendingInvoice { get; set; } = true;
    }

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

}