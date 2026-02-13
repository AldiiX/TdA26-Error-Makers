using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
[Route("api/v1"), Route("api")]
public class APIv1(
    IMaterialAccessService materialAccessService,
    IAuthService auth,
    AppDbContext db,
    IFeedStreamBroker fsb
) : Controller {

    [HttpGet]
    public IActionResult Index() {
        return Ok(new {
            organization = "Student Cyber Games",
        });
    }

    #region  courses

    #region  course
    
    [HttpGet("courses")]
    public async Task<IActionResult> GetCourses() {
        var courses = await db.CoursesFullEf()
            .OrderByDescending(c => c.CreatedAt)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();

        var obj = courses.Select(course => course.ToReadDto());

        return Ok(obj);
    }

    [HttpGet("courses/{uuid:guid}")]
    public async Task<IActionResult> GetCourseById([FromRoute] Guid uuid) {
        var course = await db.CoursesFullEf()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == uuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        return Ok(course.ToReadDto());
    }

    [HttpPut("courses/{uuid:guid}")]
    public async Task<IActionResult> EditCourse([FromRoute] Guid uuid, [FromBody] UpdateCourseRequest body) {
        var course = await db.CoursesFullEf()
            .FirstOrDefaultAsync(c => c.Uuid == uuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        if (!string.IsNullOrEmpty(body.Name)) {
            course.Name = body.Name;
        }

        if (!string.IsNullOrEmpty(body.Description)) {
            course.Description = body.Description;
        }

        var entry = db.Entry(course);
        if (entry.State == EntityState.Detached) {
            db.Courses.Update(course);
        }
        await db.SaveChangesAsync();

        return Ok(course.ToReadDto());
    }

    [HttpDelete("courses/{uuid:guid}")]
    public async Task<IActionResult> DeleteCourse([FromRoute] Guid uuid) {
        var course = await db.Courses.FindAsync(uuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        db.Courses.Remove(course);
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("courses")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest body) {
        if (string.IsNullOrEmpty(body.Name) || string.IsNullOrEmpty(body.Description)) {
            return BadRequest(new { error = "Name and description are required." });
        }

        var adminLecturer = await db.AccountsEf()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Username == "lecturer");

        var newCourse = new Course {
            Name = body.Name,
            Description = body.Description,
            LecturerUuid = adminLecturer?.Uuid ?? null
        };

        db.Courses.Add(newCourse);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCourseById), new { uuid = newCourse.Uuid }, newCourse.ToReadDto());
    }
    
    #endregion

    #region materials

    

    [HttpPost("courses/{uuid:guid}/materials")]
    [Consumes("application/json")]
    public async Task<IActionResult> AddMaterialToCourse([FromRoute] Guid uuid, [FromBody] CreateUrlMaterialRequest body) {
        if (body.Type != "url") {
            return BadRequest(new { error = "Only 'url' material type is supported in this endpoint." });
        }

        var course = await db.CoursesMinimalEf()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == uuid);
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

        db.Materials.Add(newMaterial);
        await db.SaveChangesAsync();

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
        await db.SaveChangesAsync();
        await fsb.PublishAsync(course.Uuid, new FeedStreamMessage("new_post", post));

        return CreatedAtAction(nameof(GetCourseById), new { uuid = course.Uuid }, obj);
    }

    [HttpPost("courses/{courseId:guid}/materials")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddFileMaterialToCourse(
        [FromRoute] Guid courseId,
        [FromForm] CreateFileMaterialRequest body
    ) {
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

        var course = await db.CoursesMinimalEf()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == courseId);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        var file = body.File;
        if (file.Length == 0) {
            return BadRequest(new { error = "File content is required." });
        }

        var fileUrl = await materialAccessService.UploadFileMaterialAsync(course.Uuid, file);

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
        await db.SaveChangesAsync();

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
        await db.SaveChangesAsync();
        await fsb.PublishAsync(course.Uuid, new FeedStreamMessage("new_post", post));

        return CreatedAtAction(nameof(GetCourseById), new { uuid = course.Uuid }, responseObj);
    }

    // Other content types return 400
    [HttpPost("courses/{courseId:guid}/materials")]
    public IActionResult AddMaterialToCourseUnsupported([FromRoute] Guid courseId) {
        return BadRequest(new { error = "Unsupported content type. Use 'application/json' for URL materials or 'multipart/form-data' for file materials." });
    }


    [HttpGet("courses/{uuid:guid}/materials")]
    public async Task<IActionResult> GetMaterialsByCourseId([FromRoute] Guid uuid) {
        var course = await db.CoursesMinimalEf()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == uuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }
        var materials = await db.Materials
            .Where(m => m.CourseUuid == course.Uuid)
            .OrderByDescending(m => m.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var obj = materials.Select(material => material.ToReadDto());

        return Ok(obj);
    }

    [HttpPut("courses/{courseUuid:guid}/materials/{materialUuid:guid}")]
    [Consumes("application/json")]
    public async Task<IActionResult> UpdateUrlMaterialInCourse(
        [FromRoute] Guid courseUuid,
        [FromRoute] Guid materialUuid,
        [FromBody] UpdateUrlMaterialRequest body
    ) {
        var course = await db.CoursesMinimalEf()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        var material = await db.Materials
            .FirstOrDefaultAsync(m => m.Uuid == materialUuid);

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
                await db.SaveChangesAsync();

                // odesilani info do sse 
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
        
                await db.SaveChangesAsync();
                await fsb.PublishAsync(course.Uuid, new FeedStreamMessage("new_post", newFeedPost));
                
                return Ok(urlMaterial.ToReadDto());
            
            case FileMaterial fileMaterial:
                if (!string.IsNullOrEmpty(body.Name))
                    fileMaterial.Name = body.Name;

                if (!string.IsNullOrEmpty(body.Description))
                    fileMaterial.Description = body.Description;

                fileMaterial.UpdatedAt = DateTime.UtcNow;
                db.Materials.Update(fileMaterial);
                await db.SaveChangesAsync();

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
        [FromForm] UpdateFileMaterialRequest body
    ) {
        var course = await db.CoursesMinimalEf()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        var material = await db.Materials
            .FirstOrDefaultAsync(m => m.Uuid == materialUuid);

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

            await materialAccessService.DeleteFileMaterialAsync(fileMaterial.FileUrl);

            var newFileUrl = await materialAccessService.UploadFileMaterialAsync(course.Uuid, body.File);
            fileMaterial.FileUrl = newFileUrl;
            fileMaterial.MimeType = mimeType;
            fileMaterial.SizeBytes = (int)body.File.Length;
        }

        fileMaterial.UpdatedAt = DateTime.UtcNow;
        db.Materials.Update(fileMaterial);
        await db.SaveChangesAsync();

        // odesilani info do sse 
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
        await fsb.PublishAsync(course.Uuid, new FeedStreamMessage("new_post", newFeedPost));
        
        return Ok(fileMaterial.ToReadDto());
    }

    [HttpDelete("courses/{courseUuid:guid}/materials/{materialUuid:guid}")]
    public async Task<IActionResult> DeleteMaterialFromCourse(
        [FromRoute] Guid courseUuid,
        [FromRoute] Guid materialUuid
    ) {
        var course = await db.CoursesMinimalEf()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        var material = await db.Materials
            .FirstOrDefaultAsync(m => m.Uuid == materialUuid);

        if (material == null || material.CourseUuid != course.Uuid)
            return NotFound(new { error = "Material not found in the specified course." });


        if (material is FileMaterial fileMaterial)
            await materialAccessService.DeleteFileMaterialAsync(fileMaterial.FileUrl);

        db.Materials.Remove(material);
        await db.SaveChangesAsync();

        // odeslani info do sse
        var newFeedPost = new FeedPost {
            Uuid = Guid.NewGuid(),
            CourseUuid = course.Uuid,
            Type = FeedPost.FeedPostType.System,
            Message = $"Byl smazán materiál: {material.Name}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Purpose = FeedPost.FeedPurpose.DeleteMaterial
        };
        
        db.FeedPosts.Add(newFeedPost);
        await db.SaveChangesAsync();
        
        await fsb.PublishAsync(
            course.Uuid, 
            new FeedStreamMessage("new_post", newFeedPost)
        );
        
        return NoContent();
    }
    #endregion

    #region quizzes
    
    // Quizzes

    [HttpGet("courses/{courseUuid:guid}/quizzes")]
    public async Task<IActionResult> GetQuizzesByCourseId([FromRoute] Guid courseUuid) {
        var course = await db.CoursesMinimalEf()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        var quizzes = await db.QuizzesEf()
            .Where(q => q.CourseUuid == courseUuid)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();

        var obj = quizzes.Select(quiz => (dynamic)quiz.ToReadDto());

        return Ok(obj);
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

        db.FeedPosts.Add(newFeedPost);

        await db.SaveChangesAsync(ct);
        await fsb.PublishAsync(course.Uuid, new FeedStreamMessage("new_post", newFeedPost), ct);

        return Ok(newQuiz.ToReadDto());
    }

    [HttpGet("courses/{courseUuid:guid}/quizzes/{quizUuid:guid}")]
    public async Task<IActionResult> GetQuizById([FromRoute] Guid courseUuid, [FromRoute] Guid quizUuid) {
        var course = await db.CoursesMinimalEf()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid);
        if (course == null)
            return NotFound(new { error = "Course not found." });

        var quiz = await db.QuizzesEf()
            .Where(q => q.CourseUuid == courseUuid)
            .Where(q => q.Uuid == quizUuid)
            .FirstOrDefaultAsync();

        if (quiz == null || quiz.CourseUuid != course.Uuid)
            return NotFound(new { error = "Quiz not found in the specified course." });

        return Ok(quiz.ToReadDto());
    }
    
    [HttpPut("courses/{courseUuid:guid}/quizzes/{quizUuid:guid}")]
    public async Task<IActionResult> UpdateQuizInCourse(
        [FromRoute] Guid courseUuid,
        [FromRoute] Guid quizUuid,
        [FromBody] CreateUpdateQuizRequest body)
    {
        var course = await db.CoursesMinimalEf()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid);
        if (course == null)
            return NotFound(new { error = "Course not found." });

        var quiz = await db.QuizzesEf()
            .Where(q => q.CourseUuid == courseUuid)
            .Where(q => q.Uuid == quizUuid)
            .FirstOrDefaultAsync();
        
        if (quiz == null)
            return NotFound(new { error = "Quiz not found." });

        quiz.Title = body.Title;
        quiz.AttemptsCount = body.AttemptsCount;

        var existingQuestions = quiz.Questions.ToList();

        var incomingIds = body.Questions
            .Where(q => q.Uuid.HasValue)
            .Select(q => q.Uuid!.Value)
            .ToHashSet();
        
        // Delete questions not in incoming list
        foreach (var existingQuestion in existingQuestions)
        {
            if (!incomingIds.Contains(existingQuestion.Uuid))
            {
                // Remove options first
                foreach (var option in existingQuestion.Options.ToList())
                    db.QuestionOptions.Remove(option);
                db.Questions.Remove(existingQuestion);
            }
        }
        
        foreach (var dtoBase in body.Questions)
        {
            switch (dtoBase.Type)
            {
                case "singleChoice":
                {
                    var dto = dtoBase as CreateUpdateSingleChoiceQuestionRequest
                              ?? throw new InvalidOperationException("Expected singleChoice DTO");
                    
                    // CREATE
                    if (!dto.Uuid.HasValue)
                    {
                        
                        var newQuestion = new SingleChoiceQuestion
                        {
                            Text = dto.Question,
                            Quiz = quiz,
                            Order = dtoBase.Order
                        };

                        for (int i = 0; i < dto.Options.Count; i++)
                        {
                            var optionText = dto.Options[i];
                            var option = new QuestionOption
                            {
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
                    
                    if (existingQuestion == null)
                    {
                        return BadRequest(new { error = $"Question with UUID {dto.Uuid} not found." });
                    }
                    
                    existingQuestion.Text = dto.Question;
                    existingQuestion.Order = dtoBase.Order;
                    
                    // Clear options
                    foreach (var option in existingQuestion.Options.ToList())
                        db.QuestionOptions.Remove(option);
                    
                    // Add options
                    for (int i = 0; i < dto.Options.Count; i++)
                    {
                        var optionText = dto.Options[i];
                        var option = new QuestionOption
                        {
                            Text = optionText,
                            IsCorrect = i == dto.CorrectIndex,
                            Question = existingQuestion,
                            Order = i
                        };
                        
                        db.QuestionOptions.Add(option);
                    }

                    break;
                }

                case "multipleChoice":
                {
                    var dto = dtoBase as CreateUpdateMultipleChoiceQuestionRequest
                              ?? throw new InvalidOperationException("Expected multipleChoice DTO");
                    
                    // CREATE
                    if (!dto.Uuid.HasValue)
                    {
                        
                        var newQuestion = new MultipleChoiceQuestion
                        {
                            Text = dto.Question,
                            Quiz = quiz,
                            Order = dtoBase.Order
                        };

                        for (int i = 0; i < dto.Options.Count; i++)
                        {
                            var optionText = dto.Options[i];
                            var option = new QuestionOption
                            {
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
                    
                    if (existingQuestion == null)
                    {
                        return BadRequest(new { error = $"Question with UUID {dto.Uuid} not found." });
                    }
                    
                    existingQuestion.Text = dto.Question;
                    existingQuestion.Order = dtoBase.Order;
                    
                    // Clear options
                    foreach (var option in existingQuestion.Options.ToList())
                        db.QuestionOptions.Remove(option);
                    
                    // Add options
                    for (int i = 0; i < dto.Options.Count; i++)
                    {
                        var optionText = dto.Options[i];
                        var option = new QuestionOption
                        {
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
        
        db.FeedPosts.Add(newFeedPost);
        
        await db.SaveChangesAsync();
        await fsb.PublishAsync(course.Uuid, new FeedStreamMessage("new_post", newFeedPost));
        
        return Ok(quiz.ToReadDto());
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
        if (course == null)
            return NotFound(new { error = "Course not found." });

        var quiz = await db.QuizzesEf()
            .Where(q => q.CourseUuid == courseUuid)
            .Where(q => q.Uuid == quizUuid)
            .FirstOrDefaultAsync();
        
        if (quiz == null)
            return NotFound(new { error = "Quiz not found." });
        
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
        
        db.FeedPosts.Add(newFeedPost);
        
        await db.SaveChangesAsync();
        await fsb.PublishAsync(course.Uuid, new FeedStreamMessage("new_post", newFeedPost));
        
        return NoContent();
    }

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

        var quiz = await db.QuizzesEf()
            .Where(q => q.CourseUuid == courseUuid)
            .Where(q => q.Uuid == quizUuid)
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
                case SingleChoiceQuestion scq:
                    var correctOptionIndex = scq.Options.ToList().FindIndex(o => o.IsCorrect);
                    if (submission.SelectedIndex == correctOptionIndex) {
                        isCorrect = true;
                    }
                    break;

                case MultipleChoiceQuestion mcq:
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
                    break;
            }
            
            if (isCorrect) correctAnswers++;
            correctPerQuestion.Add(isCorrect);
        }
        
        var quizResult = new QuizResult {
            QuizUuid = quiz.Uuid,
            Score = correctAnswers,
            CompletedAt = DateTime.UtcNow
        };

        quiz.AttemptsCount++;
        
        db.QuizResults.Add(quizResult);
        await db.SaveChangesAsync();

        var response = new {
            quizUuid,
            score = correctAnswers,
            maxScore = totalQuestions,
            correctPerQuestion,
            submittedAt = DateTime.UtcNow
        };
        

        return Ok(response);
    }
    #endregion
    
    #endregion
    



    #region FeedPosts

    [HttpGet("courses/{courseUuid:guid}/feed")]
    public async Task<IActionResult> GetFeedPostsByCourseId([FromRoute] Guid courseUuid) {
        var course = await db.CoursesMinimalEf()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        var feedPosts = await db.FeedPostsEf()
            .Where(fp => fp.CourseUuid == course.Uuid)
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
        var course = await db.CoursesMinimalEf()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
        if (course is null)
            return NotFound(new { error = "Course not found." });


        var loggedAccount = await auth.ReAuthAsync(ct);

        var newFeedPost = new FeedPost {
            Uuid = Guid.NewGuid(),
            Type = FeedPost.FeedPostType.Manual,
            Message = body.Message,
            CourseUuid = course.Uuid,
            AccountUuid = loggedAccount?.Uuid,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.FeedPosts.Add(newFeedPost);
        await db.SaveChangesAsync(ct);

        await fsb.PublishAsync(course.Uuid, new FeedStreamMessage("new_post", newFeedPost), ct);

        return CreatedAtAction(nameof(GetFeedPostsByCourseId), new { courseUuid = course.Uuid }, newFeedPost);
    }


    [HttpDelete("courses/{courseUuid:guid}/feed/{feedPostUuid:guid}")]
    public async Task<IActionResult> DeleteFeedPostFromCourse(
        [FromRoute] Guid courseUuid,
        [FromRoute] Guid feedPostUuid,
        CancellationToken ct
    ) {
        var course = await db.CoursesMinimalEf()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
        if (course is null)
            return NotFound(new { error = "Course not found." });

        var feedPost = await db.FeedPosts
            .Where(fp => fp.CourseUuid == course.Uuid)
            .Where(fp => fp.Uuid == feedPostUuid)
            .FirstOrDefaultAsync(ct);

        if (feedPost is null)
            return NotFound(new { error = "Feed post not found in the specified course."  });

        db.FeedPosts.Remove(feedPost);
        await db.SaveChangesAsync(ct);

        await fsb.PublishAsync(course.Uuid, new FeedStreamMessage("delete_post", new { uuid = feedPostUuid }), ct);

        return NoContent();
    }

    [HttpPut("courses/{courseUuid:guid}/feed/{feedPostUuid:guid}")]
    public async Task<IActionResult> UpdateFeedPostInCourse(
        [FromRoute] Guid courseUuid,
        [FromRoute] Guid feedPostUuid,
        [FromBody] EditCourseFeedPostRequest body,
        CancellationToken ct
    ) {
        var course = await db.CoursesMinimalEf()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
        if (course is null)
            return NotFound(new { error = "Course not found." });

        var feedPost = await db.FeedPostsEf()
            .Where(fp => fp.CourseUuid == course.Uuid)
            .Where(fp => fp.Uuid == feedPostUuid)
            .FirstOrDefaultAsync(ct);

        if (feedPost is null)
            return NotFound(new { error = "Feed post not found in the specified course." });

        feedPost.Message = body.Message;
        feedPost.Edited = body.Edited;

        await db.SaveChangesAsync(ct);

        await fsb.PublishAsync(course.Uuid, new FeedStreamMessage("update_post", feedPost), ct);

        return Ok(feedPost);
    }

    #endregion
}