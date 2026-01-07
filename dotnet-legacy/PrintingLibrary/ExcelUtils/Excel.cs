using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using MiniExcelLibs.Picture;
using PrintingLibrary.EncryptUtils;

namespace PrintingLibrary.ExcelUtils;

public static class ExcelUtils {
  private static readonly ILogger _logger = NullLogger.Instance;

  public static int GetLastDataRowIndex(Stream file) {
    var rows = MiniExcel.Query(file).ToList();
    var lastRowIndex = 1;

    for (int i = 0; i < rows.Count; i++)
      foreach (var property in (IDictionary<string, object>)rows[i])
        if (property.Value != null) lastRowIndex = i + 1;

    return lastRowIndex;
  }

  public static Stream CreateOutputExcel(string templateFile, object data) {
    _logger.LogInformation("Creating file: {templateFile} \n", templateFile);

    var config = new OpenXmlConfiguration() {
      IgnoreTemplateParameterMissing = true,
      FillMergedCells = true,
      EnableWriteNullValueCell = true,
    };

    MemoryStream ms = new();
    ms.SaveAsByTemplate(templateFile, data, configuration: config);
    ms.Seek(0, SeekOrigin.Begin);

    return ms;
  }


  public static bool AddPrintStamp(Stream data, string? printStampImageName, string? printStampHash, string? hashSecret, string? stampRowIndex = null) {
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

    var _stampRowIndex = stampRowIndex ?? $"A{GetLastDataRowIndex(data) + 2}";
    MiniExcel.AddPicture(data, new MiniExcelPicture {
      ImageBytes = File.ReadAllBytes(imageFile),
      PictureType = "image/png",
      CellAddress = _stampRowIndex,
      WidthPx = 300,
    });

    return true;
  }

  public static bool AddPrintStamp(
    Stream fileStream,
    byte[]? imageBytes,
    int? width = null,
    int? height = null,
    string? stampRowIndex = null
  ) {
    if (imageBytes == null) return false;

    try {
      var _stampRowIndex = stampRowIndex ?? $"A{GetLastDataRowIndex(fileStream) + 2}";

      var picture = new MiniExcelPicture {
        ImageBytes = imageBytes,
        PictureType = "image/png",
        CellAddress = _stampRowIndex,
      };

      if (width.HasValue) picture.WidthPx = width.Value;
      if (height.HasValue) picture.HeightPx = height.Value;

      MiniExcel.AddPicture(fileStream, picture);
    } catch (Exception ex) {
      _logger.LogError(ex, "Failed to add print stamp to file: {filePath}", fileStream);
    }

    return true;
  }
}