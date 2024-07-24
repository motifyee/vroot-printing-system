
using Microsoft.AspNetCore.Mvc;

namespace TodoApi.Controllers {

  [ApiController]
  [Route("[controller]")]
  public class OpenDrawerController : ControllerBase {


    public OpenDrawerController() { }

    [HttpGet(Name = "TestOpenDrawer")]
    public dynamic Get() {
      return Ok("Success");
    }

    [HttpPost(Name = "PostOpenDrawer")]
    public dynamic Post(string printerName) {
      const string ESC = "\u001B";
      const string p = "\u0070";
      const string m = "\u0000";
      const string t1 = "\u0025";
      const string t2 = "\u0250";
      const string openTillCommand = ESC + p + m + t1 + t2;

      RawPrinterHelper.SendStringToPrinter(printerName, openTillCommand);
      return Ok();
    }
  }
}