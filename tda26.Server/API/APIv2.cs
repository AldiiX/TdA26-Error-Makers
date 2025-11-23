using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using tda26.Server.Classes;
using tda26.Server.Repositories;
using tda26.Server.Services;

namespace tda26.Server.API;

[ApiController]
[Route("api/v2")]
public class APIv2(
    IAuthService auth,
    ILecturerRepository lecturers,
    ICourseRepository courseRepository
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

    [HttpPost("auth/logout")]
    public async Task<IActionResult> Logout(CancellationToken ct) {
        await auth.ReAuthAsync(ct);
        return Ok(new { message = "Logged out successfully." });
    }
    

    // lecturers
    [HttpGet("lecturers")]
    public async Task<IActionResult> GetLecturers(CancellationToken ct) {
        var all = await lecturers.GetAllAsync(ct);
        var arr = new JsonArray();

        foreach (var l in all.Select(lecturer => lecturer.ToJsonNode())) {
            arr.Add(l);
        }

        return new OkObjectResult(arr);
    }
    
    [HttpGet("lecturers/{uuid:guid}")]
    public async Task<IActionResult> GetLecturer([FromRoute] Guid uuid, CancellationToken ct) {
        var lecturer = await lecturers.GetByIdAsync(uuid, ct);
        if (lecturer == null) return new NotFoundObjectResult(new { message = "Lecturer not found." });

        return new OkObjectResult(lecturer.ToJsonNode());
    }
    
    //courses
    
    [HttpGet("courses")]
    public async Task<IActionResult> GetCourses() {
        var courses = await courseRepository.GetAllAsync();
        return Ok(courses);
    }
    
    [HttpGet("courses/{uuid:guid}")]
    public async Task<IActionResult> GetCourseById([FromRoute] Guid uuid) {
        var course = await courseRepository.GetByIdAsync(uuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }
        return Ok(course);
    }

    [HttpPut("courses/{uuid:guid}")]
    public async Task<IActionResult> EditCourse([FromRoute] Guid uuid, [FromBody] JsonNode body) {
        var course = await courseRepository.GetByIdAsync(uuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        var name = body["name"]?.GetValue<string>();
        var description = body["description"]?.GetValue<string>();

        if(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description)) {
            return BadRequest(new { error = "Name and description are required." });
        }

        var updatedCourse = await courseRepository.UpdateAsync(uuid, name, description);
        if (updatedCourse is null) {
            return StatusCode(500, new { error = "Failed to update course." });
        }

        return new OkObjectResult(updatedCourse);
    }

    [HttpDelete("courses/{uuid:guid}")]
    public async Task<IActionResult> DeleteCourse([FromRoute] Guid uuid) {
        var course = await courseRepository.GetByIdAsync(uuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        var success = await courseRepository.DeleteAsync(uuid);
        if (!success) {
            return StatusCode(500, new { error = "Failed to delete course." });
        }

        return NoContent();
    }
    
    [HttpPost("courses")]
    public async Task<IActionResult> CreateCourse([FromBody] JsonNode body ) {
        var name = body["name"]?.GetValue<string>();
        var description = body["description"]?.GetValue<string>();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description)) {
            return BadRequest(new { error = "Name and description are required." });
        }

        var course = await courseRepository.CreateAsync(name, description);
        if (course == null) {
            return StatusCode(500, new { error = "Failed to create course." });
        }

        return new JsonResult(course) { StatusCode = 201 };

    }
}