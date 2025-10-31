using Microsoft.AspNetCore.Mvc;

namespace tda26.Server.API;

[ApiController]
[Route("api/v2")]
public class APIv2 : Controller {

    [HttpGet]
    public IActionResult Index() {
        return Ok(new {
            status = "ok",
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            message = "This is API version 2.",
        });
    }

    [HttpGet("status")]
    public IActionResult GetStatus() {
        return Ok(new
        {
            status = "ok",
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        });
    }
}