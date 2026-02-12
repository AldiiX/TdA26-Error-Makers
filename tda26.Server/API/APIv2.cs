using System.Net;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using tda26.Server.Controllers;
using tda26.Server.Data;
using tda26.Server.Data.Models;
using tda26.Server.DTOs.Mapping;
using tda26.Server.DTOs.v1;
using tda26.Server.DTOs.v2;
using tda26.Server.Infrastructure;
using tda26.Server.Services;
using CreateCourseRequest = tda26.Server.DTOs.v2.CreateCourseRequest;

namespace tda26.Server.API;

[ApiController]
[Route("api/v2")]
public class APIv2(
    IAuthService auth,
    IMaterialAccessService materialAccessService,
    AppDbContext db,
    IFeedStreamBroker fsb
) : Controller
{

    // random picovinky
    private static readonly HttpClient HttpClient = new();
    public static readonly Dictionary<Guid, List<IPAddress>> UsedIPsForCourse = new();



    [HttpGet]
    public IActionResult Index()
    {
        return Ok(new
        {
            status = "ok",
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            message = "Toto je API verze 2.",
        });
    }

#if DEBUG
    [HttpGet("gpw")]
    public IActionResult GeneratePassword([FromQuery] string password)
    {
        var hashedPassword = Utilities.EncryptPassword(password);
        return Ok(new
        {
            password,
            hashedPassword
        });
    }
#endif


    // auth
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct = default)
    {
        var acc = await auth.ReAuthAsync(ct);
        if (acc == null) return Unauthorized();

        // odstraneni policek
        foreach (var like in acc?.Ratings ?? [])
        {
            like.Account = null;
            like.Course.Account = null;
        }

        return Ok(acc);
    }

    [HttpPost("auth/login")]
    public async Task<IActionResult> Login([FromBody] AuthLoginRequest body, CancellationToken ct = default)
    {
        body.Username = body.Username?.Trim() ?? string.Empty;
        body.Password = body.Password?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(body.Username) || string.IsNullOrEmpty(body.Password))
        {
            return new BadRequestObjectResult(new
            {
                message = "Uživatelské jméno a heslo jsou povinné."
            });
        }

        var acc = await auth.LoginAsync(body.Username, body.Password, ct);
        
        if (acc == null) return new UnauthorizedObjectResult(new { message = "Neplatné uživatelské jméno nebo heslo." });
        
        // odstraneni policek
        foreach (var like in acc?.Ratings ?? [])
        {
            like.Account = null;
            like.Course.Account = null;
        }

        return Ok(acc);
    }

    [HttpPost("auth/logout")]
    public async Task<IActionResult> Logout(CancellationToken ct = default)
    {
        await auth.LogoutAsync(ct);
        return Ok(new { message = "Úspěšně odhlášen." });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRegisterRequest body, CancellationToken ct = default)
    {
        body.Username = body.Username?.Trim() ?? string.Empty;
        body.Email = body.Email?.Trim() ?? string.Empty;
        body.Password = body.Password?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(body.Username) || string.IsNullOrEmpty(body.Password))
        {
            return new BadRequestObjectResult(new
            {
                message = "Uživatelské jméno a heslo jsou povinné."
            });
        }

        var acc = await auth.RegisterAsync(body.Username, body.Email, body.Password, ct);

        if (acc == null)
        {
            return new ConflictObjectResult(new
            {
                message = "Uživatelské jméno už existuje."
            });
        }

        foreach (var like in acc?.Ratings ?? [])
        {
            like.Account = null;
            like.Course.Account = null;
        }

        return new CreatedAtActionResult(
            actionName: nameof(GetAccount),
            controllerName: "APIv2",
            routeValues: new { uuid = acc.Uuid },
            value: acc
        );
    }



    #region lecturers
    // lecturers
    [HttpGet("lecturers")]
    public async Task<IActionResult> GetLecturers([FromQuery] uint limit = 0, CancellationToken ct = default) {
        var isLimited = limit > 0;

        var all = await db.Lecturers
            .OrderBy(l => l.CreatedAt)
            .Take(isLimited ? (int) limit : int.MaxValue)
            .AsNoTracking()
            .ToListAsync(ct);
        return new OkObjectResult(all);
    }

    #if DEBUG
    [HttpPost("lecturers")]
    public async Task<IActionResult> CreateLecturer([FromBody] CreateLecturerRequest body, CancellationToken ct = default) {
        var existingAccount = await db.Accounts
            .FirstOrDefaultAsync(a => a.Username == body.Username!, ct);
        
        if (existingAccount != null) {
            return new ConflictObjectResult(new { message = "Uživatelské jméno už existuje." });
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
        
        db.Accounts.Add(newLecturer);
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
    public async Task<IActionResult> GetLecturer([FromRoute] Guid uuid, CancellationToken ct = default) {
        var lecturer = await db.Lecturers
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Uuid == uuid, ct);
        if (lecturer == null) return new NotFoundObjectResult(new { message = "Lektor nenalezen." });

        return new OkObjectResult(lecturer);
    }

    #endregion





    #region accounts
    
    [HttpGet("accounts/{uuid:guid}")]
    public async Task<IActionResult> GetAccount([FromRoute] Guid uuid, CancellationToken ct = default) {
        var account = await db.Accounts
            .Include(a => a.Ratings)
            .ThenInclude(l => l.Course)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Uuid == uuid, ct);
        if (account == null) return new NotFoundObjectResult(new { message = "Účet nenalezen." });

        // odstraneni policek
        foreach (var like in account?.Ratings ?? []) {
            like.Account = null;
            like.Course.Account = null;
        }

        return account switch {
            Lecturer lecturer => Ok(lecturer),
            _ => Ok(account)
        };
    }

    #endregion





    #region courses
    
    [HttpGet("courses")]
    public async Task<IActionResult> GetCourses([FromQuery] uint limit = 0, CancellationToken ct = default) {
        var isLimited = limit > 0;

        var courses = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Account)
            .Include(c => c.Category)
            .OrderByDescending(c => c.CreatedAt)
            .Take(isLimited ? (int) limit : int.MaxValue)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);

        foreach (var c in courses) {
            c.Materials = [];
            c.Quizzes = [];
            c.Feed = [];
            if(c.Account != null) c.Account.Ratings = [];
            if (c.ImageUrl != null) c.ImageUrl = "/api/v2/courses/" + c.Uuid + "/image";
        }

        return Ok(courses);
    }

    [HttpGet("course-categories")]
    public async Task<IActionResult> GetCoursesCategories(CancellationToken ct = default) {
        var categories = db.Categories.ToList();

        return Ok(categories);
    }
    
    [HttpGet("course-tags")]
    public async Task<IActionResult> GetCoursesTags([FromQuery] Guid categoryUuid, CancellationToken ct = default) {
        var tags = await db.Tags
            .Where(t => t.CategoryUuid == categoryUuid)
            .ToListAsync(ct);
        
        return Ok(tags);
    }
    
    [HttpPost("course-tags")]
    public async Task<IActionResult> CreateCourseTag([FromBody] CreateCourseTagRequest body, CancellationToken ct = default) {
        var acc = await auth.ReAuthAsync(ct);
        if (acc == null) return Unauthorized();

        if (acc is not Admin) return Forbid();

        var category = await db.Categories.FirstOrDefaultAsync(c => c.Uuid == body.CategoryUuid, ct);
        if (category == null) {
            return NotFound(new { error = "Category not found." });
        }

        var newTag = new Tag {
            DisplayName = body.DisplayName,
            CategoryUuid = body.CategoryUuid
        };

        db.Tags.Add(newTag);
        await db.SaveChangesAsync(ct);

        return new CreatedAtActionResult(
            actionName: nameof(GetCoursesTags),
            controllerName: "APIv2",
            routeValues: new { categoryUuid = body.CategoryUuid },
            value: newTag
        );
    }
    
    [HttpGet("me/courses")]
    public async Task<IActionResult> GetMyCourses(
        [FromQuery] bool full = false,
        [FromQuery] uint limit = 0u,
        CancellationToken ct = default
    ) {
        var acc = await auth.ReAuthAsync(ct);
        if (acc == null) return Unauthorized();
    
        var isLimited = limit > 0;
        var takeCount = limit == 0 ? int.MaxValue : (int)limit;
        
        if (full) {
            var query = db.Courses.AsQueryable();
            if (acc is not Admin) {
                query = query.Where(c => c.LecturerUuid == acc.Uuid);
            }
            
            var courses = await query
                .Include(c => c.Tags)
                .ThenInclude(t => t.Category)
                .Include(c => c.Ratings)
                .ThenInclude(l => l.Account)
                .Include(c => c.Account)
                .Include(c => c.Materials)
                .Include(c => c.Quizzes
                    .OrderByDescending(q => q.CreatedAt))
                .Include(c => c.Feed)
                .Include(c => c.Category)
                .OrderByDescending(c => c.CreatedAt)
                .Take(takeCount)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync(ct);

            foreach (var c in courses) {
                c.Materials = [];
                c.Quizzes = [];
                c.Feed = [];
                if(c.Account != null) c.Account.Ratings = [];
                if (c.ImageUrl != null) c.ImageUrl = "/api/v2/courses/" + c.Uuid + "/image";
            }

            return Ok(courses);
        } else {
            var query = db.Courses.AsQueryable();
            if (acc is not Admin) {
                query = query.Where(c => c.LecturerUuid == acc.Uuid);
            }
            
            var courses = await query
                .Include(c => c.Tags)
                .ThenInclude(t => t.Category)
                .Include(c => c.Ratings)
                .ThenInclude(l => l.Account)
                .Include(c => c.Category)
                .OrderByDescending(c => c.CreatedAt)
                .Take(takeCount)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync(ct);

            foreach (var c in courses) {
                c.Materials = [];
                c.Quizzes = [];
                c.Feed = [];
                if(c.Account != null) c.Account.Ratings = [];
                if (c.ImageUrl != null) c.ImageUrl = "/api/v2/courses/" + c.Uuid + "/image";
            }

            return Ok(courses);
        }
    }

    [HttpGet("courses/{uuid:guid}")]
    public async Task<IActionResult> GetCourseById(
        [FromRoute] Guid uuid,
        [FromQuery] bool full = true,
        CancellationToken ct = default
    ) {
        Course? course;

        if (full) {
            course = await db.Courses
                .Include(c => c.Materials
                    .OrderByDescending(m => m.CreatedAt))
                .Include(c => c.Quizzes
                    .OrderByDescending(q => q.CreatedAt))
                .Include(c => c.Feed)
                .Include(c => c.Account)
                .Include(c => c.Ratings)
                .Include(c => c.Category)
                .Include(c => c.Tags)
                .FirstOrDefaultAsync(c => c.Uuid == uuid, ct);

            if (course == null) return NotFound(new { error = "Course not found." });

            /*course.Account = await db.Lecturers
                .FirstOrDefaultAsync(l => l.Uuid == course.LecturerUuid, ct);

            Console.WriteLine(course.LecturerUuid + " " + course.Account?.Uuid);*/
        } else {
            course = await db.Courses
                .Include(c => c.Account)
                .Include(c => c.Ratings)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Uuid == uuid, ct);

            if (course == null) {
                return NotFound(new { error = "Course not found." });
            }
            
            course.Materials = [];
            course.Quizzes = [];
            course.Feed = [];
        }
        
        if(course.Account != null) course.Account.Ratings = [];

        return Ok(course.ToReadDto(true));
    }

    [HttpPut("courses/{uuid:guid}")]
    public async Task<IActionResult> UpdateCourse(Guid uuid, [FromBody] CreateCourseRequest body, CancellationToken ct = default) {
        var acc = await auth.ReAuthAsync(ct);
        if (acc == null) return Unauthorized();

        if (string.IsNullOrEmpty(body.Name) || string.IsNullOrEmpty(body.Description)) {
            return BadRequest(new { error = "Name and description are required." });
        }

        var existingCourse = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Account)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
        if (existingCourse == null) return NotFound();

        if (acc is not Admin && existingCourse.LecturerUuid != acc.Uuid) return Forbid();

        existingCourse.Materials = [];
        existingCourse.Quizzes = [];
        existingCourse.Feed = [];
        if(existingCourse.Account != null) existingCourse.Account.Ratings = [];
        existingCourse.Name = body.Name;
        existingCourse.Description = body.Description;
        
        // Category
        if (body.CategoryUuid.HasValue) {
            var category = await db.Categories
                .FirstOrDefaultAsync(c => c.Uuid == body.CategoryUuid.Value, ct);

            if (category == null) {
                return BadRequest(new { error = "Invalid category UUID." });
            }
            
            existingCourse.CategoryUuid = body.CategoryUuid;
        }

        // Tags
        if (body.TagsUuid is { Count: > 0 }) {
            var tags = await db.Tags
                .Where(t => body.TagsUuid.Contains(t.Uuid))
                .ToListAsync(ct);

            if (tags.Count != body.TagsUuid.Count) {
                return BadRequest(new { error = "One or more tags are invalid." });
            }
            
            existingCourse.Tags = tags;
        }
        if (body.TagsUuid is { Count: 0 }) {
            existingCourse.Tags = [];
        }
        
        var entry = db.Entry(existingCourse);
        if (entry.State == EntityState.Detached) {
            db.Courses.Update(existingCourse);
        }
        await db.SaveChangesAsync(ct);

        return Ok(existingCourse);
    }

    [HttpPut("courses/{uuid:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateCourseWithMaterials(Guid uuid, [FromForm] UpdateCourseWithMaterialsRequest body, CancellationToken ct = default) {
        var acc = await auth.ReAuthAsync(ct);
        if (acc == null) return Unauthorized();

        if (string.IsNullOrEmpty(body.Course.Name) || string.IsNullOrEmpty(body.Course.Description)) {
            return BadRequest(new { error = "Course name and description are required." });
        }

        var existingCourse = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Account)
            .Include(c => c.Materials)
            .Include(c => c.Quizzes)
            .Include(c => c.Feed)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
        if (existingCourse == null) return NotFound();

        if (acc is not Admin && existingCourse.LecturerUuid != acc.Uuid) return Forbid();

        existingCourse.Materials = [];
        existingCourse.Quizzes = [];
        existingCourse.Feed = [];
        if(existingCourse.Account != null) existingCourse.Account.Ratings = [];
        existingCourse.Name = body.Course.Name;
        existingCourse.Description = body.Course.Description;

        foreach (var urlMaterial in body.UrlMaterials) {

            var existingMaterial = existingCourse.Materials
                .OfType<UrlMaterial>()
                .FirstOrDefault(m => m.Uuid == urlMaterial.Uuid);

            if (existingMaterial == null) {
                if (string.IsNullOrEmpty(urlMaterial.Name)) {
                    return BadRequest(new { error = "Name is required for URL materials." });
                }

                if (string.IsNullOrEmpty(urlMaterial.Url)) {
                    return BadRequest(new { error = "URL is required for URL materials." });
                }

                var newMaterial = new UrlMaterial {
                    Name = urlMaterial.Name,
                    Description = urlMaterial.Description,
                    Type = Material.MaterialType.Url,
                    CourseUuid = existingCourse.Uuid,
                    Url = urlMaterial.Url,
                    FaviconUrl = $"https://www.google.com/s2/favicons?domain={new Uri(urlMaterial.Url).Host}&sz=64"
                };

                db.Materials.Add(newMaterial);
                await db.SaveChangesAsync(ct);
                continue;
            }

            if (!string.IsNullOrEmpty(urlMaterial.Name)) existingMaterial.Name = urlMaterial.Name;
            existingMaterial.Description = urlMaterial.Description;
            if (!string.IsNullOrEmpty(urlMaterial.Url)) {
                existingMaterial.Url = urlMaterial.Url;
                existingMaterial.FaviconUrl = $"https://www.google.com/s2/favicons?domain={new Uri(urlMaterial.Url).Host}&sz=64";
            }

            existingMaterial.UpdatedAt = DateTime.UtcNow;
            db.Materials.Update(existingMaterial);
            await db.SaveChangesAsync(ct);
        }

        foreach (var fileMaterial in body.FileMaterials) {
            var existingMaterial = existingCourse.Materials
                .OfType<FileMaterial>()
                .FirstOrDefault(m => m.Uuid == fileMaterial.Uuid);

            if (existingMaterial == null) {
                if (string.IsNullOrEmpty(fileMaterial.Name)) {
                    return BadRequest(new { error = "Name is required for file materials." });
                }

                if (fileMaterial.File == null || fileMaterial.File.Length == 0) {
                    return BadRequest(new { error = "File is required for file materials." });
                }

                if (!fileMaterial.File.IsAllowedMimeType()) return BadRequest(new { error = "Unsupported file type." });
                if (!fileMaterial.File.IsAllowedFileSize()) return BadRequest(new { error = "File size exceeds the maximum allowed limit of 30 MB." });

                var uploadedUrl = await materialAccessService.UploadFileMaterialAsync(existingCourse.Uuid, fileMaterial.File, ct);
                var mimeType = fileMaterial.File.ContentType.ToLowerInvariant().Split(';')[0];

                var newMaterial = new FileMaterial {
                    Name = fileMaterial.Name,
                    Description = fileMaterial.Description,
                    Type = Material.MaterialType.File,
                    CourseUuid = existingCourse.Uuid,
                    FileUrl = uploadedUrl,
                    MimeType = mimeType,
                    SizeBytes = (int)fileMaterial.File.Length
                };

                db.Materials.Add(newMaterial);
                await db.SaveChangesAsync(ct);
                continue;
            }

            if (!string.IsNullOrEmpty(fileMaterial.Name)) existingMaterial.Name = fileMaterial.Name;
            existingMaterial.Description = fileMaterial.Description;

            if (fileMaterial.File != null && fileMaterial.File.Length > 0) {
                if (!fileMaterial.File.IsAllowedMimeType()) return BadRequest(new { error = "Unsupported file type." });
                if (!fileMaterial.File.IsAllowedFileSize()) return BadRequest(new { error = "File size exceeds the maximum allowed limit of 30 MB." });

                var uploadedUrl = await materialAccessService.UploadFileMaterialAsync(existingCourse.Uuid, fileMaterial.File, ct);
                existingMaterial.FileUrl = uploadedUrl;
            }

            existingMaterial.UpdatedAt = DateTime.UtcNow;
            db.Materials.Update(existingMaterial);
            await db.SaveChangesAsync(ct);
        }

        // Category
        if (body.Course.CategoryUuid.HasValue) {
            var category = await db.Categories
                .FirstOrDefaultAsync(c => c.Uuid == body.Course.CategoryUuid.Value, ct);

            if (category == null) {
                return BadRequest(new { error = "Invalid category UUID." });
            }
            
            existingCourse.CategoryUuid = body.Course.CategoryUuid;
        }

        // Tags
        if (body.Course.TagsUuid is { Count: > 0 }) {
            var tags = await db.Tags
                .Where(t => body.Course.TagsUuid.Contains(t.Uuid))
                .ToListAsync(ct);

            if (tags.Count != body.Course.TagsUuid.Count) {
                return BadRequest(new { error = "One or more tags are invalid." });
            }
            
            existingCourse.Tags = tags;
        }
        if (body.Course.TagsUuid is { Count: 0 }) {
            existingCourse.Tags = [];
        }

        var entry = db.Entry(existingCourse);
        if (entry.State == EntityState.Detached) {
            db.Courses.Update(existingCourse);
        }
        await db.SaveChangesAsync(ct);
        
        return Ok(existingCourse.ToReadDto(true));
    }
    
    [HttpPost("courses/{uuid:guid}/image")]
    public async Task<IActionResult> UpdateCourseImage([FromRoute] Guid uuid, [FromForm] UpdateCourseImageRequest body, CancellationToken ct = default) {
        var acc = await auth.ReAuthAsync(ct);
        if (acc == null) return Unauthorized();

        var existingCourse = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Account)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
        if (existingCourse == null) return NotFound();

        if (acc is not Admin && existingCourse.LecturerUuid != acc.Uuid) return Forbid();

        if (body.Image == null || body.Image.Length == 0) {
            return BadRequest(new { error = "Image file is required." });
        }

        if (!body.Image.IsAllowedFileSize()) return BadRequest(new { error = "Image file size exceeds the maximum allowed limit of 5 MB." });

        var resizedImage = await ResizeImageAsync(body.Image, 500, ct);
        var imageUrl = await materialAccessService.UploadCourseImageAsync(existingCourse.Uuid, resizedImage, ct);
        existingCourse.ImageUrl = imageUrl;

        var entry = db.Entry(existingCourse);
        if (entry.State == EntityState.Detached) {
            db.Courses.Update(existingCourse);
        }
        await db.SaveChangesAsync(ct);

        return Ok(new { imageUrl = existingCourse.ImageUrl });
    }
    
    public async Task<IFormFile> ResizeImageAsync(IFormFile file, int size, CancellationToken ct)
    {
        await using var input = file.OpenReadStream();
        using var image = await Image.LoadAsync(input, ct);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(size, size),
            Mode = ResizeMode.Max // keeps aspect ratio
        }));

        var ms = new MemoryStream();
        await image.SaveAsWebpAsync(ms, new WebpEncoder { Quality = 80 }, ct);
        ms.Position = 0;

        return new FormFile(ms, 0, ms.Length, file.Name, "image.webp")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/webp"
        };
    }
    
    [HttpDelete("courses/{uuid:guid}/image")]
    public async Task<IActionResult> DeleteCourseImage([FromRoute] Guid uuid, CancellationToken ct = default) {
        var acc = await auth.ReAuthAsync(ct);
        if (acc == null) return Unauthorized();

        var existingCourse = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Account)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
        if (existingCourse == null) return NotFound();

        if (acc is not Admin && existingCourse.LecturerUuid != acc.Uuid) return Forbid();

        if (string.IsNullOrEmpty(existingCourse.ImageUrl)) {
            return NotFound(new { error = "Course image not found." });
        }

        await materialAccessService.DeleteFileMaterialAsync(existingCourse.ImageUrl, ct);
        existingCourse.ImageUrl = null;

        var entry = db.Entry(existingCourse);
        if (entry.State == EntityState.Detached) {
            db.Courses.Update(existingCourse);
        }
        await db.SaveChangesAsync(ct);

        return NoContent();
    }
    
    [HttpGet("courses/{uuid:guid}/image")]
    public async Task<IActionResult> GetCourseImage([FromRoute] Guid uuid, CancellationToken ct = default) {
        var imageUrl = await db.Courses
            .Where(c => c.Uuid == uuid)
            .Select(c => c.ImageUrl)
            .FirstOrDefaultAsync(ct);
        
        if (imageUrl == null) return NotFound();

        if (string.IsNullOrEmpty(imageUrl)) {
            return NotFound(new { error = "Course image not found." });
        }

        var imageStream = await materialAccessService.DownloadFileMaterialAsync(imageUrl, ct);
        imageStream.Position = 0;

        // Determine content type based on file extension
        string contentType = "application/octet-stream"; // Default content type
        var extension = Path.GetExtension(imageUrl).ToLowerInvariant();
        switch (extension) {
            case ".jpg":
            case ".jpeg":
                contentType = "image/jpeg";
                break;
            case ".png":
                contentType = "image/png";
                break;
            case ".gif":
                contentType = "image/gif";
                break;
            case ".bmp":
                contentType = "image/bmp";
                break;
            case ".webp":
                contentType = "image/webp";
                break;
        }

        return File(imageStream, contentType);
    }

    [HttpDelete("courses/{uuid:guid}")]
    public async Task<IActionResult> DeleteCourse([FromRoute] Guid uuid, CancellationToken ct = default) {
        var acc = await auth.ReAuthAsync(ct);
        if (acc == null) return Unauthorized();

        var existingCourse = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Account)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
        if (existingCourse == null) return NotFound();

        if (acc is not Admin && existingCourse.LecturerUuid != acc.Uuid) return Forbid();

        existingCourse.Materials = [];
        existingCourse.Quizzes = [];
        existingCourse.Feed = [];
        if(existingCourse.Account != null) existingCourse.Account.Ratings = [];

        db.Courses.Remove(existingCourse);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPost("courses/{uuid:guid}/rating")]
    public async Task<IActionResult> LikeOrDislikeCourse([FromRoute] Guid uuid, [FromBody] LikeCourseRequest body, CancellationToken ct = default) {
        var acc = await auth.ReAuthAsync(ct);
        if (acc == null) return Unauthorized();

        var course = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Account)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        var existingRating = await db.Ratings
            .FirstOrDefaultAsync(r => r.AccountUuid == acc.Uuid && r.CourseUuid == course.Uuid, ct);

        if (existingRating != null) {
            db.Ratings.Remove(existingRating);
        }

        switch (body.Type) {
            case "like": {
                var newRating = new Like {
                    AccountUuid = acc.Uuid,
                    CourseUuid = course.Uuid,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                db.Likes.Add(newRating);
            } break;

            case "dislike": {
                var newRating = new Dislike {
                    AccountUuid = acc.Uuid,
                    CourseUuid = course.Uuid,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                db.Dislikes.Add(newRating);
            } break;

            case null:
                // zadny novy rating, jen odstraneni stavajiciho
                break;

            default:
                return BadRequest(new { error = "Invalid rating type. Must be 'like' or 'dislike' or null." });
        }

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("courses/{courseUuid:guid}/view")] // TODO: pripadne casove omezeni resetu ip
    public async Task<IActionResult> UpdateCourseViewCount(
        [FromRoute] Guid courseUuid,
        [FromBody] CaptchaTokenRequest ctr,
        CancellationToken ct = default
    ) {
        // overeni captchy
        using var requestMessage = new HttpRequestMessage(
            HttpMethod.Post,
            "https://www.google.com/recaptcha/api/siteverify"
        );

        var formData = new FormUrlEncodedContent(new Dictionary<string, string> {
            { "secret", Program.ENV.GetValueOrNull("RECAPTCHA_SECRET_KEY") ?? "x" },
            { "response", ctr.Token }
            // { "remoteip", HttpContext.GetIPAddress()?.ToString() ?? "" } // volitelne pro recaptchu
        });

        requestMessage.Content = formData;

        var response = await HttpClient.SendAsync(requestMessage, ct);
        var responseContent = await response.Content.ReadAsStringAsync(ct);

        // overeni captcha
        var jsonResponse = JsonNode.Parse(responseContent);
        if (jsonResponse == null || jsonResponse["success"]?.GetValue<bool>() != true) {
            return BadRequest(new { error = "Recaptcha verification failed." });
        }

        // nalezeni kurzu v db
        var course = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Account)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        course.Materials = [];
        course.Quizzes = [];
        course.Feed = [];
        if(course.Account != null) course.Account.Ratings = [];

        // zjisteni jestli ip adresa neni v pouzitych
        var ipAddress = HttpContext.GetIPAddress();
        if (ipAddress == null) {
            return BadRequest(new { error = "Unable to determine client IP address." });
        }

        // kontrola jestli uz ip neni v pouzitych
        if (UsedIPsForCourse.TryGetValue(course.Uuid, out var l) && l.Contains(ipAddress)) {
            return BadRequest(new { error = "You have already used your view for today." });
        }

        // ip oznacit jako pouzitou (az po uspesne captcha)
        if(!UsedIPsForCourse.ContainsKey(course.Uuid)) UsedIPsForCourse.Add(course.Uuid, new List<IPAddress>());
        UsedIPsForCourse.TryGetValue(course.Uuid, out var list);
        list ??= [];
        list.Add(ipAddress);

        // aktualizace poctu zobrazeni kurzu
        course.ViewCount += 1;
        var entry = db.Entry(course);
        if (entry.State == EntityState.Detached) {
            db.Courses.Update(course);
        }
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPost("courses")] // tohle se na frontnendu nepouziva, jen pro API konzoli apod.
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest body, CancellationToken ct = default) {
        var acc = await auth.ReAuthAsync(ct);
        if (acc == null) return Unauthorized();

        if(string.IsNullOrEmpty(body.Name) || string.IsNullOrEmpty(body.Description)) {
            return BadRequest(new { error = "Name and description are required." });
        }

        if(acc is not Lecturer and not Admin) {
            return new JsonResult(new { error = "Only lecturers can create courses." }) { StatusCode = StatusCodes.Status403Forbidden };
        }

        var newCourse = new Course {
            Name = body.Name,
            Description = body.Description,
            LecturerUuid = acc.Uuid
        };

        // Category
        if (body.CategoryUuid.HasValue) {
            var category = await db.Categories
                .FirstOrDefaultAsync(c => c.Uuid == body.CategoryUuid.Value, ct);

            if (category == null) {
                return BadRequest(new { error = "Invalid category UUID." });
            }
            
            newCourse.CategoryUuid = body.CategoryUuid;
        }

        // Tags
        if (body.TagsUuid is { Count: > 0 }) {
            var tags = await db.Tags
                .Where(t => body.TagsUuid.Contains(t.Uuid))
                .ToListAsync(ct);

            if (tags.Count != body.TagsUuid.Count) {
                return BadRequest(new { error = "One or more tags are invalid." });
            }
            
            newCourse.Tags = tags;
        }
        if (body.TagsUuid == null || body.TagsUuid.Count == 0) {
            newCourse.Tags = [];
        }

        db.Courses.Add(newCourse);
        await db.SaveChangesAsync(ct);

        return new CreatedAtActionResult(
            actionName: nameof(GetCourseById),
            controllerName: "APIv2",
            routeValues: new { uuid = newCourse.Uuid },
            value: newCourse
        );
    }

    [HttpPost("courses")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateCourseWithMaterials([FromForm] CreateCourseWithMaterialsRequest body, CancellationToken ct = default) {
        var acc = await auth.ReAuthAsync(ct);
        if (acc == null) return Unauthorized();

        if(string.IsNullOrEmpty(body.Course.Name) || string.IsNullOrEmpty(body.Course.Description)) {
            return BadRequest(new { error = "Course name and description are required." });
        }

        if(acc is not Lecturer and not Admin) {
            return new JsonResult(new { error = "Only lecturers can create courses." }) { StatusCode = StatusCodes.Status403Forbidden };
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
            if (file.File == null || file.File.Length == 0) {
                return BadRequest(new { error = "File is required for file materials." });
            }

            if (!file.File.IsAllowedMimeType()) return BadRequest(new { error = "Unsupported file type." });
            if (!file.File.IsAllowedFileSize()) return BadRequest(new { error = "File size exceeds the maximum allowed limit of 30 MB." });

            var uploadedUrl = await materialAccessService.UploadFileMaterialAsync(newCourse.Uuid, file.File, ct);
            var mimeType = file.File.ContentType.ToLowerInvariant().Split(';')[0];

            var newMaterial = new FileMaterial {
                Name = file.Name,
                Description = file.Description,
                Type = Material.MaterialType.File,
                CourseUuid = newCourse.Uuid,
                FileUrl = uploadedUrl,
                MimeType = mimeType
            };

            newCourse.Materials.Add(newMaterial);
        }

        // Category
        if (body.Course.CategoryUuid.HasValue) {
            var category = await db.Categories
                .FirstOrDefaultAsync(c => c.Uuid == body.Course.CategoryUuid.Value, ct);

            if (category == null) {
                return BadRequest(new { error = "Invalid category UUID." });
            }
            
            newCourse.CategoryUuid = body.Course.CategoryUuid;
        }

        // Tags
        if (body.Course.TagsUuid is { Count: > 0 }) {
            var tags = await db.Tags
                .Where(t => body.Course.TagsUuid.Contains(t.Uuid))
                .ToListAsync(ct);

            if (tags.Count != body.Course.TagsUuid.Count) {
                return BadRequest(new { error = "One or more tags are invalid." });
            }
            
            newCourse.Tags = tags;
        }
        if (body.Course.TagsUuid == null || body.Course.TagsUuid.Count == 0) {
            newCourse.Tags = [];
        }

        db.Courses.Add(newCourse);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetCourseById), new { uuid = newCourse.Uuid }, null);
    }

    #region course materials

    [HttpPost("courses/{uuid:guid}/materials")]
    [Consumes("application/json")]
    public async Task<IActionResult> AddMaterialToCourse([FromRoute] Guid uuid, [FromBody] CreateUrlMaterialRequest body, CancellationToken ct = default) {
        var acc = await auth.ReAuthAsync(ct);
        if (acc == null) return Unauthorized();

        var course = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Account)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Category)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }
        
        if (acc is not Admin && course.LecturerUuid != acc.Uuid) return Forbid();

        if (body.Type != "url") {
            return BadRequest(new { error = "Only 'url' material type is supported in this endpoint." });
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

        db.Materials.Add(newMaterial);
        await db.SaveChangesAsync(ct);

        var obj = new {
            uuid = newMaterial.Uuid,
            name = newMaterial.Name,
            description = newMaterial.Description,
            url = newMaterial.Url,
            faviconUrl = newMaterial.FaviconUrl,
            type = "url",
            createdAt = newMaterial.CreatedAt,
            updatedAt = newMaterial.UpdatedAt
        };

        // odeslani info do sse
        var post = new FeedPost {
            Uuid = Guid.NewGuid(),
            CourseUuid = course.Uuid,
            Type = FeedPost.FeedPostType.System,
            Message = $"Byl přidán nový odkazový materiál: {newMaterial.Name}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Purpose = FeedPost.FeedPurpose.CreateMaterial
        };

        db.FeedPosts.Add(post);
        await db.SaveChangesAsync(ct);
        await fsb.PublishAsync(course.Uuid, new FeedStreamMessage("new_post", post), ct);

        return CreatedAtAction(nameof(GetCourseById), new { uuid = course.Uuid }, obj);
    }

    [HttpPost("courses/{courseId:guid}/materials")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddFileMaterialToCourse(
        [FromRoute] Guid courseId,
        [FromForm] CreateFileMaterialRequest body,
        CancellationToken ct = default
    ) {
        var acc = await auth.ReAuthAsync(ct);
        if (acc == null) return Unauthorized();

        var course = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Account)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Category)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == courseId, ct);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        if (acc is not Admin && course.LecturerUuid != acc.Uuid) return Forbid();

        if (body.Type != "file")
            return BadRequest(new { error = "Only 'file' material type is supported in this endpoint." });

        if (body.File == null)
            return BadRequest(new { error = "File is required." });

        if (!body.File.IsAllowedMimeType()) return BadRequest(new { error = "Unsupported file type." });
        if (!body.File.IsAllowedFileSize()) return BadRequest(new { error = "File size exceeds the maximum allowed limit of 30 MB." });

        var mimeType = body.File.ContentType.ToLowerInvariant().Split(';')[0];

        const long maxFileSizeBytes = 30 * 1024 * 1024;
        if (body.File.Length > maxFileSizeBytes) {
            return BadRequest(new { error = "File size exceeds the maximum allowed limit of 30 MB." });
        }

        var file = body.File;
        if (file.Length == 0) {
            return BadRequest(new { error = "File content is required." });
        }

        var fileUrl = await materialAccessService.UploadFileMaterialAsync(course.Uuid, file, ct);

        var newMaterial = new FileMaterial {
            Name = body.Name,
            Description = body.Description,
            Type = Material.MaterialType.File,
            CourseUuid = course.Uuid,
            FileUrl = fileUrl,
            MimeType = mimeType,
            SizeBytes = (int)file.Length,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Materials.Add(newMaterial);
        await db.SaveChangesAsync(ct);

        var responseObj = new {
            uuid = newMaterial.Uuid,
            type = "file",
            name = newMaterial.Name,
            description = newMaterial.Description,
            fileUrl = newMaterial.FileUrl,
            mimeType = newMaterial.MimeType,
            sizeBytes = newMaterial.SizeBytes,
            createdAt = newMaterial.CreatedAt,
            updatedAt = newMaterial.UpdatedAt
        };

        // odeslani info do sse
        var post = new FeedPost {
            Uuid = Guid.NewGuid(),
            CourseUuid = course.Uuid,
            Type = FeedPost.FeedPostType.System,
            Message = $"Byl přidán nový souborový materiál: {newMaterial.Name}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Purpose = FeedPost.FeedPurpose.CreateMaterial
        };

        db.FeedPosts.Add(post);
        await db.SaveChangesAsync(ct);
        await fsb.PublishAsync(course.Uuid, new FeedStreamMessage("new_post", post), ct);

        return CreatedAtAction(nameof(GetCourseById), new { uuid = course.Uuid }, responseObj);
    }

    [HttpGet("courses/{courseUuid:guid}/materials/{materialUuid:guid}")]
    public async Task<IActionResult> GetCourseMaterialById([FromRoute] Guid courseUuid, [FromRoute] Guid materialUuid, CancellationToken ct = default) {
        var course = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Account)
            .Include(c => c.Materials)
            .Include(c => c.Quizzes)
            .Include(c => c.Feed)
            .Include(c => c.Category)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
        if (course == null)
            return NotFound(new { error = "Course not found." });

        var material = course.Materials.FirstOrDefault(m => m.Uuid == materialUuid);
        if (material == null)
            return NotFound(new { error = "Material not found." });

        switch (material) {
            case UrlMaterial urlMaterial:
                return Ok(urlMaterial.ToReadDto());
            case FileMaterial fileMaterial:
                try {
                    var memoryStream = await materialAccessService.DownloadFileMaterialAsync(fileMaterial.FileUrl, ct);

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

                    Response.Headers.ContentDisposition = "inline";

                    // If mime type can be shown in browser, do not force download
                    return File(memoryStream, fileMaterial.MimeType ?? "application/octet-stream", fileMaterial.MimeType == null || fileMaterial.MimeType == "application/octet-stream" ? fileName : null);
                }
                catch (Minio.Exceptions.MinioException e)
                {
                    return StatusCode(500, new { error = "Error fetching file from storage.", detail = e.Message });
                }
            default:
                return StatusCode(500, new { error = "Unknown material type." });

        }
    }

    [HttpDelete("courses/{courseUuid:guid}/materials/{materialUuid:guid}")]
    public async Task<IActionResult> DeleteCourseMaterialById([FromRoute] Guid courseUuid, [FromRoute] Guid materialUuid, CancellationToken ct = default) {
        var acc = await auth.ReAuthAsync(ct);
        if (acc == null) return Unauthorized();

        var existingCourse = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Account)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
        if (existingCourse == null) return NotFound();

        if (acc is not Admin && existingCourse.LecturerUuid != acc.Uuid) return Forbid();

        var material = await db.Materials
            .FirstOrDefaultAsync(m => m.Uuid == materialUuid, ct);
        if (material == null || material.CourseUuid != courseUuid) {
            return NotFound(new { error = "Material not found." });
        }
        
        // odeslani info do sse
        var newFeedPost = new FeedPost {
            Uuid = Guid.NewGuid(),
            CourseUuid = existingCourse.Uuid,
            Type = FeedPost.FeedPostType.System,
            Message = $"Byl smazán materiál: {material.Name}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Purpose = FeedPost.FeedPurpose.DeleteMaterial
        };
        
        db.FeedPosts.Add(newFeedPost);
        await db.SaveChangesAsync();
        
        await fsb.PublishAsync(
            existingCourse.Uuid, 
            new FeedStreamMessage("new_post", newFeedPost)
        );

        db.Materials.Remove(material);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPut("courses/{courseUuid:guid}/materials/{materialUuid:guid}")]
    [Consumes("application/json")]
    public async Task<IActionResult> UpdateUrlMaterialInCourse(
        [FromRoute] Guid courseUuid,
        [FromRoute] Guid materialUuid,
        [FromBody] UpdateUrlMaterialRequest body,
        CancellationToken ct = default
    ) {
        var acc = await auth.ReAuthAsync(ct);
        if (acc == null) return Unauthorized();


        var course = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Account)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        if (acc is not Admin && course.LecturerUuid != acc.Uuid) return Forbid();

        var material = await db.Materials
            .FirstOrDefaultAsync(m => m.Uuid == materialUuid, ct);

        switch (material) {
            case UrlMaterial urlMaterial:
                if (!string.IsNullOrEmpty(body.Name))
                    urlMaterial.Name = body.Name;

                if (!string.IsNullOrEmpty(body.Description))
                    urlMaterial.Description = body.Description;

                if (!string.IsNullOrEmpty(body.Url)) {
                    urlMaterial.Url = body.Url;
                    urlMaterial.FaviconUrl = $"https://www.google.com/s2/favicons?domain={new Uri(body.Url).Host}&sz=64";
                }

                urlMaterial.UpdatedAt = DateTime.UtcNow;
                db.Materials.Update(urlMaterial);
                await db.SaveChangesAsync(ct);
                
                var newFeedPost = new FeedPost {
                    Uuid = Guid.NewGuid(),
                    CourseUuid = course.Uuid,
                    Type = FeedPost.FeedPostType.System,
                    Message = $"Byl upraven odkazový materiál: {urlMaterial.Name}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Purpose = FeedPost.FeedPurpose.UpdateMaterial
                };

                db.FeedPosts.Add(newFeedPost);
                await db.SaveChangesAsync(ct);

                await fsb.PublishAsync(
                    course.Uuid,
                    new FeedStreamMessage("new_post", newFeedPost),
                    ct
                );
                
                return Ok(urlMaterial.ToReadDto());
            
            case FileMaterial fileMaterial:
                if (!string.IsNullOrEmpty(body.Name))
                    fileMaterial.Name = body.Name;

                if (!string.IsNullOrEmpty(body.Description))
                    fileMaterial.Description = body.Description;

                fileMaterial.UpdatedAt = DateTime.UtcNow;
                db.Materials.Update(fileMaterial);
                await db.SaveChangesAsync(ct);
                
                return Ok(fileMaterial.ToReadDto());

            default:
                return BadRequest(new { error = "Material is not of type 'url'." });
        }
    }

    [HttpPut("courses/{courseUuid:guid}/materials/{materialUuid:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateFileMaterialInCourse(
        [FromRoute] Guid courseUuid,
        [FromRoute] Guid materialUuid,
        [FromForm] UpdateFileMaterialRequest body,
        CancellationToken ct = default
    ) {
        var acc = await auth.ReAuthAsync(ct);
        if (acc == null) return Unauthorized();


        var course = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Account)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        if (acc is not Admin && course.LecturerUuid != acc.Uuid) return Forbid();

        var material = await db.Materials
            .FirstOrDefaultAsync(m => m.Uuid == materialUuid, ct);

        if (material == null || material.CourseUuid != course.Uuid)
            return NotFound(new { error = "Material not found in the specified course." });

        if (material is not FileMaterial fileMaterial)
            return BadRequest(new { error = "Material is not of type 'file'." });

        if (!string.IsNullOrEmpty(body.Name))
            fileMaterial.Name = body.Name;

        if (!string.IsNullOrEmpty(body.Description))
            fileMaterial.Description = body.Description;

        if (body.File != null && body.File.Length > 0) {
            if (!body.File.IsAllowedMimeType()) return BadRequest(new { error = "Unsupported file type." });
            if (!body.File.IsAllowedFileSize()) return BadRequest(new { error = "File size exceeds the maximum allowed limit of 30 MB." });

            var mimeType = body.File.ContentType.ToLowerInvariant()?.Split(';')[0] ?? "";

            await materialAccessService.DeleteFileMaterialAsync(fileMaterial.FileUrl, ct);

            var newFileUrl = await materialAccessService.UploadFileMaterialAsync(course.Uuid, body.File, ct);
            fileMaterial.FileUrl = newFileUrl;
            fileMaterial.MimeType = mimeType;
            fileMaterial.SizeBytes = (int)body.File.Length;
        }

        fileMaterial.UpdatedAt = DateTime.UtcNow;
        db.Materials.Update(fileMaterial);
        await db.SaveChangesAsync(ct);

        // odeslani info do sse
        
        var newFeedPost = new FeedPost {
            Uuid = Guid.NewGuid(),
            CourseUuid = course.Uuid,
            Type = FeedPost.FeedPostType.System,
            Message = $"Byl upraven souborový materiál: {fileMaterial.Name}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Purpose = FeedPost.FeedPurpose.UpdateMaterial
        };
        
        db.FeedPosts.Add(newFeedPost);
        await db.SaveChangesAsync();
        
        await fsb.PublishAsync(
            course.Uuid, 
            new FeedStreamMessage("new_post", newFeedPost)
        );
        
        return Ok(fileMaterial.ToReadDto());
    }

    #endregion
    
    #region course Kvizy
    
    [HttpPost("courses/{courseUuid:guid}/quizzes/{quizUuid:guid}/submit")]
    public async Task<IActionResult> SubmitQuiz(
        [FromRoute] Guid courseUuid,
        [FromRoute] Guid quizUuid,
        [FromBody] CreateQuizSubmissionRequest body
    ) {
        var course = await db.Courses
            .Where(c => c.Uuid == courseUuid)
            .FirstOrDefaultAsync();
        
        if (course == null)
            return NotFound(new { error = "Course not found." });

        var quiz = await db.Quizzes
            .Where(q => q.CourseUuid == courseUuid)
            .Where(q => q.Uuid == quizUuid)
            .Include(q => q.Questions)
            .ThenInclude(qn => qn.Options)
            .FirstOrDefaultAsync();
        
        if (quiz == null)
            return NotFound(new { error = "Quiz not found." });

        int totalQuestions = quiz.Questions.Count;
        int correctAnswers = 0;
        var correctPerQuestion = new List<bool>();

        foreach (var question in quiz.Questions) {
            var submission = body.Answers.FirstOrDefault(a => a.Uuid == question.Uuid);
            if (submission == null) continue;

            bool isCorrect = false;
            
            question.Options = question.Options.OrderBy(o => o.Order).ToList();

            switch (question) {
                case SingleChoiceQuestion scq: {
                    var correctOptionIndex = scq.Options.ToList().FindIndex(o => o.IsCorrect);
                    if (submission.SelectedIndex == correctOptionIndex) {
                        isCorrect = true;
                    }
                } break;

                case MultipleChoiceQuestion mcq: {
                    var correctIndices = mcq.Options
                        .Select((o, index) => new { o.IsCorrect, index })
                        .Where(x => x.IsCorrect)
                        .Select(x => x.index)
                        .ToHashSet();

                    if (submission.SelectedIndices != null &&
                        submission.SelectedIndices.Count == correctIndices.Count &&
                        submission.SelectedIndices.All(i => correctIndices.Contains(i))) {
                        isCorrect = true;
                    }
                } break;
            }
            
            if (isCorrect) correctAnswers++;
            correctPerQuestion.Add(isCorrect);
        }
        
        var quizResult = new QuizResult {
            QuizUuid = quiz.Uuid,
            Score = correctAnswers,
            CompletedAt = DateTime.UtcNow,
            Answers = body.Answers.Select(a => {
                var answer = new QuizAnswer {
                    QuestionUuid = a.Uuid,
                    SelectedOptions = new List<QuizAnswerOption>()
                };

                var question = quiz.Questions.FirstOrDefault(q => q.Uuid == a.Uuid);
                if (question == null) return answer;

                question.Options = question.Options.OrderBy(o => o.Order).ToList();

                switch (question) {
                    case SingleChoiceQuestion scq: {
                        if (a.SelectedIndex is >= 0 && a.SelectedIndex.Value < scq.Options.Count) {
                            var selectedOption = scq.Options.ElementAt(a.SelectedIndex.Value);
                            answer.SelectedOptions.Add(new QuizAnswerOption {
                                OptionUuid = selectedOption.Uuid
                            });
                        }
                    } break;

                    case MultipleChoiceQuestion mcq: {
                        if (a.SelectedIndices != null) {
                            foreach (var selectedOption in from index in a.SelectedIndices where index >= 0 && index < mcq.Options.Count select mcq.Options.ElementAt(index)) {
                                answer.SelectedOptions.Add(new QuizAnswerOption {
                                    OptionUuid = selectedOption.Uuid
                                });
                            }
                        }
                    } break;
                }

                return answer;
            }).ToList()
        };

        quiz.AttemptsCount++;
        
        db.QuizResults.Add(quizResult);
        await db.SaveChangesAsync();

        return Ok(new {
            resultUuid = quizResult.Uuid
        });
    }

    [HttpGet("courses/{courseUuid:guid}/quizzes/{quizUuid:guid}/results/{resultUuid:guid}")]
    public async Task<IActionResult> GetQuizResult(
        [FromRoute] Guid courseUuid,
        [FromRoute] Guid quizUuid,
        [FromRoute] Guid resultUuid
    ) {
        var course = await db.Courses
            .Where(c => c.Uuid == courseUuid)
            .FirstOrDefaultAsync();
        
        if (course == null)
            return NotFound(new { error = "Course not found." });
        
        var quiz = await db.Quizzes
            .Where(q => q.CourseUuid == courseUuid)
            .Where(q => q.Uuid == quizUuid)
            .Include(q => q.Questions
                .OrderBy(qs => qs.Order))
                .ThenInclude(qn => qn.Options)
            .FirstOrDefaultAsync();

        if (quiz == null || quiz.CourseUuid != course.Uuid)
            return NotFound(new { error = "Quiz not found in the specified course." });
        
        var quizResult = await db.QuizResults
            .Where(qr => qr.Uuid == resultUuid)
            .Include(qr => qr.Answers)
                .ThenInclude(a => a.SelectedOptions)
            .FirstOrDefaultAsync();
        
        if (quizResult == null || quizResult.QuizUuid != quiz.Uuid)
            return NotFound(new { error = "Quiz result not found for the specified quiz." });
        
        var quizDto = quiz.ToReadDto(true);

        foreach (ReadQuestionResponse question in quizDto.Questions) {
            var answer = quizResult.Answers.FirstOrDefault(a => a.QuestionUuid == question.Uuid);
            
            if (answer == null) continue;
            
            var selectedIndices = answer.SelectedOptions.Select(selectedOpt => quiz.Questions.First(q => q.Uuid == question.Uuid)
                    .Options.OrderBy(o => o.Order)
                    .ToList()
                    .FindIndex(o => o.Uuid == selectedOpt.OptionUuid))
                .Where(optionIndex => optionIndex >= 0)
                .ToList();
            
            question.SelectedIndices = selectedIndices;
            
            switch (question) {
                case ReadSingleChoiceQuestionResponse scq: {
                    var correctIndex = quiz.Questions
                        .OfType<SingleChoiceQuestion>()
                        .First(q => q.Uuid == question.Uuid)
                        .Options
                        .OrderBy(o => o.Order)
                        .ToList()
                        .FindIndex(o => o.IsCorrect);
                
                    question.IsCorrect = selectedIndices.Count == 1 && selectedIndices[0] == correctIndex;
                    break;
                }

                case ReadMultipleChoiceQuestionResponse mcq: {
                    var correctIndices = quiz.Questions
                        .OfType<MultipleChoiceQuestion>()
                        .First(q => q.Uuid == question.Uuid)
                        .Options
                        .Select((o, index) => new { o.IsCorrect, index })
                        .Where(x => x.IsCorrect)
                        .Select(x => x.index)
                        .ToHashSet();
                
                    question.IsCorrect = selectedIndices.Count == correctIndices.Count &&
                                         selectedIndices.All(i => correctIndices.Contains(i));
                    break;
                }
            }
        }
        
        return Ok(new {
            uuid = quizResult.Uuid,
            quiz = quizDto,
            score = quizResult.Score,
            totalQuestions = quiz.Questions.Count,
            completedAt = quizResult.CompletedAt
        });
    }

    #endregion

    #endregion





    #region FeedPosts

    [HttpGet("courses/{courseUuid:guid}/feed")]
    public async Task<IActionResult> GetFeedPostsByCourseId([FromRoute] Guid courseUuid) {
        var courseExists = await db.Courses
            .AnyAsync(c => c.Uuid == courseUuid);
        if (!courseExists) {
            return NotFound(new { error = "Course not found." });
        }

        var feedPosts = await db.FeedPosts
            .Where(fp => fp.CourseUuid == courseUuid)
            .Include(fp => fp.Course)
            .Include(fp => fp.Account)
            .OrderByDescending(fp => fp.CreatedAt)
            .ToListAsync();

        return Ok(feedPosts);
    }

    [HttpPost("courses/{courseUuid:guid}/feed")]
    public async Task<IActionResult> CreateFeedPostInCourse(
        [FromRoute] Guid courseUuid,
        [FromBody] CreateCourseFeedPostRequest body,
        CancellationToken ct
    ) {
        var courseExists = await db.Courses
            .AnyAsync(c => c.Uuid == courseUuid, ct);
        if (!courseExists)
            return NotFound(new { error = "Course not found." });


        var loggedAccount = await auth.ReAuthAsync(ct);

        var newFeedPost = new FeedPost {
            Uuid = Guid.NewGuid(),
            Type = FeedPost.FeedPostType.Manual,
            Message = body.Message,
            CourseUuid = courseUuid,
            AccountUuid = loggedAccount?.Uuid,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.FeedPosts.Add(newFeedPost);
        await db.SaveChangesAsync(ct);

        await fsb.PublishAsync(courseUuid, new FeedStreamMessage("new_post", newFeedPost), ct);

        return CreatedAtAction(nameof(GetFeedPostsByCourseId), new { courseUuid }, newFeedPost);
    }


    [HttpDelete("courses/{courseUuid:guid}/feed/{feedPostUuid:guid}")]
    public async Task<IActionResult> DeleteFeedPostFromCourse(
        [FromRoute] Guid courseUuid,
        [FromRoute] Guid feedPostUuid,
        CancellationToken ct
    ) {
        var courseExists = await db.Courses
            .AnyAsync(c => c.Uuid == courseUuid, ct);
        if (!courseExists)
            return NotFound(new { error = "Course not found." });

        var feedPost = await db.FeedPosts
            .Where(fp => fp.CourseUuid == courseUuid)
            .Where(fp => fp.Uuid == feedPostUuid)
            .FirstOrDefaultAsync(ct);

        if (feedPost is null)
            return NotFound(new { error = "Feed post not found in the specified course." });

        db.FeedPosts.Remove(feedPost);
        await db.SaveChangesAsync(ct);

        await fsb.PublishAsync(courseUuid, new FeedStreamMessage("delete_post", new { uuid = feedPostUuid }), ct);

        return NoContent();
    }

    [HttpPut("courses/{courseUuid:guid}/feed/{feedPostUuid:guid}")]
    public async Task<IActionResult> UpdateFeedPostInCourse(
        [FromRoute] Guid courseUuid,
        [FromRoute] Guid feedPostUuid,
        [FromBody] EditCourseFeedPostRequest body,
        CancellationToken ct
    ) {
        var courseExists = await db.Courses
            .AnyAsync(c => c.Uuid == courseUuid, ct);
        if (!courseExists)
            return NotFound(new { error = "Course not found." });

        var feedPost = await db.FeedPosts
            .Where(fp => fp.CourseUuid == courseUuid)
            .Where(fp => fp.Uuid == feedPostUuid)
            .Include(fp => fp.Course)
            .Include(fp => fp.Account)
            .FirstOrDefaultAsync(ct);

        if (feedPost is null)
            return NotFound(new { error = "Feed post not found in the specified course." });

        feedPost.Message = body.Message;
        feedPost.Edited = body.Edited;

        await db.SaveChangesAsync(ct);

        await fsb.PublishAsync(courseUuid, new FeedStreamMessage("update_post", feedPost), ct);

        return Ok(feedPost);
    }

    #endregion
}