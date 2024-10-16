
using Microsoft.AspNetCore.Mvc;

namespace PrintingApi.Controllers {

  [ApiController]
  [Route("[controller]")]
  public class OpenDrawerController : ControllerBase {

    [HttpGet(Name = "TestOpenDrawer")]
    public dynamic Get() => Ok("Open Drawer Api Works!");

    [HttpPost(Name = "PostOpenDrawer")]
    public dynamic Post(string printerName) {
      try {
        const string ESC = "\u001B";
        const string p = "\u0070";
        const string m = "\u0000";
        const string t1 = "\u0025";
        const string t2 = "\u0250";
        const string openTillCommand = ESC + p + m + t1 + t2;

        RawPrinterHelper.SendStringToPrinter(printerName, openTillCommand);
        return Ok();
      } catch (Exception e) {
        return StatusCode(StatusCodes.Status500InternalServerError, e);
      }
    }
  }
}