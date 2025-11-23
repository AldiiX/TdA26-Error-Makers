using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tda26.Server.Data;
using tda26.Server.Data.Models;
using tda26.Server.DTOs.v2;
using tda26.Server.Infrastructure;
using tda26.Server.Repositories;
using tda26.Server.Services;

namespace tda26.Server.API;

[ApiController]
[Route("api/v2")]
public class APIv2(
    IAuthService auth,
    ILecturerRepository lecturers,
    ICourseRepository courseRepository,
    AppDbContext db
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
        if (acc == null) return Unauthorized();

        return Ok(acc);
    }

    [HttpPost("auth/login")]
    public async Task<IActionResult> Login([FromBody] AuthLoginRequest body, CancellationToken ct) {
        if(string.IsNullOrEmpty(body.Username) || string.IsNullOrEmpty(body.Password)) {
            return new BadRequestObjectResult(new {
                message = "Username and password are required."
            });
        }

        var acc = await auth.LoginAsync(body.Username, body.Password, ct);
        if (acc == null) return new UnauthorizedObjectResult(new { message = "Invalid username or password." });

        return Ok(acc);
    }



    // lecturers
    [HttpGet("lecturers")]
    public async Task<IActionResult> GetLecturers(CancellationToken ct) {
        var all = await lecturers.GetAllAsync(ct);
        return new OkObjectResult(all);
    }

    #if DEBUG
    [HttpPost("lecturers")]
    public async Task<IActionResult> CreateLecturer([FromBody] CreateLecturerRequest body, CancellationToken ct) {
        var existingAccount = await db.Accounts
            .AnyAsync(a => a.Username == body.Username, ct);
        
        if (existingAccount) {
            return new ConflictObjectResult(new { message = "Username already exists." });
        }

        var newLecturer = new Lecturer {
            Username = body.Username!,
            Password = Utilities.EncryptPassword(body.Password!),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            TitleBefore = body.TitleBefore,
            FirstName = body.FirstName!,
            MiddleName = body.MiddleName,
            LastName = body.LastName!,
            TitleAfter = body.TitleAfter,
            Bio = body.Bio,
            PictureUrl = body.PictureUrl,
            Claim = body.Claim,
            Location = body.Location,
            PricePerHour = body.PricePerHour,
            MobileNumbers = body.MobileNumbers,
            Emails = body.Emails,
            Tags = body.Tags
        };
        
        db.Lecturers.Add(newLecturer);
        await db.SaveChangesAsync(ct);
        
        return new CreatedAtActionResult(
            actionName: nameof(GetLecturer),
            controllerName: "APIv2",
            routeValues: new { uuid = newLecturer.Uuid },
            value: newLecturer
        );
    }
    #endif


    [HttpGet("lecturers/{uuid:guid}")]
    public async Task<IActionResult> GetLecturer([FromRoute] Guid uuid, CancellationToken ct) {
        var lecturer = await lecturers.GetByIdAsync(uuid, ct);
        if (lecturer == null) return new NotFoundObjectResult(new { message = "Lecturer not found." });

        return new OkObjectResult(lecturer);
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

        if (!string.IsNullOrEmpty(name)) {
            course.Name = name;
        }

        if (!string.IsNullOrEmpty(description)) {
            course.Description = description;
        }

        await courseRepository.UpdateAsync(course);

        return Ok(course);
    }

    [HttpDelete("courses/{uuid:guid}")]
    public async Task<IActionResult> DeleteCourse([FromRoute] Guid uuid) {
        var success = await courseRepository.DeleteAsync(uuid);
        if (!success) {
            return NotFound(new { error = "Course not found." });
        }

        return NoContent();
    }
    
    [HttpPost("courses")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest body ) {
        if(string.IsNullOrEmpty(body.Name) || string.IsNullOrEmpty(body.Description)) {
            return BadRequest(new { error = "Name and description are required." });
        }

        var newCourse = new Course {
            Name = body.Name,
            Description = body.Description
        };

        await courseRepository.CreateAsync(newCourse);

        return new CreatedAtActionResult(
            actionName: nameof(GetCourseById),
            controllerName: "APIv2",
            routeValues: new { uuid = newCourse.Uuid },
            value: newCourse
        );
    }
}