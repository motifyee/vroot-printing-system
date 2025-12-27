using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using MiniExcelLibs.Picture;
using TemplatePrinting.Models.Invoice;

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

    var logoWatcher = System.Diagnostics.Stopwatch.StartNew();
    var logoAdded = AddLogo(outputFilePath, invoice.LogoImage ?? "print_stamp.png");
    logoWatcher.Stop();
    if (invoice.LogoImage != null || logoAdded)
      _logger.LogInformation("Time to add logo: {time} \n", logoWatcher.Elapsed);

    _logger.LogInformation("Excel file created: {outputFile} \n", outputFilePath);
  }

  // add logo image
  private bool AddLogo(string outputFilePath, string logoImage) {
    var logoRowIndex = GetLastDataRowIndex(outputFilePath) + 2;
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
      CellAddress = $"A{logoRowIndex}",
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
