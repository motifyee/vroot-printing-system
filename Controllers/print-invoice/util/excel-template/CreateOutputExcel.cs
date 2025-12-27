using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using MiniExcelLibs.Picture;
using TemplatePrinting.Models.Invoice;
using System.Security.Cryptography;
using System.Text;

namespace TemplatePrinting.Controllers;

public partial class PrintInvoiceController {


  private void CreateOutputExcel(string outputFilePath, string templateFile, Invoice invoice) {
    _logger.LogInformation("Creating file: {outputFile} \n", outputFilePath);

    var config = new OpenXmlConfiguration() {
      IgnoreTemplateParameterMissing = true,
      FillMergedCells = true,
      EnableWriteNullValueCell = true,
    };

    var templateWatcher = System.Diagnostics.Stopwatch.StartNew();
    MiniExcel.SaveAsByTemplate(outputFilePath, templateFile, invoice, configuration: config);
    templateWatcher.Stop();
    _logger.LogInformation("Time to create excel file: {time} \n", templateWatcher.Elapsed);

    var stampWatcher = System.Diagnostics.Stopwatch.StartNew();
    var printStampImageName = invoice.PrintStampImageName ?? _util.PrintingSettings.PrintStampImage;
    var printStampHash = invoice.PrintStampHash ?? _util.PrintingSettings.PrintStampHash;
    var stampAdded = AddPrintStamp(outputFilePath, printStampImageName, printStampHash, _util.PrintStampSecret);
    stampWatcher.Stop();
    if (invoice.PrintStampImageName != null || stampAdded)
      _logger.LogInformation("Time to add print stamp: {time} \n", stampWatcher.Elapsed);

    _logger.LogInformation("Excel file created: {outputFile} \n", outputFilePath);
  }

  // add.PrintStampSecret
  private bool AddPrintStamp(string templateFilePath, string? printStampImageName, string? printStampHash, string? hashSecret) {
    if (string.IsNullOrEmpty(printStampImageName)) return false;
    if (string.IsNullOrEmpty(printStampHash)) return false;
    if (string.IsNullOrEmpty(hashSecret)) return false;

    string imageFile = Path.Combine(
                Environment.CurrentDirectory,
                "printer",
                "images",
                printStampImageName
            );

    if (!System.IO.File.Exists(imageFile)) {
      _logger.LogError("Logo image not found: {imageFile}", imageFile);
      return false;
    }

    var computedHash = GetFileHash(imageFile, hashSecret);
    if (computedHash != printStampHash) {
      _logger.LogError("Print stamp hash mismatch! Expected: {expected}, Got: {actual}", printStampHash, computedHash);
      return false;
    }

    var stampRowIndex = GetLastDataRowIndex(templateFilePath) + 2;
    MiniExcel.AddPicture(templateFilePath, new MiniExcelPicture {
      ImageBytes = System.IO.File.ReadAllBytes(imageFile),
      PictureType = "image/png",
      CellAddress = $"A{stampRowIndex}",
      WidthPx = 320,
    });

    return true;
  }

  private static int GetLastDataRowIndex(string filePath) {
    var rows = MiniExcel.Query(filePath).ToList();
    var lastRowIndex = 1;

    for (int i = 0; i < rows.Count; i++)
      foreach (var property in (IDictionary<string, object>)rows[i])
        if (property.Value != null) lastRowIndex = i + 1;

    return lastRowIndex;
  }

  private static string GetFileHash(string filePath, string secret) {
    var keyBytes = Encoding.UTF8.GetBytes(secret);
    using var hmac = new HMACSHA256(keyBytes);
    using var stream = System.IO.File.OpenRead(filePath);
    var hashBytes = hmac.ComputeHash(stream);
    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
  }
}
