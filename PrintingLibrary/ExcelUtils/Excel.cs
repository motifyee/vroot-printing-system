using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using MiniExcelLibs.Picture;
using PrintingLibrary.EncryptUtils;

namespace PrintingLibrary.ExcelUtils;

public static class ExcelUtils {
  private static readonly ILogger _logger = NullLogger.Instance;

  public static int GetLastDataRowIndex(string filePath) {
    var rows = MiniExcel.Query(filePath).ToList();
    var lastRowIndex = 1;

    for (int i = 0; i < rows.Count; i++)
      foreach (var property in (IDictionary<string, object>)rows[i])
        if (property.Value != null) lastRowIndex = i + 1;

    return lastRowIndex;
  }

  public static void CreateOutputExcel(string outputFilePath, string templateFile, object data) {
    _logger.LogInformation("Creating file: {outputFile} \n", outputFilePath);

    var config = new OpenXmlConfiguration() {
      IgnoreTemplateParameterMissing = true,
      FillMergedCells = true,
      EnableWriteNullValueCell = true,
    };

    MiniExcel.SaveAsByTemplate(outputFilePath, templateFile, data, configuration: config);

    _logger.LogInformation("Excel file created: {outputFile} \n", outputFilePath);
  }

  public static bool AddPrintStamp(string filePath, string? printStampImageName, string? printStampHash, string? hashSecret, string? stampRowIndex = null) {
    if (string.IsNullOrEmpty(printStampImageName)) return false;
    if (string.IsNullOrEmpty(printStampHash)) return false;
    if (string.IsNullOrEmpty(hashSecret)) return false;

    string imageFile = Path.Combine(
                Environment.CurrentDirectory,
                "printer",
                "images",
                printStampImageName
            );

    if (!File.Exists(imageFile)) {
      _logger.LogError("Logo image not found: {imageFile}", imageFile);
      return false;
    }

    var computedHash = EncryptUtil.GetFileHash(imageFile, hashSecret);
    if (computedHash != printStampHash) {
      _logger.LogError("Print stamp hash mismatch! Expected: {expected}, Got: {actual}", printStampHash, computedHash);
      return false;
    }

    var _stampRowIndex = stampRowIndex ?? $"A{GetLastDataRowIndex(filePath) + 2}";
    MiniExcel.AddPicture(filePath, new MiniExcelPicture {
      ImageBytes = File.ReadAllBytes(imageFile),
      PictureType = "image/png",
      CellAddress = _stampRowIndex,
      WidthPx = 300,
    });

    return true;
  }

  public static bool AddPrintStamp(string filePath, byte[]? imageBytes, string? stampRowIndex = null) {
    if (imageBytes == null) return false;

    var _stampRowIndex = stampRowIndex ?? $"A{GetLastDataRowIndex(filePath) + 2}";
    MiniExcel.AddPicture(filePath, new MiniExcelPicture {
      ImageBytes = imageBytes,
      PictureType = "image/png",
      CellAddress = _stampRowIndex,
      WidthPx = 300,
    });

    return true;
  }
}