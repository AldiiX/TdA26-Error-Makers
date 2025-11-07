using Microsoft.AspNetCore.Mvc;
using tda26.Server.Classes.Objects;
using tda26.Server.Repositories;
using tda26.Server.Services;

namespace tda26.Server.API;

[ApiController]
[Route("api/v1"), Route("api")]
public class APIv1(
    IDatabaseService db,
    ICourseRepository courseRepository
) : Controller {

    [HttpGet]
    public IActionResult Index() {
        return Ok(new {
            organization = "Student Cyber Games",
            // status = "ok",
            // timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            // message = "This is API version 1.",
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
    
    [HttpPost("courses")]
    public async Task<IActionResult> CreateCourse([FromBody] Course course) {
        var success = await courseRepository.CreateAsync(course);
        if (!success) {
            return StatusCode(500, new { error = "Failed to create course." });
        }
        return new ObjectResult(course) { StatusCode = 201 };
    }
}