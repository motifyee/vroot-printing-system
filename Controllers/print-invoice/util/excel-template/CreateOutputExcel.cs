using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using MiniExcelLibs.Picture;
using TemplatePrinting.Models.Invoice;

namespace TemplatePrinting.Controllers;

public partial class PrintInvoiceController {


  private void CreateOutputExcel(string outputFilePath, string templateFile, object data) {
    _logger.LogInformation("Creating file: {outputFile} \n", outputFilePath);

    var config = new OpenXmlConfiguration() {
      IgnoreTemplateParameterMissing = true,
      FillMergedCells = true,
      EnableWriteNullValueCell = true,
    };

    MiniExcel.SaveAsByTemplate(outputFilePath, templateFile, data, configuration: config);

    _logger.LogInformation("Excel file created: {outputFile} \n", outputFilePath);
  }

  // add logo image
  private bool AddLogo(string outputFilePath, string logoImage, string? cellAddress = null) {
    var _cellAddress = cellAddress ?? $"A{GetLastDataRowIndex(outputFilePath) + 2}";
    string imageFile = Path.Combine(
                Environment.CurrentDirectory,
                "printer",
                "images",
                logoImage
            );

    if (!System.IO.File.Exists(imageFile)) {
      _logger.LogError("Logo image not found: {imageFile}", imageFile);
      return false;
    }

    MiniExcel.AddPicture(outputFilePath, new MiniExcelPicture {
      ImageBytes = System.IO.File.ReadAllBytes(imageFile),
      PictureType = "image/png",
      CellAddress = _cellAddress,
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
}
