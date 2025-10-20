using Microsoft.AspNetCore.Mvc;

namespace tda26.Server.API;

[ApiController]
[Route("api/v1")]
public class APIv1 : Controller {

    [HttpGet]
    public IActionResult Index() {
        return Ok(new {
            status = "ok",
            message = "API v1 is running",
            timestamp = DateTime.UtcNow
        });
    }
}