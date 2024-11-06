using System.Globalization;

namespace TemplatePrinting.Controllers;

public partial class PrintInvoiceController {

  public string GetOutputFilePath(string? date, string? invoiceNo, string? invoiceType, string ext = "xlsx") {
    var culture = new CultureInfo("ar-EG");
    _ = DateTime.TryParse(date ?? DateTime.Now.ToString(), out DateTime _date);

    string year = _date.Year.ToString();
    string month = $"{_date.Month} — {_date.ToString("MMMM", culture)}";
    string day = $"{_date.Day} — {_date.ToString("dddd", culture)}";

    string fileName = $"{date ?? ""} #{invoiceNo} — {invoiceType} — {Guid.NewGuid()}.{ext}";

    string folderPath = Path.Combine(
           Environment.CurrentDirectory,
           "printer",
           "out",
           year,
           month,
           day,
           $"فاتورة — {invoiceNo}"
       );
    if (!Directory.Exists(folderPath)) {
      Directory.CreateDirectory(folderPath);
    }

    return Path.Combine(folderPath, fileName);
  }
}