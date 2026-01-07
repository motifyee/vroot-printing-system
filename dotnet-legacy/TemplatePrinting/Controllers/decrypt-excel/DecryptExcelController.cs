using Microsoft.AspNetCore.Mvc;
using PrintingLibrary.EncryptUtils;
using PrintingLibrary.Setup;
using PrintingLibrary.SyncfusionUtils;


namespace TemplatePrinting.Controllers;

[ApiController]
[Route("decrypt-excel")]
public class DecryptExcelController(
    ILogger<DecryptExcelController> logger,
    IPrintingSetup util
) : ControllerBase
{
  private readonly ILogger<DecryptExcelController> _logger = logger;
  private readonly IPrintingSetup _util = util;

  [HttpGet("")]
  public IActionResult Index()
  {
    var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "decrypt-excel", "index.html");
    if (!System.IO.File.Exists(htmlPath))
    {
      return NotFound("Decryption page not found");
    }
    var html = System.IO.File.ReadAllText(htmlPath);
    return Content(html, "text/html");
  }

  [HttpPost("decrypt")]
  public async Task<IActionResult> DecryptFile([FromForm] IFormFile file, [FromForm] string password)
  {
    if (file == null || file.Length == 0)
    {
      return BadRequest(new { error = "No file uploaded" });
    }

    if (string.IsNullOrEmpty(password))
    {
      return BadRequest(new { error = "Password is required" });
    }

    try
    {
      // Create temp files for processing
      var tempInputPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
      var tempOutputPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

      try
      {
        // Save uploaded file to temp location
        using (var stream = new FileStream(tempInputPath, FileMode.Create))
        {
          await file.CopyToAsync(stream);
        }

        // Decrypt the file
        EncryptUtil.DecryptFile(tempInputPath, tempOutputPath, password);

        // Read decrypted file
        var decryptedBytes = await System.IO.File.ReadAllBytesAsync(tempOutputPath);

        // Clean up temp files
        System.IO.File.Delete(tempInputPath);
        System.IO.File.Delete(tempOutputPath);

        // Get original filename without extension and add .xlsx
        var originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
        var decryptedFileName = $"{originalFileName}_decrypted.xlsx";

        _logger.LogInformation("Successfully decrypted file: {FileName}", file.FileName);

        // Return decrypted file
        return File(decryptedBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", decryptedFileName);

      }
      catch
      {
        // Clean up temp files on error
        if (System.IO.File.Exists(tempInputPath))
          System.IO.File.Delete(tempInputPath);
        if (System.IO.File.Exists(tempOutputPath))
          System.IO.File.Delete(tempOutputPath);
        throw;
      }

    }
    catch (System.Security.Cryptography.CryptographicException)
    {
      _logger.LogWarning("Failed to decrypt file - incorrect password or corrupted file");
      return BadRequest(new { error = "Decryption failed. The password may be incorrect or the file may be corrupted." });
    }
    catch (InvalidDataException ex)
    {
      _logger.LogWarning("Invalid encrypted file format: {Message}", ex.Message);
      return BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error decrypting file");
      return StatusCode(500, new { error = "An error occurred while decrypting the file" });
    }
  }

  [HttpPost("open")]
  public IActionResult Open([FromForm] IFormFile file)
  {
    if (file == null || file.Length == 0)
    {
      return BadRequest(new { error = "No file uploaded" });
    }

    try
    {
      // Processing the Excel file and return the workbook JSON.
      var result = SyncfusionSpreadsheetUtils.Open(file);
      return Content(result);

    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error processing Excel file for Syncfusion Spreadsheet");
      return StatusCode(500, new { error = "An error occurred while processing the Excel file" });
    }
  }
}
