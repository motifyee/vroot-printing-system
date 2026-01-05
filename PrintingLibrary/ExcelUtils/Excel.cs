using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using MiniExcelLibs.Picture;
using PrintingLibrary.EncryptUtils;
using System.Runtime.InteropServices;

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

  public static void CreateOutputExcel(string outputFilePath, string templateFile, object data, string? encryptionPassword = null) {
    _logger.LogInformation("Creating file: {outputFile} \n", outputFilePath);

    var config = new OpenXmlConfiguration() {
      IgnoreTemplateParameterMissing = true,
      FillMergedCells = true,
      EnableWriteNullValueCell = true,
    };

    MiniExcel.SaveAsByTemplate(outputFilePath, templateFile, data, configuration: config);

    _logger.LogInformation("Excel file created: {outputFile} \n", outputFilePath);

    // Encrypt the file if password is provided
    if (!string.IsNullOrEmpty(encryptionPassword)) {
      try {
        EncryptUtil.EncryptFileInPlace(outputFilePath, encryptionPassword);
        _logger.LogInformation("Excel file encrypted: {outputFile} \n", outputFilePath);
      } catch (Exception ex) {
        _logger.LogError(ex, "Failed to encrypt Excel file: {outputFile}", outputFilePath);
        throw;
      }
    }
  }


  public static bool AddPrintStamp(string filePath, string? printStampImageName, string? printStampHash, string? hashSecret, string? encryptionPassword = null, string? stampRowIndex = null) {
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

    // Decrypt if password is provided
    if (!string.IsNullOrEmpty(encryptionPassword)) {
      EncryptUtil.DecryptFileInPlace(filePath, encryptionPassword);
    }

    try {
      var _stampRowIndex = stampRowIndex ?? $"A{GetLastDataRowIndex(filePath) + 2}";
      MiniExcel.AddPicture(filePath, new MiniExcelPicture {
        ImageBytes = File.ReadAllBytes(imageFile),
        PictureType = "image/png",
        CellAddress = _stampRowIndex,
        WidthPx = 300,
      });
    } finally {
      // Re-encrypt if password was provided
      if (!string.IsNullOrEmpty(encryptionPassword)) {
        EncryptUtil.EncryptFileInPlace(filePath, encryptionPassword);
      }
    }

    return true;
  }

  public static bool AddPrintStamp(
    string filePath,
    byte[]? imageBytes,
    int? width = null,
    int? height = null,
    string? encryptionPassword = null,
    string? stampRowIndex = null
  ) {
    if (imageBytes == null) return false;

    // Decrypt if password is provided
    if (!string.IsNullOrEmpty(encryptionPassword)) {
      EncryptUtil.DecryptFileInPlace(filePath, encryptionPassword);
    }

    try {
      var _stampRowIndex = stampRowIndex ?? $"A{GetLastDataRowIndex(filePath) + 2}";
      var picture = new MiniExcelPicture {
        ImageBytes = imageBytes,
        PictureType = "image/png",
        CellAddress = _stampRowIndex,
        // SheetName = "Sheet1",
      };
      if (width.HasValue) picture.WidthPx = width.Value;
      if (height.HasValue) picture.HeightPx = height.Value;

      MiniExcel.AddPicture(filePath, picture);
    } catch (Exception ex) {
      _logger.LogError(ex, "Failed to add print stamp to file: {filePath}", filePath);
    } finally {
      // Re-encrypt if password was provided
      if (!string.IsNullOrEmpty(encryptionPassword)) {
        EncryptUtil.EncryptFileInPlace(filePath, encryptionPassword);
      }
    }

    return true;
  }
}