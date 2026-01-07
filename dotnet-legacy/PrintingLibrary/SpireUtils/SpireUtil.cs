#pragma warning disable CA1416 // Validate platform compatibility

using System.Drawing.Printing;
using System.Runtime.InteropServices;
using Spire.Pdf;

namespace PrintingLibrary.SpireUtils;

public static class SpireUtil
{

  public static void PrintPdf(byte[] data, string? password = null, string? printerName = null, int width = 315, int height = 10000)
  {
    var pdf = new PdfDocument();
    if (password != null)
      pdf.LoadFromBytes(data, password);
    else
      pdf.LoadFromBytes(data);

    if (printerName != null)
    {
      pdf.PrintSettings.PrinterName = printerName;
    }

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      pdf.PrintSettings.PrintController = new StandardPrintController();

      var paperSize = new PaperSize("Custom", width, height)
      {
        RawKind = (int)PaperKind.Custom
      };
      pdf.PrintSettings.PaperSize = paperSize;

      pdf.PrintSettings.SelectPageRange(1, 1);
      pdf.PrintSettings.SelectSinglePageLayout(Spire.Pdf.Print.PdfSinglePageScalingMode.FitSize, true);
    }

    pdf.Print();
  }

  public static void SavePdf(byte[] data, string filePath, string? password = null)
  {
    var pdf = new PdfDocument();
    if (password != null)
      pdf.LoadFromBytes(data, password);
    else
      pdf.LoadFromBytes(data);

    pdf.SaveToFile(filePath, FileFormat.PDF);
  }
}
