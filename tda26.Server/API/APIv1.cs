using Microsoft.AspNetCore.Mvc;

namespace tda26.Server.API;

[ApiController]
[Route("api/v1")]
public class APIv1 : Controller {

    [HttpGet()]
    public IActionResult Index() {
        return new JsonResult(new
        {
            organization = "",
        });
    }
}