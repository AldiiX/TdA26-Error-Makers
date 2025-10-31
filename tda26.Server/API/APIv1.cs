using Microsoft.AspNetCore.Mvc;
using tda26.Server.Services;

namespace tda26.Server.API;

[ApiController]
[Route("api/v1"), Route("api")]
public class APIv1(
    IDatabaseService db
) : Controller {

    [HttpGet]
    public IActionResult Index() {
        return Ok(new {
            status = "ok",
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            message = "This is API version 1.",
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

    [HttpGet("lecturers")]
    public async Task<IActionResult> GetLecturers() {
        await using var conn = await db.GetOpenConnectionAsync();
        if (conn == null)
            return StatusCode(500, new { error = "Database connection failed." });

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM lecturers";

        var lecturers = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (reader.Read()) {
            lecturers.Add(new {
                uuid = reader.GetGuid("uuid"),
                username = reader.GetString("username"),
            });
        }

        return Ok(lecturers);
    }
}