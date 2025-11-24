using Microsoft.AspNetCore.Mvc;
using tda26.Server.Data;
using tda26.Server.Data.Models;
using tda26.Server.DTOs.Mapping;
using tda26.Server.DTOs.v1;
using tda26.Server.Repositories;
using CreateCourseRequest = tda26.Server.DTOs.v1.CreateCourseRequest;

namespace tda26.Server.API;

[ApiController]
[Route("api/v1"), Route("api")]
public class APIv1(
    ICourseRepository courseRepository,
    IMaterialRepository materialRepository
) : Controller {

    [HttpGet]
    public IActionResult Index() {
        return Ok(new {
            organization = "Student Cyber Games",
        });
    }

    [HttpGet("courses")]
    public async Task<IActionResult> GetCourses() {
        var courses = await courseRepository.GetAllAsyncFull();
        
        var obj = courses.Select(course => course.ToReadDto());
        
        return Ok(obj);
    }
    
    [HttpGet("courses/{uuid:guid}")]
    public async Task<IActionResult> GetCourseById([FromRoute] Guid uuid) {
        var course = await courseRepository.GetByIdAsyncFull(uuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }
        
        return Ok(course.ToReadDto());
    }

    [HttpPut("courses/{uuid:guid}")]
    public async Task<IActionResult> EditCourse([FromRoute] Guid uuid, [FromBody] UpdateCourseRequest body) {
        var course = await courseRepository.GetByIdAsyncFull(uuid);
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
        
        return Ok(course.ToReadDto());
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
        
        return CreatedAtAction(nameof(GetCourseById), new { uuid = newCourse.Uuid }, newCourse.ToReadDto());
    }

    [HttpPost("courses/{uuid:guid}/materials")]
    [Consumes("application/json")]
    public async Task<IActionResult> AddMaterialToCourse([FromRoute] Guid uuid, [FromBody] CreateUrlMaterialRequest body) {
        if (body.Type != "url") {
            return BadRequest(new { error = "Only 'url' material type is supported in this endpoint." });
        }
        
        var course = await courseRepository.GetByIdAsync(uuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        if (string.IsNullOrEmpty(body.Name) || string.IsNullOrEmpty(body.Url)) {
            return BadRequest(new { error = "Name and URL are required." });
        }

        var newMaterial = new UrlMaterial {
            Name = body.Name,
            Description = body.Description,
            Url = body.Url,
            FaviconUrl = $"https://www.google.com/s2/favicons?domain={new Uri(body.Url).Host}&sz=64",
            Type = Material.MaterialType.Url,
            CourseUuid = course.Uuid
        };

        await materialRepository.AddMaterialAsync(course.Uuid, newMaterial);

        var obj = new {
            uuid = newMaterial.Uuid,
            name = newMaterial.Name,
            description = newMaterial.Description,
            url = newMaterial.Url,
            faviconUrl = newMaterial.FaviconUrl,
            type = newMaterial.Type,
            createdAt = newMaterial.CreatedAt,
            updatedAt = newMaterial.UpdatedAt
        };
        
        return CreatedAtAction(nameof(GetCourseById), new { uuid = course.Uuid }, obj);
    }

    [HttpGet("courses/{uuid:guid}/materials")]
    public async Task<IActionResult> GetMaterialsByCourseId([FromRoute] Guid uuid) {
        var course = await courseRepository.GetByIdAsync(uuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }
        var materials = await materialRepository.GetMaterialsByCourseIdAsync(course.Uuid);

        var obj = materials.Select(material => new {
            uuid = material.Uuid,
            name = material.Name,
            description = material.Description,
            type = material.Type,
            createdAt = material.CreatedAt,
            updatedAt = material.UpdatedAt,
            url = material is UrlMaterial urlMaterial ? urlMaterial.Url : null,
            faviconUrl = material is UrlMaterial urlMaterial2 ? urlMaterial2.FaviconUrl : null,
        });

        return Ok(obj);
    }
}