using Microsoft.AspNetCore.Http;
using Syncfusion.EJ2.Spreadsheet;
using Syncfusion.Licensing;

namespace PrintingLibrary.SyncfusionUtils;

// TODO: optionally include syncfusion binaries
public static class SyncfusionSpreadsheetUtils
{

  public static void RegisterLicence()
  {
    // Register Syncfusion license
    SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NHaF5cXmVCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH1dcHRRRWBeUkB+XktWYUo=");
  }

  public static string Open(IFormFile file)
  {
    if (file == null || file.Length == 0)
    {
      throw new ArgumentException("No file uploaded", nameof(file));
    }

    OpenRequest open = new()
    {
      File = file
    };

    return Workbook.Open(open);
  }
}
