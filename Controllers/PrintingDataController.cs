using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
namespace TodoApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PrintingDataController : ControllerBase {

    private readonly ILogger<PrintingDataController> _logger;

    public PrintingDataController(ILogger<PrintingDataController> logger) {
        _logger = logger;
    }

    [HttpPost(Name = "PostPrintingData")]
    public dynamic Post([FromBody] Invoice invoice) {

        string outputfile = $"{invoice.Date}#{invoice.InvoiceNo}-{Guid.NewGuid()}.xlsx";
        string outputPath = Path.Combine(Environment.CurrentDirectory, "printer", "out", outputfile);

        string inputPath = Path.Combine(Environment.CurrentDirectory, "printer", "templates", $"{invoice.TemplateName ?? ""}.xlsx");

        var config = new OpenXmlConfiguration() {
            IgnoreTemplateParameterMissing = true,
            FillMergedCells = true,
            EnableWriteNullValueCell = true
        };

        try {

            MiniExcel.SaveAsByTemplate(@outputPath, @inputPath, invoice, configuration: config);

            // TODO: check if required printer exists otherwise it sends to default
            // TODO: clean up files after sending to printer
            // TODO: check if template && lib files exists

            var result = SendXlsx2PrinterByInterop(outputPath, invoice.PrinterName);

            if (result.Trim() == "")
                return Ok();
            return StatusCode(StatusCodes.Status500InternalServerError, result);

        } catch (Exception e) {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }


    private string SendXlsx2PrinterByInterop(string filePath, string? printerName) {
        string result = "";
        using (var process = new Process()) {
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "printer", "lib", "printer.exe"));
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
            }
            while (!process.WaitForExit(500));

        }
        return result;
    }




}