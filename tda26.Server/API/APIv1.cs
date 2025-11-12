using System.Text.Json.Nodes;
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
        
        var obj = courses.Select(course => new {
            uuid = course.Uuid,
            name = course.Name,
            description = course.Description,
            createdAt = course.CreatedAt,
            updatedAt = course.UpdatedAt,
        });
        
        return Ok(obj);
    }
    
    [HttpGet("courses/{uuid:guid}")]
    public async Task<IActionResult> GetCourseById([FromRoute] Guid uuid) {
        var course = await courseRepository.GetByIdAsync(uuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }
        var obj = new {
            uuid = course.Uuid,
            name = course.Name,
            description = course.Description,
            createdAt = course.CreatedAt,
            updatedAt = course.UpdatedAt,
        };
        
        return Ok(obj);
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

        var obj = new {
            uuid = updatedCourse.Uuid,
            name = updatedCourse.Name,
            description = updatedCourse.Description,
            createdAt = updatedCourse.CreatedAt,
            updatedAt = updatedCourse.UpdatedAt,
        };

        return new OkObjectResult(obj);
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

        var obj = new {
            uuid = course.Uuid,
            name = course.Name,
            description = course.Description,
            createdAt = course.CreatedAt,
            updatedAt = course.UpdatedAt,
        };
        
        return new JsonResult(obj) { StatusCode = 201 };
    }
}