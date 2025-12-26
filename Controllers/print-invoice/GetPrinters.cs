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

    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
      var errorJson = System.Text.Json.JsonSerializer.Serialize(new { error = "Printer information collection is only supported on Windows systems." });
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
        var json = System.Text.Json.JsonSerializer.Serialize(printers);
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
      var initialJson = System.Text.Json.JsonSerializer.Serialize(initialPrinters);
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
