using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using tda26.Server.Classes;
using tda26.Server.Services;

namespace tda26.Server.API;

[ApiController]
[Route("api/v2")]
public class APIv2(
    IAuthService auth
) : Controller {

    [HttpGet]
    public IActionResult Index() {
        return Ok(new {
            status = "ok",
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            message = "This is API version 2.",
        });
    }

    #if DEBUG
    [HttpGet("gpw")]
    public IActionResult GeneratePassword([FromQuery] string password) {
        var hashedPassword = Utilities.EncryptPassword(password);
        return Ok(new {
            password,
            hashedPassword
        });
    }
    #endif


    // auth
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct) {
        var acc = await auth.ReAuthAsync(ct);
        if (acc == null) return new UnauthorizedResult();

        var obj = JsonSerializer.SerializeToNode(acc, JsonSerializerOptions.Web);
        if(obj == null) return StatusCode(500, new { error = "Serialization error." });

        // odstraneni hesla
        obj.AsObject().Remove("password");

        return Ok(obj);
    }

    [HttpPost("auth/login")]
    public async Task<IActionResult> Login([FromBody] JsonNode body, CancellationToken ct) {
        var username = body["username"]?.GetValue<string>();
        var password = body["password"]?.GetValue<string>();

        if(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) {
            return new BadRequestObjectResult(new {
                message = "Username and password are required."
            });
        }

        var acc = await auth.LoginAsync(username, password, ct);
        if (acc == null) return new UnauthorizedObjectResult(new { message = "Invalid username or password." });

        var obj = JsonSerializer.SerializeToNode(acc, JsonSerializerOptions.Web);
        if(obj == null) return StatusCode(500, new { message = "Serialization error." });

        // odstraneni hesla
        obj.AsObject().Remove("password");

        return Ok(obj);
    }
}