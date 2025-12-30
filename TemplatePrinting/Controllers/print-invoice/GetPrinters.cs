using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using TemplatePrinting.Models;

namespace TemplatePrinting.Controllers;

public partial class PrintInvoiceController {

  [HttpGet("Printers/Data")]
  public async Task GetPrintersData() {
    Response.Headers.Append("Content-Type", "text/event-stream");
    Response.Headers.Append("Cache-Control", "no-cache");
    Response.Headers.Append("Connection", "keep-alive");

    var jsonOptions = new System.Text.Json.JsonSerializerOptions {
      PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };

    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
      var errorJson = System.Text.Json.JsonSerializer.Serialize(new { error = "Printer information collection is only supported on Windows systems." }, jsonOptions);
      await Response.WriteAsync($"data: {errorJson}\n\n");
      await Response.Body.FlushAsync();
      return;
    }

    var printerService = HttpContext.RequestServices.GetService<PrinterBackgroundService>();
    if (printerService == null) {
      Response.StatusCode = 500;
      await Response.WriteAsync("Printer service not available");
      return;
    }

    var tcs = new TaskCompletionSource<bool>();
    var semaphore = new SemaphoreSlim(1, 1);

    Action<List<PrinterInfo>> onPrintersChanged = async (printers) => {
      await semaphore.WaitAsync();
      try {
        var json = System.Text.Json.JsonSerializer.Serialize(printers, jsonOptions);
        await Response.WriteAsync($"data: {json}\n\n");
        await Response.Body.FlushAsync();
      } catch {
        tcs.TrySetResult(true);
      } finally {
        semaphore.Release();
      }
    };

    // Subscribe and get initial data atomicaly relative to background updates
    var initialPrinters = printerService.Subscribe(onPrintersChanged);

    await semaphore.WaitAsync();
    try {
      var initialJson = System.Text.Json.JsonSerializer.Serialize(initialPrinters, jsonOptions);
      await Response.WriteAsync($"data: {initialJson}\n\n");
      await Response.Body.FlushAsync();
    } finally {
      semaphore.Release();
    }

    try {
      await tcs.Task.WaitAsync(HttpContext.RequestAborted);
    } catch (OperationCanceledException) {
      // Client disconnected
    } finally {
      printerService.Unsubscribe(onPrintersChanged);
    }
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
            PaperSizes = [
                new PaperSizeInfo { Name = "A4", Width = 827, Height = 1169, Kind = "A4", RawKind = 9 },
                new PaperSizeInfo { Name = "Letter", Width = 850, Height = 1100, Kind = "Letter", RawKind = 1 },
                new PaperSizeInfo { Name = "Legal", Width = 850, Height = 1400, Kind = "Legal", RawKind = 5 }
            ],
            DefaultPaperSize = new PaperSizeInfo { Name = "A4", Width = 827, Height = 1169, Kind = "A4", RawKind = 9 },
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
            PaperSizes = [
                new PaperSizeInfo { Name = "A4", Width = 827, Height = 1169, Kind = "A4", RawKind = 9 },
                new PaperSizeInfo { Name = "4x6in", Width = 400, Height = 600, Kind = "Custom", RawKind = 256 }
            ],
            DefaultPaperSize = new PaperSizeInfo { Name = "A4", Width = 827, Height = 1169, Kind = "A4", RawKind = 9 },
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
            PaperSizes = [
                new PaperSizeInfo { Name = "A0", Width = 3311, Height = 4681, Kind = "Custom", RawKind = 256 },
                new PaperSizeInfo { Name = "A1", Width = 2339, Height = 3311, Kind = "Custom", RawKind = 256 }
            ],
            DefaultPaperSize = new PaperSizeInfo { Name = "A0", Width = 3311, Height = 4681, Kind = "Custom", RawKind = 256 },
            Resolutions = ["1200dpi", "2400dpi"]
        }
    ];

    return Ok(printers);
  }
}
