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
using tda26.Server.Infrastructure;
using tda26.Server.Services;
using CreateCourseRequest = tda26.Server.DTOs.v1.CreateCourseRequest;

namespace tda26.Server.API;

[ApiController]
[Route("api/v1")]
public sealed class APIv1(
	IAuthService auth,
	IMaterialAccessService materialAccessService,
	AppDbContext db,
	IFeedStreamBroker fsb,
	IStreamBroker sb,
	ILogger<APIv1> logger
) : Controller {

	// random picovinky
	private static readonly HttpClient HttpClient = new();
	private static readonly Dictionary<Guid, List<IPAddress>> UsedIPsForCourse = new();



	[HttpGet]
	public IActionResult Index() {
		return Ok(new {
			status = "ok",
			timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
			message = "Toto je API verze 1."
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
	public async Task<IActionResult> Me(CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();

		// odstraneni policek
		foreach (var like in acc?.Ratings ?? []) {
			like.Account = null;
			like.Course.Account = null;
		}

		return Ok(acc);
	}

	[HttpPost("auth/login")]
	public async Task<IActionResult> Login([FromBody] AuthLoginRequest body, CancellationToken ct = default) {
		body.Username = body.Username?.Trim() ?? string.Empty;
		body.Password = body.Password?.Trim() ?? string.Empty;
		if (string.IsNullOrEmpty(body.Username) || string.IsNullOrEmpty(body.Password)) {
			return new BadRequestObjectResult(new {
				message = "Uživatelské jméno a heslo jsou povinné."
			});
		}

		var acc = await auth.LoginAsync(body.Username, body.Password, ct);

		if (acc == null) return new UnauthorizedObjectResult(new { message = "Neplatné uživatelské jméno nebo heslo." });

		// odstraneni policek
		foreach (var like in acc?.Ratings ?? []) {
			like.Account = null;
			like.Course.Account = null;
		}

		return Ok(acc);
	}

	[HttpPost("auth/logout")]
	public async Task<IActionResult> Logout(CancellationToken ct = default) {
		await auth.LogoutAsync(ct);
		return Ok(new { message = "Úspěšně odhlášen." });
	}

	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] AuthRegisterRequest body, CancellationToken ct = default) {
		body.Username = body.Username?.Trim() ?? string.Empty;
		body.Email = body.Email?.Trim() ?? string.Empty;
		body.Password = body.Password?.Trim() ?? string.Empty;

		if (string.IsNullOrEmpty(body.Username) || string.IsNullOrEmpty(body.Password)) {
			return new BadRequestObjectResult(new {
				message = "Uživatelské jméno a heslo jsou povinné."
			});
		}

		var acc = await auth.RegisterAsync(body.Username, body.Email, body.Password, ct);

		if (acc == null) {
			return new ConflictObjectResult(new {
				message = "Uživatelské jméno už existuje."
			});
		}

		foreach (var like in acc?.Ratings ?? []) {
			like.Account = null;
			like.Course.Account = null;
		}

		return new CreatedAtActionResult(
			nameof(GetAccount),
			"APIv1",
			new { uuid = acc?.Uuid },
			acc
		);
	}



	#region lecturers

	// lecturers
	[HttpGet("lecturers")]
	public async Task<IActionResult> GetLecturers([FromQuery] uint limit = 0, CancellationToken ct = default) {
		var isLimited = limit > 0;

		var all = await db.Lecturers
			.OrderBy(l => l.CreatedAt)
			.Take(isLimited ? (int)limit : int.MaxValue)
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
			nameof(GetLecturer),
			"APIv1",
			new { uuid = newLecturer.Uuid },
			newLecturer
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
		var account = await db.AccountsEf()
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
		var acc = await auth.ReAuthAsync(ct);
		var accUuid = acc?.Uuid;
		var isAdmin = acc is Admin;
		var isLimited = limit > 0;

		var courses = await db.CoursesMinimalEf()
			.Where(c => c.Status == CourseStatus.Live || c.Status == CourseStatus.Paused || c.Status == CourseStatus.Scheduled || isAdmin)
			.OrderByDescending(c => c.CreatedAt)
			.Take(isLimited ? (int)limit : int.MaxValue)
			.AsNoTracking()
			.AsSplitQuery()
			.ToListAsync(ct);

		foreach (var c in courses) {
			c.Materials = [];
			c.Quizzes = [];
			c.Feed = [];
			if (c.Account != null) c.Account.Ratings = [];
			if (c.ImageUrl != null) c.ImageUrl = "/api/v1/courses/" + c.Uuid + "/image";
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
			nameof(GetCoursesTags),
			"APIv1",
			new { categoryUuid = body.CategoryUuid },
			newTag
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
			var query = db.CoursesFullEf()
				.AsNoTracking()
				.AsSplitQuery();
			
			if (acc is not Admin) {
				query = query.Where(c => c.LecturerUuid == acc.Uuid);
			}

			var courses = await query
				.OrderByDescending(c => c.CreatedAt)
				.Take(takeCount)
				.ToListAsync(ct);

			foreach (var c in courses) {
				c.Materials = [];
				c.Quizzes = [];
				c.Feed = [];
				if (c.Account != null) c.Account.Ratings = [];
				if (c.ImageUrl != null) c.ImageUrl = "/api/v1/courses/" + c.Uuid + "/image";
			}

			return Ok(courses);
		}
		else {
			var query = db.CoursesMinimalEf()
				.AsNoTracking()
				.AsSplitQuery();
			if (acc is not Admin) {
				query = query.Where(c => c.LecturerUuid == acc.Uuid);
			}

			var courses = await query
				.OrderByDescending(c => c.CreatedAt)
				.Take(takeCount)
				.ToListAsync(ct);

			foreach (var c in courses) {
				c.Materials = [];
				c.Quizzes = [];
				c.Feed = [];
				if (c.Account != null) c.Account.Ratings = [];
				if (c.ImageUrl != null) c.ImageUrl = "/api/v1/courses/" + c.Uuid + "/image";
			}

			return Ok(courses);
		}
	}

	private IActionResult? ValidateRestrictedCourseAccess(Course course, Account? acc) {
		if (course.Status is CourseStatus.Archived or CourseStatus.Draft) {
			if (acc == null) {
				return Unauthorized();
			}

			if (course.LecturerUuid != acc.Uuid && acc is not Admin) {
				return Forbid();
			}
		}

		return null;
	}

	[HttpGet("courses/{uuid:guid}")]
	public async Task<IActionResult> GetCourseById(
		[FromRoute] Guid uuid,
		[FromQuery] bool full = true,
		CancellationToken ct = default
	) {
		Course? course;

		var acc = await auth.ReAuthAsync(ct);

		if (full) {
			course = await db.CoursesFullEf()
				.AsNoTracking()
				.AsSplitQuery()
				.FirstOrDefaultAsync(c => c.Uuid == uuid, ct);

			if (course == null) return NotFound(new { error = "Course not found." });

			var ownsCourse = acc != null && course.LecturerUuid != null && course.LecturerUuid == acc.Uuid;

			// Hide non-visible modules for users who do not own the course
			if (!ownsCourse && acc is not Admin) {
				course.Materials = [];
				course.Quizzes = [];
				
				course.Modules = course.Modules
					.Where(m => m.IsVisible)
					.ToList();
			}

			/*course.Account = await db.Lecturers
			    .FirstOrDefaultAsync(l => l.Uuid == course.LecturerUuid, ct);

			Console.WriteLine(course.LecturerUuid + " " + course.Account?.Uuid);*/
		}
		else {
			course = await db.CoursesMinimalEf()
				.AsNoTracking()
				.AsSplitQuery()
				.FirstOrDefaultAsync(c => c.Uuid == uuid, ct);

			if (course == null) {
				return NotFound(new { error = "Course not found." });
			}

			course.Materials = [];
			course.Quizzes = [];
			course.Feed = [];
		}

		var accessResult = ValidateRestrictedCourseAccess(course, acc);
		if (accessResult != null) {
			return accessResult;
		}

		if (course.Account != null) course.Account.Ratings = [];

		foreach (var quiz in course.Quizzes) {
			// Fix inconsistent attempt counts for quizzes
			var attemptsCount = await db.QuizResultsEf()
				.AsNoTracking()
				.CountAsync(qa => qa.QuizUuid == quiz.Uuid, ct);
			
			quiz.AttemptsCount = attemptsCount;
		}

		return Ok(course.ToReadDto(true));
	}

	[HttpPut("courses/{uuid:guid}")]
	public async Task<IActionResult> UpdateCourse(Guid uuid, [FromBody] CreateCourseRequest body, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();

		if (string.IsNullOrEmpty(body.Name) || string.IsNullOrEmpty(body.Description)) {
			return BadRequest(new { error = "Name and description are required." });
		}

		var existingCourse = await db.CoursesMinimalEf()
			.FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
		if (existingCourse == null) return NotFound();

		if (acc is not Admin && existingCourse.LecturerUuid != acc.Uuid) return Forbid();

		if (existingCourse.Status != CourseStatus.Draft) return BadRequest(new { error = "Only courses in draft status can be updated." });

		existingCourse.Materials = [];
		existingCourse.Quizzes = [];
		existingCourse.Feed = [];
		if (existingCourse.Account != null) existingCourse.Account.Ratings = [];
		existingCourse.Name = body.Name;
		existingCourse.Description = body.Description;

		// zmeneni statusu na live pokud je naplanovany start v minulosti
		var scheduledStatusChangedToLive = false;
		if (body.Status == CourseStatus.Scheduled) {
			existingCourse.Status = CourseStatus.Scheduled;
			scheduledStatusChangedToLive = await existingCourse.CheckSchedulingAsync(sb, ct);
		}

		// pokud se zmenil status (v requestu zmeneno a nebylo to nasledkem scheduled checku), tak se publishne do streamu zprava
		if (!scheduledStatusChangedToLive && body.Status != existingCourse.Status) {
			existingCourse.Status = body.Status;

			logger.LogTrace("Course {CourseUuid} status changed to {Status}, publishing to stream", existingCourse.Uuid, body.Status);

			await sb.PublishAsync(
				existingCourse.Uuid,
				new StreamMessage("status_changed", new { status = body.Status.ToString().ToLower() }),
				ct
			);
		}


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

		var existingCourse = await db.CoursesFullEf()
			.FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
		if (existingCourse == null) return NotFound();

		if (acc is not Admin && existingCourse.LecturerUuid != acc.Uuid) return Forbid();

		if (existingCourse.Status != CourseStatus.Draft) return BadRequest(new { error = "Only courses in draft status can be updated." });

		existingCourse.Materials = [];
		existingCourse.Quizzes = [];
		existingCourse.Feed = [];
		if (existingCourse.Account != null) existingCourse.Account.Ratings = [];
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

			if (fileMaterial.File is { Length: > 0 }) {
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

		var existingCourse = await db.CoursesMinimalEf()
			.FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
		if (existingCourse == null) return NotFound();

		if (acc is not Admin && existingCourse.LecturerUuid != acc.Uuid) return Forbid();

		if (existingCourse.Status != CourseStatus.Draft) return BadRequest(new { error = "Only courses in draft status can be updated." });

		if (body.Image == null || body.Image.Length == 0) {
			return BadRequest(new { error = "Image file is required." });
		}

		if (!body.Image.IsAllowedFileSize()) return BadRequest(new { error = "Image file size exceeds the maximum allowed limit of 5 MB." });

		var resizedImage = await ResizeImageAsync(body.Image, 500, ct);
		var imageUrl = await materialAccessService.UploadCourseImageAsync(existingCourse.Uuid, resizedImage, ct);
		existingCourse.ImageUrl = imageUrl;
		existingCourse.UpdatedAt = DateTime.UtcNow;

		var entry = db.Entry(existingCourse);
		if (entry.State == EntityState.Detached) {
			db.Courses.Update(existingCourse);
		}
		await db.SaveChangesAsync(ct);

		return Ok(new { imageUrl = existingCourse.ImageUrl });
	}

	public async Task<IFormFile> ResizeImageAsync(IFormFile file, int size, CancellationToken ct) {
		await using var input = file.OpenReadStream();
		using var image = await Image.LoadAsync(input, ct);

		image.Mutate(x => x.Resize(new ResizeOptions {
			Size = new Size(size, size),
			Mode = ResizeMode.Max // keeps aspect ratio
		}));

		var ms = new MemoryStream();
		await image.SaveAsWebpAsync(ms, new WebpEncoder { Quality = 80 }, ct);
		ms.Position = 0;

		return new FormFile(ms, 0, ms.Length, file.Name, "image.webp") {
			Headers = new HeaderDictionary(),
			ContentType = "image/webp"
		};
	}

	[HttpDelete("courses/{uuid:guid}/image")]
	public async Task<IActionResult> DeleteCourseImage([FromRoute] Guid uuid, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();

		var existingCourse = await db.CoursesMinimalEf()
			.FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
		if (existingCourse == null) return NotFound();

		if (acc is not Admin && existingCourse.LecturerUuid != acc.Uuid) return Forbid();

		if (existingCourse.Status != CourseStatus.Draft) return BadRequest(new { error = "Only courses in draft status can be updated." });

		if (string.IsNullOrEmpty(existingCourse.ImageUrl)) {
			return NotFound(new { error = "Course image not found." });
		}

		await materialAccessService.DeleteFileMaterialAsync(existingCourse.ImageUrl, ct);
		existingCourse.ImageUrl = null;
		existingCourse.UpdatedAt = DateTime.UtcNow;

		var entry = db.Entry(existingCourse);
		if (entry.State == EntityState.Detached) {
			db.Courses.Update(existingCourse);
		}
		await db.SaveChangesAsync(ct);

		return NoContent();
	}

	[HttpGet("courses/{uuid:guid}/image")]
	public async Task<IActionResult> GetCourseImage([FromRoute] Guid uuid, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);

		var courseImageData = await db.Courses
			.AsNoTracking()
			.Where(c => c.Uuid == uuid)
			.Select(c => new { Exists = true, c.ImageUrl, c.LecturerUuid, c.Status })
			.FirstOrDefaultAsync(ct);

		if (courseImageData == null) return NotFound(new { error = "Course not found." });

		var course = new Course {
			LecturerUuid = courseImageData.LecturerUuid,
			Status = courseImageData.Status
		};

		var accessResult = ValidateRestrictedCourseAccess(course, acc);
		if (accessResult != null) {
			return accessResult;
		}

		if (string.IsNullOrEmpty(courseImageData.ImageUrl)) {
			return NotFound(new { error = "Course image not found." });
		}

		var imageStream = await materialAccessService.DownloadFileMaterialAsync(courseImageData.ImageUrl, ct);
		
		if (imageStream == null) {
			return NotFound(new { error = "Course image not found." });
		}
		
		imageStream.Position = 0;

		// Determine content type based on file extension
		var contentType = "application/octet-stream"; // Default content type
		var extension = Path.GetExtension(courseImageData.ImageUrl).ToLowerInvariant();
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

		var existingCourse = await db.CoursesFullEf()
			.FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
		if (existingCourse == null) return NotFound();

		if (acc is not Admin && existingCourse.LecturerUuid != acc.Uuid) return Forbid();

		if (existingCourse.Status != CourseStatus.Draft) return BadRequest(new { error = "Only courses in draft status can be updated." });

		// Remove quizzes and materials that belong to modules first (to avoid FK constraint violations)
		foreach (var module in existingCourse.Modules) {
			db.Quizzes.RemoveRange(module.Quizzes);
			db.Materials.RemoveRange(module.Materials);
		}
		db.CourseModules.RemoveRange(existingCourse.Modules);

		existingCourse.Materials = [];
		existingCourse.Quizzes = [];
		existingCourse.Feed = [];
		if (existingCourse.Account != null) existingCourse.Account.Ratings = [];

		db.Courses.Remove(existingCourse);
		await db.SaveChangesAsync(ct);

		return NoContent();
	}

	[HttpPost("courses/{uuid:guid}/rating")]
	public async Task<IActionResult> LikeOrDislikeCourse([FromRoute] Guid uuid, [FromBody] LikeCourseRequest body, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();

		var course = await db.CoursesMinimalEf()
			.AsNoTracking()
			.AsSplitQuery()
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
					UpdatedAt = DateTime.UtcNow
				};

				db.Likes.Add(newRating);
			} break;

			case "dislike": {
				var newRating = new Dislike {
					AccountUuid = acc.Uuid,
					CourseUuid = course.Uuid,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
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
		var acc = await auth.ReAuthAsync(ct);

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
		var course = await db.CoursesMinimalEf()
			.FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
		if (course == null) {
			return NotFound(new { error = "Course not found." });
		}

		if (course.Status == CourseStatus.Archived || course.Status == CourseStatus.Paused || course.Status == CourseStatus.Draft) {
			if (acc == null) return Unauthorized();
			if (course.LecturerUuid != acc.Uuid && acc is not Admin) return Forbid();
		}

		course.Materials = [];
		course.Quizzes = [];
		course.Feed = [];
		if (course.Account != null) course.Account.Ratings = [];

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
		if (!UsedIPsForCourse.ContainsKey(course.Uuid)) UsedIPsForCourse.Add(course.Uuid, new List<IPAddress>());
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

	[HttpPut("courses/{uuid:guid}/status")]
	public async Task<IActionResult> UpdateCourseStatus(Guid uuid, [FromBody] UpdateCourseStatusRequest body, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();
		var existingCourse = await db.CoursesMinimalEf()
			.FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
		
		if (body.Status == null && body.ScheduledStart == null) {
			return BadRequest(new { error = "At least one of 'status' or 'scheduledStart' must be provided." });
		}
		
		if (existingCourse == null) return NotFound();
		
		if (acc is not Admin && existingCourse.LecturerUuid != acc.Uuid) return Forbid();
		
		if (body.Status.HasValue && existingCourse.Status == body.Status.Value && body.ScheduledStart == existingCourse.ScheduledStart) {
			return BadRequest(new { error = "Course is already in the specified status." });
		}
		
		if (body.Status.HasValue) existingCourse.Status = body.Status.Value;
		if (body.ScheduledStart != null) existingCourse.ScheduledStart = body.ScheduledStart;
		
		await db.SaveChangesAsync(ct);
		await sb.PublishAsync(
			existingCourse.Uuid,
			new StreamMessage("status_changed", new { status = existingCourse.Status.ToString().ToLower() }),
			ct
		);

		return Ok(existingCourse);
	}
	
	[HttpPost("courses/{uuid:guid}/module-order")]
	public async Task<IActionResult> UpdateCourseModuleOrder(Guid uuid, [FromBody] UpdateCourseModuleOrderRequest body, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();

		var existingCourse = await db.CoursesFullEf()
			.FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
		if (existingCourse == null) return NotFound();

		if (acc is not Admin && existingCourse.LecturerUuid != acc.Uuid) return Forbid();

		if (existingCourse.Status != CourseStatus.Draft) return BadRequest(new { error = "Only courses in draft status can be updated." });

		foreach (var module in body.ModuleOrders) {
			if (module.ModuleType == "material") {
				var existingMaterial = existingCourse.Materials.FirstOrDefault(m => m.Uuid == module.Uuid);
				
				if (existingMaterial == null)
					return BadRequest(new { error = $"Invalid material UUID: {module.Uuid}" });
				
				existingMaterial.Order = module.Order;
			}

			if (module.ModuleType == "quiz") {
				var existingQuiz = existingCourse.Quizzes.FirstOrDefault(q => q.Uuid == module.Uuid);
				
				if (existingQuiz == null)
					return BadRequest(new { error = $"Invalid quiz UUID: {module.Uuid}" });
				
				existingQuiz.Order = module.Order;
			}
		}

		var affected = await db.SaveChangesAsync(ct);
		return Ok(new { affected });
	}

	#region course modules

	[HttpGet("courses/{courseUuid:guid}/modules")]
	public async Task<IActionResult> GetCourseModules([FromRoute] Guid courseUuid, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);

		var course = await db.CoursesMinimalEf()
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
		if (course == null) return NotFound(new { error = "Course not found." });

		var accessResult = ValidateRestrictedCourseAccess(course, acc);
		if (accessResult != null) return accessResult;

		var ownsCourse = acc != null && course.LecturerUuid == acc.Uuid;
		var isAdmin = acc is Admin;

		var modules = await db.CourseModules
			.Where(m => m.CourseUuid == courseUuid)
			.Include(m => m.Materials.OrderBy(mat => mat.Order))
			.Include(m => m.Quizzes.OrderBy(q => q.Order))
			.OrderBy(m => m.Order)
			.AsNoTracking()
			.ToListAsync(ct);

		var result = modules
			.Where(m => m.IsVisible || ownsCourse || isAdmin)
			.Select(m => new ReadModuleResponse {
				Uuid = m.Uuid,
				Title = m.Title,
				Description = m.Description,
				IsVisible = m.IsVisible,
				Order = m.Order,
				CreatedAt = m.CreatedAt,
				UpdatedAt = m.UpdatedAt,
				Materials = (ownsCourse || isAdmin
					? m.Materials
					: m.Materials.Where(mat => mat.IsVisible).ToList())
					.OrderBy(mat => mat.Order)
					.Select(mat => mat.ToReadDto())
					.ToList(),
				Quizzes = (ownsCourse || isAdmin
					? m.Quizzes
					: m.Quizzes.Where(q => q.IsVisible).ToList())
					.OrderBy(q => q.Order)
					.Select(q => q.ToReadDto())
					.ToList()
			})
			.ToList();

		return Ok(result);
	}

	[HttpPost("courses/{courseUuid:guid}/modules")]
	public async Task<IActionResult> CreateCourseModule([FromRoute] Guid courseUuid, [FromBody] CreateModuleRequest body, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();

		var course = await db.CoursesMinimalEf()
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
		if (course == null) return NotFound(new { error = "Course not found." });

		if (acc is not Admin && course.LecturerUuid != acc.Uuid) return Forbid();

		if (string.IsNullOrWhiteSpace(body.Title))
			return BadRequest(new { error = "Title is required." });

		var module = new CourseModule {
			Title = body.Title,
			Description = body.Description,
			IsVisible = body.IsVisible,
			Order = body.Order,
			CourseUuid = courseUuid
		};

		db.CourseModules.Add(module);
		await db.SaveChangesAsync(ct);

		return CreatedAtAction(nameof(GetCourseModuleById), new { courseUuid, moduleUuid = module.Uuid }, new ReadModuleResponse {
			Uuid = module.Uuid,
			Title = module.Title,
			Description = module.Description,
			IsVisible = module.IsVisible,
			Order = module.Order,
			CreatedAt = module.CreatedAt,
			UpdatedAt = module.UpdatedAt,
			Materials = [],
			Quizzes = []
		});
	}

	[HttpGet("courses/{courseUuid:guid}/modules/{moduleUuid:guid}")]
	public async Task<IActionResult> GetCourseModuleById([FromRoute] Guid courseUuid, [FromRoute] Guid moduleUuid, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);

		var course = await db.CoursesMinimalEf()
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
		if (course == null) return NotFound(new { error = "Course not found." });

		var accessResult = ValidateRestrictedCourseAccess(course, acc);
		if (accessResult != null) return accessResult;

		var module = await db.CourseModules
			.Where(m => m.CourseUuid == courseUuid && m.Uuid == moduleUuid)
			.Include(m => m.Materials.OrderBy(mat => mat.Order))
			.Include(m => m.Quizzes.OrderBy(q => q.Order))
			.AsNoTracking()
			.FirstOrDefaultAsync(ct);
		if (module == null) return NotFound(new { error = "Module not found." });

		var ownsCourse = acc != null && course.LecturerUuid == acc.Uuid;
		var isAdmin = acc is Admin;

		if (!module.IsVisible && !ownsCourse && !isAdmin)
			return NotFound(new { error = "Module not found." });

		return Ok(new ReadModuleResponse {
			Uuid = module.Uuid,
			Title = module.Title,
			Description = module.Description,
			IsVisible = module.IsVisible,
			Order = module.Order,
			CreatedAt = module.CreatedAt,
			UpdatedAt = module.UpdatedAt,
			Materials = (ownsCourse || isAdmin
				? module.Materials
				: module.Materials.Where(mat => mat.IsVisible).ToList())
				.OrderBy(mat => mat.Order)
				.Select(mat => mat.ToReadDto())
				.ToList(),
			Quizzes = (ownsCourse || isAdmin
				? module.Quizzes
				: module.Quizzes.Where(q => q.IsVisible).ToList())
				.OrderBy(q => q.Order)
				.Select(q => q.ToReadDto())
				.ToList()
		});
	}

	[HttpPut("courses/{courseUuid:guid}/modules/{moduleUuid:guid}")]
	public async Task<IActionResult> UpdateCourseModule([FromRoute] Guid courseUuid, [FromRoute] Guid moduleUuid, [FromBody] UpdateModuleRequest body, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();

		var course = await db.CoursesMinimalEf()
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
		if (course == null) return NotFound(new { error = "Course not found." });

		if (acc is not Admin && course.LecturerUuid != acc.Uuid) return Forbid();

		var module = await db.CourseModules
			.Where(m => m.CourseUuid == courseUuid && m.Uuid == moduleUuid)
			.FirstOrDefaultAsync(ct);
		if (module == null) return NotFound(new { error = "Module not found." });

		if (!string.IsNullOrWhiteSpace(body.Title)) module.Title = body.Title;
		if (body.Description != null) module.Description = body.Description;
		if (body.IsVisible.HasValue) module.IsVisible = body.IsVisible.Value;
		if (body.Order.HasValue) module.Order = body.Order.Value;
		module.UpdatedAt = DateTime.UtcNow;

		db.CourseModules.Update(module);
		await db.SaveChangesAsync(ct);

		if (body.IsVisible.HasValue) {
			var newFeedPost = new FeedPost {
				Uuid = Guid.NewGuid(),
				CourseUuid = courseUuid,
				Type = FeedPost.FeedPostType.System,
				Message = $"Viditelnost modulu '{module.Title}' byla změněna na {(body.IsVisible == true ? "viditelný" : "skrytý")}.",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
				Purpose = body.IsVisible == true ? FeedPost.FeedPurpose.ShowModule : FeedPost.FeedPurpose.HideModule
			};

			await fsb.PublishAsync(courseUuid, new FeedStreamMessage("new_post", newFeedPost), ct);
			await db.FeedPosts.AddAsync(newFeedPost, ct);
			await db.SaveChangesAsync(ct);

			await sb.PublishAsync(courseUuid, new StreamMessage("module_visibility_changed", new { moduleUuid = module.Uuid, isVisible = module.IsVisible, moduleTitle = module.Title }), ct);
		}

		return Ok(new ReadModuleResponse {
			Uuid = module.Uuid,
			Title = module.Title,
			Description = module.Description,
			IsVisible = module.IsVisible,
			Order = module.Order,
			CreatedAt = module.CreatedAt,
			UpdatedAt = module.UpdatedAt,
			Materials = [],
			Quizzes = []
		});
	}

	[HttpDelete("courses/{courseUuid:guid}/modules/{moduleUuid:guid}")]
	public async Task<IActionResult> DeleteCourseModule([FromRoute] Guid courseUuid, [FromRoute] Guid moduleUuid, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();

		var course = await db.CoursesMinimalEf()
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
		if (course == null) return NotFound(new { error = "Course not found." });

		if (acc is not Admin && course.LecturerUuid != acc.Uuid) return Forbid();

		var module = await db.CourseModules
			.Where(m => m.CourseUuid == courseUuid && m.Uuid == moduleUuid)
			.Include(m => m.Materials)
			.Include(m => m.Quizzes)
			.FirstOrDefaultAsync(ct);
		if (module == null) return NotFound(new { error = "Module not found." });

		// Unlink items from module before deleting
		foreach (var material in module.Materials) {
			material.ModuleUuid = null;
			db.Materials.Update(material);
		}
		foreach (var quiz in module.Quizzes) {
			quiz.ModuleUuid = null;
			db.Quizzes.Update(quiz);
		}

		db.CourseModules.Remove(module);
		await db.SaveChangesAsync(ct);

		return NoContent();
	}

	[HttpPut("courses/{courseUuid:guid}/modules/order")]
	public async Task<IActionResult> ReorderCourseModules([FromRoute] Guid courseUuid, [FromBody] ReorderModulesRequest body, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();

		var course = await db.CoursesMinimalEf()
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
		if (course == null) return NotFound(new { error = "Course not found." });

		if (acc is not Admin && course.LecturerUuid != acc.Uuid) return Forbid();

		var moduleUuids = body.Modules.Select(m => m.Uuid).ToList();
		var modules = await db.CourseModules
			.Where(m => m.CourseUuid == courseUuid && moduleUuids.Contains(m.Uuid))
			.ToListAsync(ct);

		if (modules.Count != body.Modules.Count)
			return BadRequest(new { error = "One or more module UUIDs are invalid." });

		var strategy = db.Database.CreateExecutionStrategy();
		await strategy.ExecuteAsync(async () => {
			await using var transaction = await db.Database.BeginTransactionAsync(ct);
			try {
				foreach (var item in body.Modules) {
					var module = modules.First(m => m.Uuid == item.Uuid);
					module.Order = item.Order;
					db.CourseModules.Update(module);
				}
				await db.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
			} catch {
				await transaction.RollbackAsync(ct);
				throw;
			}
		});

		return Ok(new { affected = modules.Count });
	}

	[HttpPost("courses/{courseUuid:guid}/modules/{moduleUuid:guid}/items/reorder")]
	public async Task<IActionResult> ReorderModuleItems([FromRoute] Guid courseUuid, [FromRoute] Guid moduleUuid, [FromBody] UpdateCourseModuleOrderRequest body, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();

		var course = await db.CoursesMinimalEf()
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
		if (course == null) return NotFound(new { error = "Course not found." });

		if (acc is not Admin && course.LecturerUuid != acc.Uuid) return Forbid();

		var module = await db.CourseModules
			.Where(m => m.CourseUuid == courseUuid && m.Uuid == moduleUuid)
			.Include(m => m.Materials)
			.Include(m => m.Quizzes)
			.FirstOrDefaultAsync(ct);
		if (module == null) return NotFound(new { error = "Module not found." });

		IActionResult? validationError = null;
		var strategy = db.Database.CreateExecutionStrategy();
		await strategy.ExecuteAsync(async () => {
			await using var transaction = await db.Database.BeginTransactionAsync(ct);
			try {
				foreach (var item in body.ModuleOrders) {
					if (item.ModuleType == "material") {
						var material = module.Materials.FirstOrDefault(m => m.Uuid == item.Uuid);
						if (material == null) {
							validationError = BadRequest(new { error = $"Invalid material UUID: {item.Uuid}" });
							return;
						}
						material.Order = item.Order;
						db.Materials.Update(material);
					} else if (item.ModuleType == "quiz") {
						var quiz = module.Quizzes.FirstOrDefault(q => q.Uuid == item.Uuid);
						if (quiz == null) {
							validationError = BadRequest(new { error = $"Invalid quiz UUID: {item.Uuid}" });
							return;
						}
						quiz.Order = item.Order;
						db.Quizzes.Update(quiz);
					}
				}
				await db.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
			} catch {
				await transaction.RollbackAsync(ct);
				throw;
			}
		});

		if (validationError != null) return validationError;
		return Ok(new { affected = body.ModuleOrders.Count });
	}

	[HttpPut("courses/{courseUuid:guid}/items/assign-module")]
	public async Task<IActionResult> AssignItemToModule([FromRoute] Guid courseUuid, [FromBody] AssignItemToModuleRequest body, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();

		var course = await db.CoursesMinimalEf()
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
		if (course == null) return NotFound(new { error = "Course not found." });

		if (acc is not Admin && course.LecturerUuid != acc.Uuid) return Forbid();

		if (course.Status != CourseStatus.Draft)
			return BadRequest(new { error = "Only courses in draft status can be updated." });

		// Validate target module if provided
		if (body.ModuleUuid.HasValue) {
			var moduleExists = await db.CourseModules
				.AnyAsync(m => m.Uuid == body.ModuleUuid.Value && m.CourseUuid == courseUuid, ct);
			if (!moduleExists)
				return BadRequest(new { error = "Invalid module UUID." });
		}

		if (body.ItemType == "material") {
			var material = await db.Materials
				.FirstOrDefaultAsync(m => m.Uuid == body.ItemUuid && m.CourseUuid == courseUuid, ct);
			if (material == null) return NotFound(new { error = "Material not found." });
			material.ModuleUuid = body.ModuleUuid;
			material.UpdatedAt = DateTime.UtcNow;
			db.Materials.Update(material);
		} else if (body.ItemType == "quiz") {
			var quiz = await db.Quizzes
				.FirstOrDefaultAsync(q => q.Uuid == body.ItemUuid && q.CourseUuid == courseUuid, ct);
			if (quiz == null) return NotFound(new { error = "Quiz not found." });
			quiz.ModuleUuid = body.ModuleUuid;
			quiz.UpdatedAt = DateTime.UtcNow;
			db.Quizzes.Update(quiz);
		} else {
			return BadRequest(new { error = "Invalid item type. Use 'material' or 'quiz'." });
		}

		await db.SaveChangesAsync(ct);
		return Ok(new { itemUuid = body.ItemUuid, moduleUuid = body.ModuleUuid });
	}

	#endregion

	[HttpPost("courses")]
	public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest body, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();

		if (string.IsNullOrEmpty(body.Name) || string.IsNullOrEmpty(body.Description)) {
			return BadRequest(new { error = "Name and description are required." });
		}

		if (acc is not Lecturer and not Admin) {
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
			nameof(GetCourseById),
			"APIv1",
			new { uuid = newCourse.Uuid },
			newCourse
		);
	}

	[HttpPost("courses")]
	[Consumes("multipart/form-data")]
	public async Task<IActionResult> CreateCourseWithMaterials([FromForm] CreateCourseWithMaterialsRequest body, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();

		if (string.IsNullOrEmpty(body.Course.Name) || string.IsNullOrEmpty(body.Course.Description)) {
			return BadRequest(new { error = "Course name and description are required." });
		}

		if (acc is not Lecturer and not Admin) {
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

		var course = await db.CoursesMinimalEf()
			.AsNoTracking()
			.AsSplitQuery()
			.FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
		if (course == null) {
			return NotFound(new { error = "Course not found." });
		}

		if (acc is not Admin && course.LecturerUuid != acc.Uuid) return Forbid();

		if (course.Status != CourseStatus.Draft) return BadRequest(new { error = "Only courses in draft status can be updated." });

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

		if (body.ModuleUuid.HasValue) {
			var module = await db.CourseModules
				.FirstOrDefaultAsync(m => m.Uuid == body.ModuleUuid.Value && m.CourseUuid == course.Uuid, ct);
			if (module == null)
				return BadRequest(new { error = "Invalid module UUID." });
			newMaterial.ModuleUuid = module.Uuid;
		}

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

		//db.FeedPosts.Add(post);
		await db.SaveChangesAsync(ct);

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

		var course = await db.CoursesMinimalEf()
			.AsNoTracking()
			.AsSplitQuery()
			.FirstOrDefaultAsync(c => c.Uuid == courseId, ct);
		
		if (course == null) {
			return NotFound(new { error = "Course not found." });
		}

		if (acc is not Admin && course.LecturerUuid != acc.Uuid) return Forbid();

		if (course.Status != CourseStatus.Draft) return BadRequest(new { error = "Only courses in draft status can be updated." });

		if (body.Type != "file") {
			return BadRequest(new { error = "Only 'file' material type is supported in this endpoint." });
		}

		if (body.File == null) {
			return BadRequest(new { error = "File is required." });
		}

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

		if (body.ModuleUuid.HasValue) {
			var module = await db.CourseModules
				.FirstOrDefaultAsync(m => m.Uuid == body.ModuleUuid.Value && m.CourseUuid == course.Uuid, ct);
			if (module == null)
				return BadRequest(new { error = "Invalid module UUID." });
			newMaterial.ModuleUuid = module.Uuid;
		}

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

		//db.FeedPosts.Add(post);
		await db.SaveChangesAsync(ct);

		return CreatedAtAction(nameof(GetCourseById), new { uuid = course.Uuid }, responseObj);
	}

	[HttpGet("courses/{courseUuid:guid}/materials/{materialUuid:guid}")]
	public async Task<IActionResult> GetCourseMaterialById([FromRoute] Guid courseUuid, [FromRoute] Guid materialUuid, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);

		var course = await db.CoursesFullEf()
			.AsNoTracking()
			.AsSplitQuery()
			.FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);

		if (course == null) {
			return NotFound(new { error = "Course not found." });
		}
		
		var isOwnerOrAdmin = acc != null && (course.LecturerUuid == acc.Uuid || acc is Admin);

		Material? material = null;

		var module = course.Modules.FirstOrDefault(m => m.Materials.Any(mat => mat.Uuid == materialUuid));
		
		if (isOwnerOrAdmin) {
			material = module?.Materials.FirstOrDefault(m => m.Uuid == materialUuid) ?? course.Materials.FirstOrDefault(m => m.Uuid == materialUuid);
		}
		else {
			material = module?.Materials.FirstOrDefault(m => m.Uuid == materialUuid);
		

			if (module == null && !isOwnerOrAdmin || material == null || module == null) {
				return NotFound(new { error = "Material not found." });
			}

			if (!module.IsVisible) {
				if (acc == null) return Unauthorized();
				if (course.LecturerUuid != acc.Uuid && acc is not Admin) return Forbid();
			}
		}

		var accessResult = ValidateRestrictedCourseAccess(course, acc);
		if (accessResult != null) {
			return accessResult;
		}
		
		if (material == null)
			return NotFound(new { error = "Material not found." });

		switch (material) {
			case UrlMaterial urlMaterial:
				return Ok(urlMaterial.ToReadDto());
			case FileMaterial fileMaterial:
				try {
					var recaptchaToken = Request.Query["recaptchaToken"].ToString();
					if (string.IsNullOrWhiteSpace(recaptchaToken)) {
						return BadRequest(new { error = "Missing reCAPTCHA token." });
					}

					using var requestMessage = new HttpRequestMessage(
						HttpMethod.Post,
						"https://www.google.com/recaptcha/api/siteverify"
					);

					var formData = new FormUrlEncodedContent(new Dictionary<string, string> {
						{ "secret", Program.ENV.GetValueOrNull("RECAPTCHA_SECRET_KEY") ?? "x" },
						{ "response", recaptchaToken }
					});

					requestMessage.Content = formData;

					var response = await HttpClient.SendAsync(requestMessage, ct);
					var responseContent = await response.Content.ReadAsStringAsync(ct);

					var jsonResponse = JsonNode.Parse(responseContent);
					if (jsonResponse == null || jsonResponse["success"]?.GetValue<bool>() != true) {
						return BadRequest(new { error = "Recaptcha verification failed." });
					}
					
					var memoryStream = await materialAccessService.DownloadFileMaterialAsync(fileMaterial.FileUrl, ct);
					
					if (memoryStream == null) {
						return StatusCode(500, new { error = "Error fetching file from storage." });
					}
					
					var now = DateTime.UtcNow;

					await db.Materials
						.OfType<FileMaterial>()
						.Where(m => m.Uuid == fileMaterial.Uuid && m.CourseUuid == courseUuid)
						.ExecuteUpdateAsync(setters => setters
								.SetProperty(m => m.DownloadCount, m => m.DownloadCount + 1)
								.SetProperty(m => m.LastDownload, _ => now)
								.SetProperty(m => m.TotalBytesDownloaded, m => m.TotalBytesDownloaded + m.SizeBytes),
							ct
						);
					
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
						if (idx >= 0 && idx < fileMaterial.FileUrl.Length - 1) {
							extension = fileMaterial.FileUrl[idx..];
						}
					}

					var fileName = string.IsNullOrEmpty(extension) ? baseName : $"{baseName}{extension}";

					Response.Headers.ContentDisposition = "inline";

					// If mime type can be shown in browser, do not force download
					return File(memoryStream, fileMaterial.MimeType ?? "application/octet-stream", fileMaterial.MimeType == null || fileMaterial.MimeType == "application/octet-stream" ? fileName : null);
				} catch (Minio.Exceptions.MinioException e) {
					return StatusCode(500, new { error = "Error fetching file from storage.", detail = e.Message });
				}
			default:
				return StatusCode(500, new { error = "Unknown material type." });

		}
	}

	[Consumes("application/json")]
	[HttpPost("courses/{courseUuid:guid}/materials/{materialUuid:guid}/track-click")]
	public async Task<IActionResult> TrackUrlClick(
		Guid courseUuid,
		Guid materialUuid,
		[FromBody] IDictionary<string, string> body,
		CancellationToken ct = default
	) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();

		var recaptchaToken = body.TryGetValue("recaptchaToken", out var _value) ? _value : null;
		if(string.IsNullOrEmpty(recaptchaToken)) {
			return BadRequest(new { error = "Invalid reCAPTCHA token." });
		}

		// overeni captchy
		using var requestMessage = new HttpRequestMessage(
			HttpMethod.Post,
			"https://www.google.com/recaptcha/api/siteverify"
		);

		var formData = new FormUrlEncodedContent(new Dictionary<string, string> {
			{ "secret", Program.ENV.GetValueOrNull("RECAPTCHA_SECRET_KEY") ?? "x" },
			{ "response", recaptchaToken }
		});

		requestMessage.Content = formData;

		var response = await HttpClient.SendAsync(requestMessage, ct);
		var responseContent = await response.Content.ReadAsStringAsync(ct);

		// overeni captcha
		var jsonResponse = JsonNode.Parse(responseContent);
		if (jsonResponse == null || jsonResponse["success"]?.GetValue<bool>() != true) {
			return BadRequest(new { error = "Recaptcha verification failed." });
		}

		var now = DateTime.UtcNow;

		var updated = await db.Materials
			.OfType<UrlMaterial>()
			.Where(m => m.CourseUuid == courseUuid && m.Uuid == materialUuid)
			.ExecuteUpdateAsync(setters => setters
					.SetProperty(m => m.ClickCount, m => m.ClickCount + 1)
					.SetProperty(m => m.LastClickedAt, _ => now),
				ct);

		if (updated == 0) return NotFound(new { error = "URL material not found." });
		return Ok(new { success = true });
	}
	
	[HttpDelete("courses/{courseUuid:guid}/materials/{materialUuid:guid}")]
	public async Task<IActionResult> DeleteCourseMaterialById([FromRoute] Guid courseUuid, [FromRoute] Guid materialUuid, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();

		var existingCourse = await db.Courses
			.Select(c => new { c.Uuid, c.LecturerUuid, c.Status })
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
		if (existingCourse == null) return NotFound();

		if (acc is not Admin && existingCourse.LecturerUuid != acc.Uuid) return Forbid();

		if (existingCourse.Status != CourseStatus.Draft) return BadRequest(new { error = "Only courses in draft status can be updated." });

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

		//db.FeedPosts.Add(newFeedPost);
		await db.SaveChangesAsync(ct);

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


		var course = await db.CoursesMinimalEf()
			.FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
		if (course == null) {
			return NotFound(new { error = "Course not found." });
		}

		if (acc is not Admin && course.LecturerUuid != acc.Uuid) return Forbid();

		if (course.Status != CourseStatus.Draft) return BadRequest(new { error = "Only courses in draft status can be updated." });

		var material = await db.Materials
			.FirstOrDefaultAsync(m => m.Uuid == materialUuid, ct);

		switch (material) {
			case UrlMaterial urlMaterial:
				if (!string.IsNullOrEmpty(body.Name)) {
					urlMaterial.Name = body.Name;
				}

				if (body.Description != null) {
					urlMaterial.Description = body.Description;
				}

				if (!string.IsNullOrEmpty(body.Url)) {
					urlMaterial.Url = body.Url;
					urlMaterial.FaviconUrl = $"https://www.google.com/s2/favicons?domain={new Uri(body.Url).Host}&sz=64";
				}

				if (body.IsVisible.HasValue) {
					urlMaterial.IsVisible = body.IsVisible.Value;
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

				//db.FeedPosts.Add(newFeedPost);
				await db.SaveChangesAsync(ct);

				return Ok(urlMaterial.ToReadDto());

			case FileMaterial fileMaterial:
				if (!string.IsNullOrEmpty(body.Name)) {
					fileMaterial.Name = body.Name;
				}

				if (body.Description != null) {
					fileMaterial.Description = body.Description;
				}

				if (body.IsVisible.HasValue) {
					fileMaterial.IsVisible = body.IsVisible.Value;
				}

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


		var course = await db.CoursesMinimalEf()
			.FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
		if (course == null) {
			return NotFound(new { error = "Course not found." });
		}

		if (acc is not Admin && course.LecturerUuid != acc.Uuid) return Forbid();

		if (course.Status != CourseStatus.Draft) return BadRequest(new { error = "Only courses in draft status can be updated." });

		var material = await db.Materials
			.FirstOrDefaultAsync(m => m.Uuid == materialUuid, ct);

		if (material == null || material.CourseUuid != course.Uuid) {
			return NotFound(new { error = "Material not found in the specified course." });
		}

		if (material is not FileMaterial fileMaterial) {
			return BadRequest(new { error = "Material is not of type 'file'." });
		}

		if (!string.IsNullOrEmpty(body.Name)) {
			fileMaterial.Name = body.Name;
		}
		
		if (Request.Form.ContainsKey("Description")) {
			fileMaterial.Description = string.IsNullOrWhiteSpace(body.Description) ? null : body.Description;
		}

		if (body.IsVisible.HasValue) {
			fileMaterial.IsVisible = body.IsVisible.Value;
		}

		if (body.File is { Length: > 0 }) {
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

		//db.FeedPosts.Add(newFeedPost);
		await db.SaveChangesAsync(ct);

		return Ok(fileMaterial.ToReadDto());
	}

	[HttpPut("courses/{courseUuid:guid}/materials/{materialUuid:guid}/visibility")]
	public async Task<IActionResult> UpdateMaterialVisibility(
		[FromRoute] Guid courseUuid,
		[FromRoute] Guid materialUuid,
		[FromBody] UpdateModuleVisibilityRequest body
	) {
		var material = await db.Materials
			.Where(m => m.CourseUuid == courseUuid)
			.Where(m => m.Uuid == materialUuid)
			.FirstOrDefaultAsync();
		
		if (material == null) return NotFound(new { error = "Material not found." });

		material.IsVisible = body.IsVisible;
		material.UpdatedAt = DateTime.UtcNow;
		
		db.Materials.Update(material);
		await db.SaveChangesAsync();
		
		//odesilani info do sse
		var newFeedPost = new FeedPost {
			Uuid = Guid.NewGuid(),
			CourseUuid = courseUuid,
			Type = FeedPost.FeedPostType.System,
			Message = $"Viditelnost materiálu '{material.Name}' byla změněna na {(body.IsVisible ? "viditelný" : "skrytý")}.",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			Purpose = body.IsVisible ? FeedPost.FeedPurpose.ShowMaterial : FeedPost.FeedPurpose.HideMaterial
		};
		
		db.FeedPosts.Add(newFeedPost);
		await db.SaveChangesAsync();
		await fsb.PublishAsync(courseUuid, new FeedStreamMessage("new_post", newFeedPost));
		await sb.PublishAsync(courseUuid, new StreamMessage("material_visibility_changed", new { materialUuid = material.Uuid, isVisible = material.IsVisible }));

		return Ok(new { quizUuid = material.Uuid, isVisible = material.IsVisible });
	}
	
	[HttpGet("courses/{courseUuid:guid}/materials/{materialUuid:guid}/url-stats")]
	public async Task<IActionResult> GetUrlMaterialStats(
		[FromRoute] Guid courseUuid,
		[FromRoute] Guid materialUuid,
		CancellationToken ct = default
	) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();

		var course = await db.Courses
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);

		if (course == null)
			return NotFound(new { error = "Course not found." });

		// jen lecturer/admin
		if (acc is not Admin && course.LecturerUuid != acc.Uuid)
			return Forbid();

		var accessResult = ValidateRestrictedCourseAccess(course, acc);
		if (accessResult != null)
			return accessResult;

		var urlMaterial = await db.Materials
			.OfType<UrlMaterial>()
			.AsNoTracking()
			.FirstOrDefaultAsync(m => 
				m.CourseUuid == courseUuid && 
				m.Uuid == materialUuid, ct);

		if (urlMaterial == null)
			return NotFound(new { error = "URL material not found." });

		return Ok(new {
			materialUuid = urlMaterial.Uuid,
			type = "url",
			clickCount = urlMaterial.ClickCount,
			lastClickedAt = urlMaterial.LastClickedAt
		});
	}
	
	[HttpGet("courses/{courseUuid:guid}/materials/{materialUuid:guid}/file-stats")]
	public async Task<IActionResult> GetFileMaterialStats(
	    [FromRoute] Guid courseUuid,
	    [FromRoute] Guid materialUuid,
	    CancellationToken ct = default
	) {
	    var acc = await auth.ReAuthAsync(ct);
	    if (acc == null) return Unauthorized();

	    var course = await db.Courses
	        .AsNoTracking()
	        .Where(c => c.Uuid == courseUuid)
	        .Select(c => new { c.Uuid, c.LecturerUuid, c.Status })
	        .FirstOrDefaultAsync(ct);

	    if (course == null) return NotFound(new { error = "Course not found." });

	    // jen lecturer/admin
	    if (acc is not Admin && course.LecturerUuid != acc.Uuid) return Forbid();

	    // restricted access pravidla (pokud používáš)
	    var accessResult = ValidateRestrictedCourseAccess(await db.Courses
	        .AsNoTracking()
	        .FirstAsync(c => c.Uuid == courseUuid, ct), acc);
	    if (accessResult != null) return accessResult;

	    var material = await db.Materials
	        .AsNoTracking()
	        .Where(m => m.CourseUuid == courseUuid && m.Uuid == materialUuid)
	        .FirstOrDefaultAsync(ct);

	    if (material == null) return NotFound(new { error = "Material not found." });

	    if (material is not FileMaterial fileMaterial)
	        return BadRequest(new { error = "Material is not of type 'file'." });

	    var downloads = fileMaterial.DownloadCount;

	    // TotalBytesDownloaded máš jako int — radši počítat v longu
	    long totalBytesDownloaded = fileMaterial.TotalBytesDownloaded;
	    if (totalBytesDownloaded <= 0 && downloads > 0 && fileMaterial.SizeBytes > 0) {
	        totalBytesDownloaded = (long)downloads * fileMaterial.SizeBytes;
	    }

	    var totalMb = totalBytesDownloaded / (1024.0 * 1024.0);
	    var avgMbPerDownload = downloads > 0 ? totalMb / downloads : 0;

	    return Ok(new {
	        materialUuid = fileMaterial.Uuid,
	        type = "file",
	        sizeBytes = fileMaterial.SizeBytes,
	        downloadCount = downloads,
	        lastDownloadedAt = fileMaterial.LastDownload, // doporučuju přejmenovat na LastDownloadedAt
	        totalBytesDownloaded,
	        totalMegabytesDownloaded = totalMb,
	        averageMegabytesPerDownload = avgMbPerDownload
	    });
	}

	#endregion

	
	#region course Kvizy

	[HttpGet("courses/{courseUuid:guid}/quizzes/{quizUuid:guid}")]
	public async Task<IActionResult> GetQuizById([FromRoute] Guid courseUuid, [FromRoute] Guid quizUuid) {
		var acc = await auth.ReAuthAsync();

		var course = await db.CoursesMinimalEf()
			.AsNoTracking()
			.AsSplitQuery()
			.FirstOrDefaultAsync(c => c.Uuid == courseUuid);

		if (course == null) {
			return NotFound(new { error = "Course not found." });
		}

		var accessResult = ValidateRestrictedCourseAccess(course, acc);
		if (accessResult != null) {
			return accessResult;
		}

		var quiz = await db.QuizzesEf()
			.AsNoTracking()
			.Where(q => q.CourseUuid == courseUuid)
			.Where(q => q.Uuid == quizUuid)
			.FirstOrDefaultAsync();

		if (quiz == null || quiz.CourseUuid != course.Uuid) {
			return NotFound(new { error = "Quiz not found in the specified course." });
		}

		// if (quiz.IsVisible == false) {
		// 	if (acc == null) return Unauthorized();
		// 	if (course.LecturerUuid != acc.Uuid && acc is not Admin) return Forbid();
		// }

		if (acc == null) {
			if (!quiz.ModuleUuid.HasValue) return Unauthorized();
			
			var module = await db.CourseModules
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.Uuid == quiz.ModuleUuid.Value);

			if (module == null) return Unauthorized();
			
			if (!module.IsVisible) return Unauthorized();
		}

		return Ok(quiz.ToReadDto());
	}

    [HttpPost("courses/{courseUuid:guid}/quizzes")]
    public async Task<IActionResult> CreateQuizInCourse([FromRoute] Guid courseUuid, [FromBody] CreateUpdateQuizRequest body, CancellationToken ct = default) {
        var course = await db.CoursesFullEf()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
        
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        if (course.Status != CourseStatus.Draft) return BadRequest(new { error = "Only courses in draft status can be updated." });

        var newQuiz = new Quiz {
            Title = body.Title,
            AttemptsCount = body.AttemptsCount,
            CourseUuid = course.Uuid
        };

        foreach (var questionDto in body.Questions) {
            switch (questionDto.Type) {
                case "singleChoice":
                    var singleDto = questionDto as CreateUpdateSingleChoiceQuestionRequest
                                    ?? throw new InvalidOperationException("Expected singleChoice DTO");

                    var singleChoiceQuestion = new SingleChoiceQuestion {
                        Text = singleDto.Question,
                        Quiz = newQuiz,
                        Order = questionDto.Order
                    };

                    for (int i = 0; i < singleDto.Options.Count; i++) {
                        var optionText = singleDto.Options[i];
                        var option = new QuestionOption {
                            Text = optionText,
                            IsCorrect = i == singleDto.CorrectIndex,
                            Question = singleChoiceQuestion,
                            Order = i
                        };

                        singleChoiceQuestion.Options.Add(option);
                    }

                    newQuiz.Questions.Add(singleChoiceQuestion);
                    break;

                case "multipleChoice":
                    var multipleDto = questionDto as CreateUpdateMultipleChoiceQuestionRequest
                                      ?? throw new InvalidOperationException("Expected multipleChoice DTO");

                    var multipleChoiceQuestion = new MultipleChoiceQuestion {
                        Text = multipleDto.Question,
                        Quiz = newQuiz,
                        Order = questionDto.Order
                    };

                    for (int i = 0; i < multipleDto.Options.Count; i++) {
                        var optionText = multipleDto.Options[i];
                        var option = new QuestionOption {
                            Text = optionText,
                            IsCorrect = multipleDto.CorrectIndices.Contains(i),
                            Question = multipleChoiceQuestion,
                            Order = i
                        };

                        multipleChoiceQuestion.Options.Add(option);
                    }
                    
                    newQuiz.Questions.Add(multipleChoiceQuestion);
                    break;

                default:
                    return BadRequest(new { error = $"Unsupported question type: {questionDto.Type}" });
            }
        }

        // ulozeni kvizu do db
        if (body.ModuleUuid.HasValue) {
            var module = await db.CourseModules
                .FirstOrDefaultAsync(m => m.Uuid == body.ModuleUuid.Value && m.CourseUuid == course.Uuid, ct);
            if (module == null)
                return BadRequest(new { error = "Invalid module UUID." });
            newQuiz.ModuleUuid = module.Uuid;
        }

        db.Quizzes.Add(newQuiz);

        // oznameni do sse feedu o novem kvizu
        var newFeedPost = new FeedPost {
            Uuid = Guid.NewGuid(),
            CourseUuid = course.Uuid,
            Type = FeedPost.FeedPostType.System,
            Message = "Byl přidán nový kvíz: " + newQuiz.Title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Purpose = FeedPost.FeedPurpose.CreateQuiz
        };

        //db.FeedPosts.Add(newFeedPost);

        await db.SaveChangesAsync(ct);

        return Ok(newQuiz.ToReadDto());
    }

	[HttpPut("courses/{courseUuid:guid}/quizzes/{quizUuid:guid}")]
	public async Task<IActionResult> UpdateQuizInCourse(
		[FromRoute] Guid courseUuid,
		[FromRoute] Guid quizUuid,
		[FromBody] CreateUpdateQuizRequest body) {
		var course = await db.CoursesMinimalEf()
			.AsNoTracking()
			.AsSplitQuery()
			.FirstOrDefaultAsync(c => c.Uuid == courseUuid);
		
		if (course == null) {
			return NotFound(new { error = "Course not found." });
		}

		if (course.Status != CourseStatus.Draft) return BadRequest(new { error = "Only courses in draft status can be updated." });

		var quiz = await db.QuizzesEf()
			.Where(q => q.CourseUuid == courseUuid)
			.Where(q => q.Uuid == quizUuid)
			.FirstOrDefaultAsync();

		if (quiz == null) {
			return NotFound(new { error = "Quiz not found." });
		}

		quiz.Title = body.Title;
		quiz.AttemptsCount = body.AttemptsCount;

		if (body.IsVisible.HasValue) {
			quiz.IsVisible = body.IsVisible.Value;
		}

		var existingQuestions = quiz.Questions.ToList();

		var incomingIds = body.Questions
			.Where(q => q.Uuid.HasValue)
			.Select(q => q.Uuid!.Value)
			.ToHashSet();

		// Delete questions not in incoming list
		foreach (var existingQuestion in existingQuestions) {
			if (!incomingIds.Contains(existingQuestion.Uuid)) {
				// Remove options first
				foreach (var option in existingQuestion.Options.ToList()) {
					db.QuestionOptions.Remove(option);
				}
				db.Questions.Remove(existingQuestion);
			}
		}

		foreach (var dtoBase in body.Questions) {
			switch (dtoBase.Type) {
				case "singleChoice": {
					var dto = dtoBase as CreateUpdateSingleChoiceQuestionRequest
					          ?? throw new InvalidOperationException("Expected singleChoice DTO");

					// CREATE
					if (!dto.Uuid.HasValue) {

						var newQuestion = new SingleChoiceQuestion {
							Text = dto.Question,
							Quiz = quiz,
							Order = dtoBase.Order
						};

						for (var i = 0; i < dto.Options.Count; i++) {
							var optionText = dto.Options[i];
							var option = new QuestionOption {
								Text = optionText,
								IsCorrect = i == dto.CorrectIndex,
								Question = newQuestion,
								Order = i
							};

							newQuestion.Options.Add(option);
						}

						db.Questions.Add(newQuestion);
						break;
					}

					// UPDATE
					var existingQuestion = existingQuestions
						.OfType<SingleChoiceQuestion>()
						.FirstOrDefault(q => q.Uuid == dto.Uuid.Value);

					if (existingQuestion == null) {
						return BadRequest(new { error = $"Question with UUID {dto.Uuid} not found." });
					}

					existingQuestion.Text = dto.Question;
					existingQuestion.Order = dtoBase.Order;

					// Clear options
					foreach (var option in existingQuestion.Options.ToList()) {
						db.QuestionOptions.Remove(option);
					}

					// Add options
					for (var i = 0; i < dto.Options.Count; i++) {
						var optionText = dto.Options[i];
						var option = new QuestionOption {
							Text = optionText,
							IsCorrect = i == dto.CorrectIndex,
							Question = existingQuestion,
							Order = i
						};

						db.QuestionOptions.Add(option);
					}

					break;
				}

				case "multipleChoice": {
					var dto = dtoBase as CreateUpdateMultipleChoiceQuestionRequest
					          ?? throw new InvalidOperationException("Expected multipleChoice DTO");

					// CREATE
					if (!dto.Uuid.HasValue) {

						var newQuestion = new MultipleChoiceQuestion {
							Text = dto.Question,
							Quiz = quiz,
							Order = dtoBase.Order
						};

						for (var i = 0; i < dto.Options.Count; i++) {
							var optionText = dto.Options[i];
							var option = new QuestionOption {
								Text = optionText,
								IsCorrect = dto.CorrectIndices.Contains(i),
								Question = newQuestion,
								Order = i
							};

							newQuestion.Options.Add(option);
						}

						db.Questions.Add(newQuestion);
						break;
					}

					// UPDATE
					var existingQuestion = existingQuestions
						.OfType<MultipleChoiceQuestion>()
						.FirstOrDefault(q => q.Uuid == dto.Uuid.Value);

					if (existingQuestion == null) {
						return BadRequest(new { error = $"Question with UUID {dto.Uuid} not found." });
					}

					existingQuestion.Text = dto.Question;
					existingQuestion.Order = dtoBase.Order;

					// Clear options
					foreach (var option in existingQuestion.Options.ToList()) {
						db.QuestionOptions.Remove(option);
					}

					// Add options
					for (var i = 0; i < dto.Options.Count; i++) {
						var optionText = dto.Options[i];
						var option = new QuestionOption {
							Text = optionText,
							IsCorrect = dto.CorrectIndices.Contains(i),
							Question = existingQuestion,
							Order = i
						};

						db.QuestionOptions.Add(option);
					}

					break;
				}

				default:
					return BadRequest(new { error = $"Unsupported question type: {dtoBase.Type}" });
			}
		}

		//odesilani info do sse
		var newFeedPost = new FeedPost {
			Uuid = Guid.NewGuid(),
			CourseUuid = course.Uuid,
			Type = FeedPost.FeedPostType.System,
			Message = "Byl upraven kvíz: " + quiz.Title,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			Purpose = FeedPost.FeedPurpose.UpdateQuiz
		};

		//db.FeedPosts.Add(newFeedPost);

		await db.SaveChangesAsync();

		return Ok(quiz.ToReadDto());
	}

	[HttpPut("courses/{courseUuid:guid}/quizzes/{quizUuid:guid}/visibility")]
	public async Task<IActionResult> UpdateQuizVisibility(
		[FromRoute] Guid courseUuid,
		[FromRoute] Guid quizUuid,
		[FromBody] UpdateModuleVisibilityRequest body
	) {
		var quiz = await db.Quizzes
			.Where(q => q.CourseUuid == courseUuid)
			.Where(q => q.Uuid == quizUuid)
			.FirstOrDefaultAsync();

		if (quiz == null) return NotFound(new { error = "Quiz not found." });

		quiz.IsVisible = body.IsVisible;
		quiz.UpdatedAt = DateTime.UtcNow;
		
		db.Quizzes.Update(quiz);
		await db.SaveChangesAsync();
		
		//odesilani info do sse
		var newFeedPost = new FeedPost {
			Uuid = Guid.NewGuid(),
			CourseUuid = courseUuid,
			Type = FeedPost.FeedPostType.System,
			Message = $"Viditelnost kvízu '{quiz.Title}' byla změněna na {(body.IsVisible ? "viditelný" : "skrytý")}.",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			Purpose = body.IsVisible ? FeedPost.FeedPurpose.ShowQuiz : FeedPost.FeedPurpose.HideQuiz
		};
		
		db.FeedPosts.Add(newFeedPost);
		await db.SaveChangesAsync();
		await fsb.PublishAsync(courseUuid, new FeedStreamMessage("new_post", newFeedPost));
		await sb.PublishAsync(courseUuid, new StreamMessage("quiz_visibility_changed", new { quizUuid = quiz.Uuid, isVisible = quiz.IsVisible }));

		return Ok(new { quizUuid = quiz.Uuid, isVisible = quiz.IsVisible });
	}

	[HttpDelete("courses/{courseUuid:guid}/quizzes/{quizUuid:guid}")]
	public async Task<IActionResult> DeleteQuizFromCourse(
		[FromRoute] Guid courseUuid,
		[FromRoute] Guid quizUuid
	) {
		var course = await db.CoursesMinimalEf()
			.AsNoTracking()
			.AsSplitQuery()
			.FirstOrDefaultAsync(c => c.Uuid == courseUuid);
		
		if (course == null) {
			return NotFound(new { error = "Course not found." });
		}

		if (course.Status != CourseStatus.Draft) return BadRequest(new { error = "Only courses in draft status can be updated." });

		var quiz = await db.QuizzesEf()
			.Where(q => q.CourseUuid == courseUuid)
			.Where(q => q.Uuid == quizUuid)
			.FirstOrDefaultAsync();

		if (quiz == null) {
			return NotFound(new { error = "Quiz not found." });
		}

		// Remove options and questions
		foreach (var question in quiz.Questions.ToList()) {
			foreach (var option in question.Options.ToList()) {
				db.QuestionOptions.Remove(option);
			}
			db.Questions.Remove(question);
		}

		db.Quizzes.Remove(quiz);
		await db.SaveChangesAsync();

		//odesilani info do sse
		var newFeedPost = new FeedPost {
			Uuid = Guid.NewGuid(),
			CourseUuid = course.Uuid,
			Type = FeedPost.FeedPostType.System,
			Message = "Byl smazán kvíz: " + quiz.Title,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			Purpose = FeedPost.FeedPurpose.DeleteQuiz
		};

		//db.FeedPosts.Add(newFeedPost);

		await db.SaveChangesAsync();

		return NoContent();
	}

	[HttpPost("courses/{courseUuid:guid}/quizzes/{quizUuid:guid}/submit")]
	public async Task<IActionResult> SubmitQuiz(
		[FromRoute] Guid courseUuid,
		[FromRoute] Guid quizUuid,
		[FromBody] CreateQuizSubmissionRequest body
	) {
		var acc = await auth.ReAuthAsync();

		var course = await db.Courses
			.AsNoTracking()
			.AsSplitQuery()
			.Where(c => c.Uuid == courseUuid)
			.FirstOrDefaultAsync();

		if (course == null) {
			return NotFound(new { error = "Course not found." });
		}

		var accessResult = ValidateRestrictedCourseAccess(course, acc);
		if (accessResult != null) {
			return accessResult;
		}

		var quiz = await db.QuizzesEf()
			.AsNoTracking()
			.Where(q => q.CourseUuid == courseUuid)
			.Where(q => q.Uuid == quizUuid)
			.FirstOrDefaultAsync();

		if (quiz == null) {
			return NotFound(new { error = "Quiz not found." });
		}

		var totalQuestions = quiz.Questions.Count;
		var correctAnswers = 0;
		var correctPerQuestion = new List<bool>();

		foreach (var question in quiz.Questions) {
			var submission = body.Answers.FirstOrDefault(a => a.Uuid == question.Uuid);
			if (submission == null) continue;

			var isCorrect = false;

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
			}).ToList(),
			TotalTimeSeconds = body.TotalTimeSeconds
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
		var acc = await auth.ReAuthAsync();

		var course = await db.Courses
			.AsNoTracking()
			.AsSplitQuery()
			.Where(c => c.Uuid == courseUuid)
			.FirstOrDefaultAsync();

		if (course == null) {
			return NotFound(new { error = "Course not found." });
		}

		var accessResult = ValidateRestrictedCourseAccess(course, acc);
		if (accessResult != null) {
			return accessResult;
		}

		var quiz = await db.QuizzesEf()
			.AsNoTracking()
			.Where(q => q.CourseUuid == courseUuid)
			.Where(q => q.Uuid == quizUuid)
			.FirstOrDefaultAsync();

		if (quiz == null || quiz.CourseUuid != course.Uuid) {
			return NotFound(new { error = "Quiz not found in the specified course." });
		}

		var quizResult = await db.QuizResultsEf()
			.AsNoTracking()
			.Where(qr => qr.Uuid == resultUuid)
			.FirstOrDefaultAsync();

		if (quizResult == null || quizResult.QuizUuid != quiz.Uuid) {
			return NotFound(new { error = "Quiz result not found for the specified quiz." });
		}

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
						.OrderBy(o => o.Order)
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
	
	[HttpGet("courses/{courseUuid:guid}/quizzes/{quizUuid:guid}/results-summary")]
    public async Task<IActionResult> GetQuizResultsSummary(
        [FromRoute] Guid courseUuid,
        [FromRoute] Guid quizUuid
    ) {
        var course = await db.Courses
            .Where(c => c.Uuid == courseUuid)
            .FirstOrDefaultAsync();

        if (course == null)
            return NotFound(new { error = "Course not found." });

        var quiz = await db.QuizzesEf()
            .Where(q => q.CourseUuid == courseUuid && q.Uuid == quizUuid)
            .FirstOrDefaultAsync();

        if (quiz == null)
            return NotFound(new { error = "Quiz not found in the specified course." });

        var results = await db.QuizResultsEf()
            .Where(r => r.QuizUuid == quiz.Uuid)
            .Select(r => new {
                r.Score,
                r.TotalTimeSeconds
            })
            .ToListAsync();

        var totalAttempts = results.Count;

        var totalQuestions = quiz.Questions.Count;

        var averageScore = results
	        .Select(r => (double)r.Score)
	        .DefaultIfEmpty(0)
	        .Average();

        var averageTimeSpent = results
	        .Where(r => r.TotalTimeSeconds > 0)
	        .Select(r => (double)r.TotalTimeSeconds)
	        .DefaultIfEmpty(0)
	        .Average();

        var averageScorePercentage =
	        totalQuestions > 0
		        ? averageScore / totalQuestions * 100
		        : 0;

        // Score distribution (percent)
        var scoreBuckets = new[] {
            (Min: 0, Max: 20),
            (Min: 20, Max: 40),
            (Min: 40, Max: 60),
            (Min: 60, Max: 80),
            (Min: 80, Max: 100)
        };

        var scoreDistribution = scoreBuckets.Select(b => new {
            label = $"{b.Min}–{b.Max}%",
            count = results.Count(r =>
                quiz.Questions.Count > 0 &&
                (r.Score / (double)quiz.Questions.Count * 100) >= b.Min &&
                (r.Score / (double)quiz.Questions.Count * 100) < b.Max
            )
        }).ToList();

        // Time distribution (minutes)
        var timeBuckets = new[] {
            (Label: "0–1 min", Min: 0, Max: 60),
            (Label: "1–3 min", Min: 60, Max: 180),
            (Label: "3–5 min", Min: 180, Max: 300),
            (Label: "5–10 min", Min: 300, Max: 600),
            (Label: "10+ min", Min: 600, Max: int.MaxValue)
        };

        var timeDistribution = timeBuckets.Select(b => new {
            label = b.Label,
            count = results.Count(r =>
                r.TotalTimeSeconds >= b.Min &&
                r.TotalTimeSeconds < b.Max
            )
        }).ToList();

        return Ok(new {
            totalAttempts,
            averageScore,
            averageTimeSpent,
            averageScorePercentage,
            scoreDistribution,
            timeDistribution
        });
    }

	#endregion

	[HttpPost("courses/{uuid:guid}/duplicate")]
	public async Task<IActionResult> DuplicateCourse([FromRoute] Guid uuid, CancellationToken ct = default) {
		var acc = await auth.ReAuthAsync(ct);
		if (acc == null) return Unauthorized();

		var sourceCourse = await db.CoursesMinimalEf()
			.AsNoTracking()
			.AsSplitQuery()
			.FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
		if (sourceCourse == null) return NotFound(new { error = "Course not found." });

		// if (acc is not Admin && sourceCourse.LecturerUuid != acc.Uuid) return Unauthorized();

		var materials = await db.Materials
			.AsNoTracking()
			.Where(m => m.CourseUuid == uuid)
			.ToListAsync(ct);

		var feedPosts = await db.FeedPosts
			.AsNoTracking()
			.Where(fp => fp.CourseUuid == uuid)
			.ToListAsync(ct);

		var quizzes = await db.QuizzesEf()
			.AsNoTracking()
			.Where(q => q.CourseUuid == uuid)
			.ToListAsync(ct);

		var sourceModules = await db.CourseModules
			.AsNoTracking()
			.Where(m => m.CourseUuid == uuid)
			.OrderBy(m => m.Order)
			.Include(m => m.Materials)
			.Include(m => m.Quizzes)
			.ToListAsync(ct);

		var tagUuids = sourceCourse.Tags.Select(t => t.Uuid).ToList();
		var tags = tagUuids.Count == 0
			? new List<Tag>()
			: await db.Tags.Where(t => tagUuids.Contains(t.Uuid)).ToListAsync(ct);

		var duplicateName = sourceCourse.Name + " (kopie)";
		if (duplicateName.Length > 128) duplicateName = duplicateName[..128];

		var newCourse = new Course {
			Name = duplicateName,
			Description = sourceCourse.Description,
			ImageUrl = sourceCourse.ImageUrl,
			LecturerUuid = sourceCourse.LecturerUuid,
			CategoryUuid = sourceCourse.CategoryUuid,
			Status = CourseStatus.Draft,
			ScheduledStart = null,
			ViewCount = 0,
			Tags = tags
		};

		if (!string.IsNullOrEmpty(sourceCourse.ImageUrl) && sourceCourse.ImageUrl.StartsWith("course-images/", StringComparison.Ordinal)) {
			var imageExtension = Path.GetExtension(sourceCourse.ImageUrl);
			var targetImageKey = $"course-images/{newCourse.Uuid}{imageExtension}";
			newCourse.ImageUrl = await materialAccessService.CopyFileAsync(
				sourceCourse.ImageUrl,
				targetImageKey,
				ct
			);
		}

		var sourceMaterialsPrefix = $"materials/{uuid}/";
		var targetMaterialsPrefix = $"materials/{newCourse.Uuid}/";
		await materialAccessService.CopyCourseMaterialsDirectoryAsync(uuid, newCourse.Uuid, ct);

		// oldMaterialUuid → new Material (needed for module item assignment)
		var materialMap = new Dictionary<Guid, Material>();

		foreach (var material in materials) {
			Material newMaterial;
			switch (material) {
				case UrlMaterial urlMaterial:
					newMaterial = new UrlMaterial {
						Name = urlMaterial.Name,
						Description = urlMaterial.Description,
						Type = Material.MaterialType.Url,
						CourseUuid = newCourse.Uuid,
						Url = urlMaterial.Url,
						FaviconUrl = urlMaterial.FaviconUrl,
						IsVisible = urlMaterial.IsVisible,
						Order = urlMaterial.Order
					};
					break;
				case FileMaterial fileMaterial:
					var newFileUrl = fileMaterial.FileUrl.StartsWith(sourceMaterialsPrefix, StringComparison.Ordinal)
						? targetMaterialsPrefix + fileMaterial.FileUrl[sourceMaterialsPrefix.Length..]
						: fileMaterial.FileUrl;

					newMaterial = new FileMaterial {
						Name = fileMaterial.Name,
						Description = fileMaterial.Description,
						Type = Material.MaterialType.File,
						CourseUuid = newCourse.Uuid,
						FileUrl = newFileUrl,
						MimeType = fileMaterial.MimeType,
						SizeBytes = fileMaterial.SizeBytes,
						IsVisible = fileMaterial.IsVisible,
						Order = fileMaterial.Order
					};
					break;
				default:
					continue;
			}
			materialMap[material.Uuid] = newMaterial;
			newCourse.Materials.Add(newMaterial);
		}

		// oldQuizUuid → new Quiz (needed for module item assignment)
		var quizMap = new Dictionary<Guid, Quiz>();

		foreach (var quiz in quizzes) {
			var newQuiz = new Quiz {
				Title = quiz.Title,
				AttemptsCount = 0,
				CourseUuid = newCourse.Uuid,
				IsVisible = quiz.IsVisible,
				Order = quiz.Order
			};

			foreach (var question in quiz.Questions) {
				Question newQuestion = question switch {
					SingleChoiceQuestion => new SingleChoiceQuestion(),
					MultipleChoiceQuestion => new MultipleChoiceQuestion(),
					_ => throw new InvalidOperationException("Unknown question type.")
				};

				newQuestion.Text = question.Text;
				newQuestion.Order = question.Order;
				newQuestion.QuizUuid = newQuiz.Uuid;

				foreach (var option in question.Options) {
					newQuestion.Options.Add(new QuestionOption {
						Text = option.Text,
						Order = option.Order,
						IsCorrect = option.IsCorrect,
						QuestionUuid = newQuestion.Uuid
					});
				}

				newQuiz.Questions.Add(newQuestion);
			}

			quizMap[quiz.Uuid] = newQuiz;
			newCourse.Quizzes.Add(newQuiz);
		}

		// Copy modules and re-assign their materials/quizzes using the maps built above
		foreach (var sourceModule in sourceModules) {
			var newModule = new CourseModule {
				Title = sourceModule.Title,
				Description = sourceModule.Description,
				IsVisible = sourceModule.IsVisible,
				Order = sourceModule.Order,
				CourseUuid = newCourse.Uuid
			};

			foreach (var modMaterial in sourceModule.Materials) {
				if (materialMap.TryGetValue(modMaterial.Uuid, out var newMat))
					newModule.Materials.Add(newMat);
			}

			foreach (var modQuiz in sourceModule.Quizzes) {
				if (quizMap.TryGetValue(modQuiz.Uuid, out var newQuiz))
					newModule.Quizzes.Add(newQuiz);
			}

			newCourse.Modules.Add(newModule);
		}

		foreach (var post in feedPosts) {
			newCourse.Feed.Add(new FeedPost {
				Uuid = Guid.NewGuid(),
				Type = post.Type,
				Message = post.Message,
				CourseUuid = newCourse.Uuid,
				AccountUuid = post.AccountUuid,
				Edited = post.Edited,
				Purpose = post.Purpose,
				CreatedAt = post.CreatedAt,
				UpdatedAt = post.UpdatedAt
			});
		}

		db.Courses.Add(newCourse);
		await db.SaveChangesAsync(ct);

		return new CreatedAtActionResult(
			nameof(GetCourseById),
			"APIv1",
			new { uuid = newCourse.Uuid },
			newCourse.ToReadDto(true)
		);
	}

	#endregion





	#region FeedPosts

	[HttpGet("courses/{courseUuid:guid}/feed")]
	public async Task<IActionResult> GetFeedPostsByCourseId([FromRoute] Guid courseUuid) {
		var acc = await auth.ReAuthAsync();

		var courseRaw = await db.Courses
			.AsNoTracking()
			.Where(c => c.Uuid == courseUuid)
			.Select(c => new {
				c.Uuid,
				c.LecturerUuid,
				c.Status
			})
			.FirstOrDefaultAsync();

		if (courseRaw == null) {
			return NotFound(new { error = "Course not found." });
		}

		var course = new Course {
			Uuid = courseRaw.Uuid,
			LecturerUuid = courseRaw.LecturerUuid,
			Status = courseRaw.Status
		};

		var accessResult = ValidateRestrictedCourseAccess(course, acc);
		if (accessResult != null) {
			return accessResult;
		}

		var feedPosts = await db.FeedPostsEf()
			.AsNoTracking()
			.AsSplitQuery()
			.Where(fp => fp.CourseUuid == courseUuid)
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
		if (!courseExists) {
			return NotFound(new { error = "Course not found." });
		}


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

		var feedPost = await db.FeedPostsEf()
			.AsNoTracking()
			.Where(fp => fp.Uuid == newFeedPost.Uuid)
			.FirstOrDefaultAsync(ct);

		if(feedPost == null) {
			return NotFound(new { error = "Feed post not found after creation." });
		}

		await fsb.PublishAsync(courseUuid, new FeedStreamMessage("new_post", feedPost), ct);

		return CreatedAtAction(nameof(GetFeedPostsByCourseId), new { courseUuid }, feedPost);
	}


	[HttpDelete("courses/{courseUuid:guid}/feed/{feedPostUuid:guid}")]
	public async Task<IActionResult> DeleteFeedPostFromCourse(
		[FromRoute] Guid courseUuid,
		[FromRoute] Guid feedPostUuid,
		CancellationToken ct
	) {
		var courseExists = await db.Courses
			.AnyAsync(c => c.Uuid == courseUuid, ct);
		if (!courseExists) {
			return NotFound(new { error = "Course not found." });
		}

		var feedPost = await db.FeedPosts
			.Where(fp => fp.CourseUuid == courseUuid)
			.Where(fp => fp.Uuid == feedPostUuid)
			.FirstOrDefaultAsync(ct);

		if (feedPost is null) {
			return NotFound(new { error = "Feed post not found in the specified course." });
		}

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
		if (!courseExists) {
			return NotFound(new { error = "Course not found." });
		}

		var feedPost = await db.FeedPostsEf()
			.Where(fp => fp.CourseUuid == courseUuid)
			.Where(fp => fp.Uuid == feedPostUuid)
			.FirstOrDefaultAsync(ct);

		if (feedPost is null) {
			return NotFound(new { error = "Feed post not found in the specified course." });
		}

		feedPost.Message = body.Message;
		feedPost.Edited = body.Edited;

		await db.SaveChangesAsync(ct);
		await fsb.PublishAsync(courseUuid, new FeedStreamMessage("update_post", feedPost), ct);

		return Ok(feedPost);
	}

	#endregion

}