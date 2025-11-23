using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using tda26.Server.DTOs.v1;
using tda26.Server.DTOs.v2;
using tda26.Server.Repositories;
using tda26.Server.Services;
using CreateCourseRequest = tda26.Server.DTOs.v1.CreateCourseRequest;

namespace tda26.Server.API;

[ApiController]
[Route("api/v1"), Route("api")]
public class APIv1(
    ICourseRepository courseRepository
) : Controller {

    [HttpGet]
    public IActionResult Index() {
        return Ok(new {
            organization = "Student Cyber Games",
        });
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
            materials = course.Materials,
            quizzes = course.Quizzes,
            feed = course.Feed,
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
            materials = course.Materials,
            quizzes = course.Quizzes,
            feed = course.Feed,
        };
        
        return Ok(obj);
    }

    [HttpPut("courses/{uuid:guid}")]
    public async Task<IActionResult> EditCourse([FromRoute] Guid uuid, [FromBody] UpdateCourseRequest body) {
        var course = await courseRepository.GetByIdAsync(uuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        if (!string.IsNullOrEmpty(body.Name)) {
            course.Name = body.Name;
        }

        if (!string.IsNullOrEmpty(body.Description)) {
            course.Description = body.Description;
        }

        await courseRepository.UpdateAsync(course);

        var obj = new {
            uuid = course.Uuid,
            name = course.Name,
            description = course.Description,
            createdAt = course.CreatedAt,
            updatedAt = course.UpdatedAt,
            materials = course.Materials,
            quizzes = course.Quizzes,
            feed = course.Feed
        };
        
        return Ok(obj);
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
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest body ) {
        if (string.IsNullOrEmpty(body.Name) || string.IsNullOrEmpty(body.Description)) {
            return BadRequest(new { error = "Name and description are required." });
        }

        var newCourse = new Data.Models.Course {
            Name = body.Name,
            Description = body.Description,
        };

        await courseRepository.CreateAsync(newCourse);

        var obj = new {
            uuid = newCourse.Uuid,
            name = newCourse.Name,
            description = newCourse.Description,
            createdAt = newCourse.CreatedAt,
            updatedAt = newCourse.UpdatedAt,
            materials = newCourse.Materials,
            quizzes = newCourse.Quizzes,
            feed = newCourse.Feed
        };
        
        return CreatedAtAction(nameof(GetCourseById), new { uuid = newCourse.Uuid }, obj);
    }
}