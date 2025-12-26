using Microsoft.AspNetCore.Mvc;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Text;
using TemplatePrinting.Models;
using System.Management;

namespace TemplatePrinting.Controllers;

public partial class PrintInvoiceController {

  [HttpGet("Printers")]
  public ActionResult GetPrinters() {
    var path = Path.Combine(Environment.CurrentDirectory, "Views", "Printers", "index.html");
    if (!System.IO.File.Exists(path)) return NotFound("View not found");

    var html = System.IO.File.ReadAllText(path);
    return Content(html, "text/html", Encoding.UTF8);
  }

  [HttpGet("Printers/Data")]
  public ActionResult GetPrintersData() {
    List<PrinterInfo> printers = [];

    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      return Ok(printers);

#pragma warning disable CA1416 // Validate platform compatibility
    try {
      // Use WMI for detailed technical info
      var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");
      var wmiPrinters = searcher.Get().Cast<ManagementObject>().ToDictionary(
          x => x["Name"]?.ToString() ?? "",
          x => x
      );

      foreach (string printerName in PrinterSettings.InstalledPrinters) {
        var settings = new PrinterSettings { PrinterName = printerName };
        var info = new PrinterInfo {
          Name = printerName,
          IsDefault = settings.IsDefaultPrinter,
          IsValid = settings.IsValid,
          SupportsColor = settings.SupportsColor,
          MaxCopies = settings.MaximumCopies,
          Duplex = settings.Duplex.ToString(),
          IsPlotter = settings.IsPlotter,
          PaperSizes = settings.PaperSizes.Cast<PaperSize>().Select(x => x.PaperName).ToList(),
          Resolutions = settings.PrinterResolutions.Cast<PrinterResolution>().Select(x => x.ToString()).ToList()
        };

        if (wmiPrinters.TryGetValue(printerName, out var mo)) {
          info.FullName = mo["Caption"]?.ToString() ?? printerName;
          info.PortName = mo["PortName"]?.ToString() ?? "";
          info.DriverName = mo["DriverName"]?.ToString() ?? "";
          info.Location = mo["Location"]?.ToString() ?? "";
          info.IsShared = (bool)(mo["Shared"] ?? false);
          info.ShareName = mo["ShareName"]?.ToString() ?? "";
          info.IsNetwork = (bool)(mo["Network"] ?? false);
          info.IsOffline = (bool)(mo["WorkOffline"] ?? false);
          info.Status = mo["Status"]?.ToString() ?? "";
          // info.JobCount is usually not available simple like this in WMI Win32_Printer directly without Win32_PrintJob
        }

        printers.Add(info);
      }
    } catch (Exception ex) {
      _logger.LogError(ex, "Error fetching printer details");
    }

    return Ok(printers);
  }

  [HttpGet("Printers/Dummy")]
  public ActionResult GetDummyPrintersData() {
    List<PrinterInfo> printers = [
        new PrinterInfo {
            Name = "HP LaserJet Pro M404n (Office)",
            FullName = "HP LaserJet Pro M404n [Office_Floor_1]",
            IsDefault = true,
            IsValid = true,
            IsOffline = false,
            IsNetwork = true,
            IsShared = true,
            ShareName = "OFFICE_HP_M404",
            DriverName = "HP PCL 6 V4 Driver",
            PortName = "192.168.1.50",
            Location = "First Floor - Room 102",
            JobCount = 2,
            Status = "OK",
            SupportsColor = false,
            MaxCopies = 99,
            Duplex = "Simplex",
            IsPlotter = false,
            PaperSizes = ["A4", "Letter", "Legal", "Executive", "A5", "A6"],
            Resolutions = ["600x600dpi", "1200x1200dpi"]
        },
        new PrinterInfo {
            Name = "Epson EcoTank L3250 (Home)",
            FullName = "Epson L3250 Series (USB)",
            IsDefault = false,
            IsValid = true,
            IsOffline = false,
            IsNetwork = false,
            IsShared = false,
            DriverName = "Epson ESC/P-R V4",
            PortName = "USB001",
            Location = "Home Office",
            JobCount = 0,
            Status = "OK",
            SupportsColor = true,
            MaxCopies = 99,
            Duplex = "Vertical",
            IsPlotter = false,
            PaperSizes = ["A4", "B5", "A6", "4x6in", "5x7in", "Envelope"],
            Resolutions = ["300x300dpi", "600x600dpi", "1200x2400dpi"]
        },
        new PrinterInfo {
            Name = "Canon iPF770 Plotter (Design)",
            FullName = "Canon imagePROGRAF iPF770",
            IsDefault = false,
            IsValid = false,
            IsOffline = true,
            IsNetwork = true,
            IsShared = true,
            ShareName = "PLOTTER_CANON",
            DriverName = "Canon iPF770 Driver",
            PortName = "WSD-Port",
            Location = "Design Studio",
            JobCount = 5,
            Status = "Error",
            SupportsColor = true,
            MaxCopies = 1,
            Duplex = "None",
            IsPlotter = true,
            PaperSizes = ["A0", "A1", "A2", "A3", "B0", "B1", "B2"],
            Resolutions = ["1200dpi", "2400dpi"]
        }
    ];

    return Ok(printers);
  }
}
