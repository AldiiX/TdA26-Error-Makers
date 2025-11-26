using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using tda26.Server.Data.Models;
using tda26.Server.DTOs.Mapping;
using tda26.Server.DTOs.v1;
using tda26.Server.DTOs.v2;
using tda26.Server.Infrastructure;
using tda26.Server.Repositories;
using tda26.Server.Services;
using CreateCourseRequest = tda26.Server.DTOs.v2.CreateCourseRequest;

namespace tda26.Server.API;

[ApiController]
[Route("api/v2")]
public class APIv2(
    IAuthService auth,
    ILecturerRepository lecturers,
    ICourseRepository courseRepository,
    IAccountRepository accounts,
    IMaterialAccessService materialAccessService
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

    [HttpPost("auth/logout")]
    public async Task<IActionResult> Logout(CancellationToken ct) {
        await auth.LogoutAsync(ct);
        return Ok(new { message = "Logged out successfully." });
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
        var existingAccount = await accounts.GetByUsernameAsync(body.Username!, ct);
        
        if (existingAccount != null) {
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
        
        await accounts.CreateAsync(newLecturer, ct);
        
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
    
    // accounts
    
    [HttpGet("accounts/{uuid:guid}")]
    public async Task<IActionResult> GetAccount([FromRoute] Guid uuid, CancellationToken ct) {
        var account = await accounts.GetByIdAsync(uuid, ct);
        if (account == null) return new NotFoundObjectResult(new { message = "Account not found." });

        return account switch {
            Lecturer lecturer => Ok(lecturer),
            _ => Ok(account)
        };
    }
    
    //courses
    
    [HttpGet("courses")]
    public async Task<IActionResult> GetCourses() {
        var courses = await courseRepository.GetAllAsync();
        return Ok(courses);
    }
    
    [HttpGet("me/courses")]
    public async Task<IActionResult> GetMyCourses(
        [FromQuery] bool full = false,
        [FromQuery] int max = -1
        ) {
        var acc = await auth.ReAuthAsync();
        if (acc == null) return Unauthorized();
    
        if (full) {
            var courses = await courseRepository.GetByLecturerUuidAsyncFull(acc.Uuid, max);
            return Ok(courses);
        } else {
            var courses = await courseRepository.GetByLecturerUuidAsync(acc.Uuid, max);
            return Ok(courses);
        }
    }
    
    [HttpGet("courses/{uuid:guid}")]
    public async Task<IActionResult> GetCourseById(
        [FromRoute] Guid uuid,
        [FromQuery] bool full = true
        ) {
        Course? course;
        
        if (full) {
            course = await courseRepository.GetByUuidAsyncFull(uuid);
        } else {
            course = await courseRepository.GetByUuidAsync(uuid);
        }
        
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        var courses = course.ToReadDto();
        courses.Materials = courses.Materials.OrderByDescending(m => m.CreatedAt).ToList();
        
        return Ok(courses);
    }

    [HttpPut("courses/{uuid:guid}")]
    public async Task<IActionResult> UpdateCourse(Guid uuid, [FromBody] CreateCourseRequest body) {
        var acc = await auth.ReAuthAsync();
        if (acc == null) return Unauthorized();

        if (string.IsNullOrEmpty(body.Name) || string.IsNullOrEmpty(body.Description)) {
            return BadRequest(new { error = "Name and description are required." });
        }

        var existingCourse = await courseRepository.GetByUuidAsync(uuid);
        if (existingCourse == null) return NotFound();

        if (existingCourse.LecturerUuid != acc.Uuid) return Forbid();

        existingCourse.Name = body.Name;
        existingCourse.Description = body.Description;

        await courseRepository.UpdateAsync(existingCourse);

        return Ok(existingCourse);
    }
    
    [HttpPut("courses/{uuid:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateCourseWithMaterials(Guid uuid, [FromForm] UpdateCourseWithMaterialsRequest body) {
        var acc = await auth.ReAuthAsync();
        if (acc == null) return Unauthorized();

        if (string.IsNullOrEmpty(body.Course.Name) || string.IsNullOrEmpty(body.Course.Description)) {
            return BadRequest(new { error = "Course name and description are required." });
        }

        var existingCourse = await courseRepository.GetByUuidAsyncFull(uuid);
        if (existingCourse == null) return NotFound();

        if (existingCourse.LecturerUuid != acc.Uuid) return Forbid();

        existingCourse.Name = body.Course.Name;
        existingCourse.Description = body.Course.Description;

        foreach (var url in body.UrlMaterials) {
            if (string.IsNullOrEmpty(url.Url)) {
                return BadRequest(new { error = "URL is required for URL materials." });
            }

            var existing = existingCourse.Materials.FirstOrDefault(m => m.Uuid == url.Uuid);

            if (existing is UrlMaterial urlMat) {
                urlMat.Name = url.Name;
                urlMat.Description = url.Description;
                urlMat.Url = url.Url;
                urlMat.FaviconUrl = $"https://www.google.com/s2/favicons?domain={new Uri(url.Url).Host}&sz=64";
            } 
            else {
                existingCourse.Materials.Add(new UrlMaterial {
                    Name = url.Name,
                    Description = url.Description,
                    Type = Material.MaterialType.Url,
                    CourseUuid = existingCourse.Uuid,
                    Url = url.Url,
                    FaviconUrl = $"https://www.google.com/s2/favicons?domain={new Uri(url.Url).Host}&sz=64"
                });
            }
        }

        foreach (var file in body.FileMaterials) {
            var existing = existingCourse.Materials.FirstOrDefault(m => m.Uuid == file.Uuid);

            if (existing is FileMaterial existingFileMat) {
                existingFileMat.Name = file.Name;
                existingFileMat.Description = file.Description;

                if (file.File != null && file.File.Length > 0) {
                    var uploadedUrl = await materialAccessService.UploadFileMaterialAsync(existingCourse.Uuid, file.File);
                    existingFileMat.FileUrl = uploadedUrl;
                }
            }
            else {
                if (file.File == null || file.File.Length == 0) {
                    return BadRequest(new { error = "New file material must include a file." });
                }

                var uploadedUrl = await materialAccessService.UploadFileMaterialAsync(existingCourse.Uuid, file.File);

                existingCourse.Materials.Add(new FileMaterial {
                    Name = file.Name,
                    Description = file.Description,
                    Type = Material.MaterialType.File,
                    CourseUuid = existingCourse.Uuid,
                    FileUrl = uploadedUrl
                });
            }
        }

        var sentUuids = body.UrlMaterials.Select(m => m.Uuid)
            .Concat(body.FileMaterials.Select(m => m.Uuid))
            .Where(id => id.HasValue)
            .Select(id => id.Value)
            .ToHashSet();

        var toRemove = existingCourse.Materials
            .Where(m => !sentUuids.Contains(m.Uuid))
            .ToList();

        foreach (var material in toRemove) {
            existingCourse.Materials.Remove(material);
        }

        await courseRepository.UpdateAsync(existingCourse);

        return Ok(existingCourse.ToReadDto());
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
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest body) {
        var acc = await auth.ReAuthAsync();
        if (acc == null) return Unauthorized();
        
        if(string.IsNullOrEmpty(body.Name) || string.IsNullOrEmpty(body.Description)) {
            return BadRequest(new { error = "Name and description are required." });
        }

        var newCourse = new Course {
            Name = body.Name,
            Description = body.Description,
            LecturerUuid = acc.Uuid
        };

        await courseRepository.CreateAsync(newCourse);

        return new CreatedAtActionResult(
            actionName: nameof(GetCourseById),
            controllerName: "APIv2",
            routeValues: new { uuid = newCourse.Uuid },
            value: newCourse
        );
    }

    [HttpPost("courses")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateCourseWithMaterials([FromForm] CreateCourseWithMaterialsRequest body) {
        var acc = await auth.ReAuthAsync();
        if (acc == null) return Unauthorized();
        
        if(string.IsNullOrEmpty(body.Course.Name) || string.IsNullOrEmpty(body.Course.Description)) {
            return BadRequest(new { error = "Course name and description are required." });
        }
        
        var newCourse = new Course {
            Name = body.Course.Name,
            Description = body.Course.Description,
            LecturerUuid = acc.Uuid
        };
        
        foreach (var url in body.UrlMaterials) {
            if (string.IsNullOrEmpty(url.Url)) {
                return BadRequest(new { error = "URL is required for URL materials." });
            }

            var newMaterial = new UrlMaterial {
                Name = url.Name,
                Description = url.Description,
                Type = Material.MaterialType.Url,
                CourseUuid = newCourse.Uuid,
                Url = url.Url,
                FaviconUrl = $"https://www.google.com/s2/favicons?domain={new Uri(url.Url).Host}&sz=64"
            };
            
            newCourse.Materials.Add(newMaterial);
        }

        foreach (var file in body.FileMaterials) {
            if (file.File.Length == 0) {
                return BadRequest(new { error = "File is required for file materials." });
            }

            var uploadedUrl = await materialAccessService.UploadFileMaterialAsync(newCourse.Uuid, file.File);

            var newMaterial = new FileMaterial {
                Name = file.Name,
                Description = file.Description,
                Type = Material.MaterialType.File,
                CourseUuid = newCourse.Uuid,
                FileUrl = uploadedUrl
            };

            newCourse.Materials.Add(newMaterial);
        }
        
        await courseRepository.CreateAsync(newCourse);
        
        return CreatedAtAction(nameof(GetCourseById), new { uuid = newCourse.Uuid }, null);
    }

    [HttpGet("courses/{courseUuid:guid}/materials/{materialUuid:guid}")]
    public async Task<IActionResult> GetCourseMaterialById([FromRoute] Guid courseUuid, [FromRoute] Guid materialUuid)
    {
        var course = await courseRepository.GetByUuidAsyncFull(courseUuid);
        if (course == null)
            return NotFound(new { error = "Course not found." });

        var material = course.Materials.FirstOrDefault(m => m.Uuid == materialUuid);
        if (material == null)
            return NotFound(new { error = "Material not found." });

        switch (material)
        {
            case UrlMaterial urlMaterial:
                return Ok(urlMaterial.ToReadDto());
            case FileMaterial fileMaterial:
                try {
                    var memoryStream = await materialAccessService.DownloadFileMaterialAsync(fileMaterial.FileUrl);

                    var baseName = material.Name.Trim().ToLowerInvariant().Replace(" ", "-");

                    string extension;
                    try {
                        var uri = new Uri(fileMaterial.FileUrl, UriKind.RelativeOrAbsolute);
                        var path = uri.IsAbsoluteUri ? uri.AbsolutePath : fileMaterial.FileUrl;
                        extension = Path.GetExtension(path);
                    } catch {
                        extension = Path.GetExtension(fileMaterial.FileUrl);
                    }

                    if (string.IsNullOrEmpty(extension) && fileMaterial.FileUrl.Contains('.')) {
                        var idx = fileMaterial.FileUrl.LastIndexOf('.');
                        if (idx >= 0 && idx < fileMaterial.FileUrl.Length - 1)
                            extension = fileMaterial.FileUrl[idx..];
                    }

                    var fileName = string.IsNullOrEmpty(extension) ? baseName : $"{baseName}{extension}";
                    return File(memoryStream, "application/octet-stream", fileName);
                }
                catch (Minio.Exceptions.MinioException e)
                {
                    return StatusCode(500, new { error = "Error fetching file from storage.", detail = e.Message });
                }
            default:
                return StatusCode(500, new { error = "Unknown material type." });

        }
    }
}