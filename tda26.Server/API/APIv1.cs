using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minio;
using tda26.Server.Data;
using tda26.Server.Data.Models;
using tda26.Server.DTOs.Mapping;
using tda26.Server.DTOs.v1;
using tda26.Server.Infrastructure;
using tda26.Server.Repositories;
using tda26.Server.Services;
using CreateCourseRequest = tda26.Server.DTOs.v1.CreateCourseRequest;

namespace tda26.Server.API;

[ApiController]
[Route("api/v1"), Route("api")]
public class APIv1(
    ICourseRepository courseRepository,
    IMaterialRepository materialRepository,
    IMaterialAccessService materialAccessService,
    IAccountRepository accountRepository,
    AppDbContext db
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
        var course = await courseRepository.GetByUuidAsyncFull(uuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        return Ok(course.ToReadDto());
    }

    [HttpPut("courses/{uuid:guid}")]
    public async Task<IActionResult> EditCourse([FromRoute] Guid uuid, [FromBody] UpdateCourseRequest body) {
        var course = await courseRepository.GetByUuidAsyncFull(uuid);
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
        var course = await courseRepository.GetByUuidAsync(uuid);
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
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest body) {
        if (string.IsNullOrEmpty(body.Name) || string.IsNullOrEmpty(body.Description)) {
            return BadRequest(new { error = "Name and description are required." });
        }

        var adminLecturer = await accountRepository.GetByUsernameAsync("lecturer");

        var newCourse = new Data.Models.Course {
            Name = body.Name,
            Description = body.Description,
            LecturerUuid = adminLecturer?.Uuid ?? null
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

        var course = await courseRepository.GetByUuidAsync(uuid);
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
            type = "url",
            createdAt = newMaterial.CreatedAt,
            updatedAt = newMaterial.UpdatedAt
        };

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

        var course = await courseRepository.GetByUuidAsync(courseId);
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

        await materialRepository.AddMaterialAsync(course.Uuid, newMaterial);

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

        return CreatedAtAction(nameof(GetCourseById), new { uuid = course.Uuid }, responseObj);
    }

    // Other content types return 400
    [HttpPost("courses/{courseId:guid}/materials")]
    public IActionResult AddMaterialToCourseUnsupported([FromRoute] Guid courseId) {
        return BadRequest(new { error = "Unsupported content type. Use 'application/json' for URL materials or 'multipart/form-data' for file materials." });
    }


    [HttpGet("courses/{uuid:guid}/materials")]
    public async Task<IActionResult> GetMaterialsByCourseId([FromRoute] Guid uuid) {
        var course = await courseRepository.GetByUuidAsync(uuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }
        var materials = await materialRepository.GetMaterialsByCourseIdAsync(course.Uuid);

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
        var course = await courseRepository.GetByUuidAsync(courseUuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        var material = await materialRepository.GetMaterialByUuidAsync(materialUuid);

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

                await materialRepository.UpdateMaterialAsync(urlMaterial);

                return Ok(urlMaterial.ToReadDto());
            case FileMaterial fileMaterial:
                if (!string.IsNullOrEmpty(body.Name))
                    fileMaterial.Name = body.Name;

                if (!string.IsNullOrEmpty(body.Description))
                    fileMaterial.Description = body.Description;

                await materialRepository.UpdateMaterialAsync(fileMaterial);

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
        var course = await courseRepository.GetByUuidAsync(courseUuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        var material = await materialRepository.GetMaterialByUuidAsync(materialUuid);

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

        await materialRepository.UpdateMaterialAsync(fileMaterial);

        return Ok(fileMaterial.ToReadDto());
    }

    [HttpDelete("courses/{courseUuid:guid}/materials/{materialUuid:guid}")]
    public async Task<IActionResult> DeleteMaterialFromCourse(
        [FromRoute] Guid courseUuid,
        [FromRoute] Guid materialUuid
    ) {
        var course = await courseRepository.GetByUuidAsync(courseUuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        var material = await materialRepository.GetMaterialByUuidAsync(materialUuid);

        if (material == null || material.CourseUuid != course.Uuid)
            return NotFound(new { error = "Material not found in the specified course." });


        if (material is FileMaterial fileMaterial)
            await materialAccessService.DeleteFileMaterialAsync(fileMaterial.FileUrl);

        await materialRepository.DeleteMaterialAsync(material);

        return NoContent();
    }

    // Quizzes

    [HttpGet("courses/{courseUuid:guid}/quizzes")]
    public async Task<IActionResult> GetQuizzesByCourseId([FromRoute] Guid courseUuid) {
        var course = await courseRepository.GetByUuidAsync(courseUuid);
        if (course == null) {
            return NotFound(new { error = "Course not found." });
        }

        var quizzes = await db.Quizzes
            .Where(q => q.CourseUuid == courseUuid)
            .Include(q => q.Questions)
            .ThenInclude(qn => qn.Options)
            .ToListAsync();

        var obj = quizzes.Select(quiz => (dynamic)quiz.ToReadDto());

        return Ok(obj);
    }

    [HttpPost("courses/{courseUuid:guid}/quizzes")]
    public async Task<IActionResult> CreateQuizInCourse([FromRoute] Guid courseUuid, [FromBody] CreateUpdateQuizRequest body) {
        var course = await courseRepository.GetByUuidAsyncFull(courseUuid);
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
                        Quiz = newQuiz
                    };

                    for (int i = 0; i < singleDto.Options.Count; i++) {
                        var optionText = singleDto.Options[i];
                        var option = new QuestionOption {
                            Text = optionText,
                            IsCorrect = i == singleDto.CorrectIndex,
                            Question = singleChoiceQuestion
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
                        Quiz = newQuiz
                    };

                    for (int i = 0; i < multipleDto.Options.Count; i++) {
                        var optionText = multipleDto.Options[i];
                        var option = new QuestionOption {
                            Text = optionText,
                            IsCorrect = multipleDto.CorrectIndices.Contains(i),
                            Question = multipleChoiceQuestion
                        };

                        multipleChoiceQuestion.Options.Add(option);
                    }

                    newQuiz.Questions.Add(multipleChoiceQuestion);
                    break;

                default:
                    return BadRequest(new { error = $"Unsupported question type: {questionDto.Type}" });
            }
        }

        db.Quizzes.Add(newQuiz);
        await db.SaveChangesAsync();

        return Ok(newQuiz.ToReadDto());
    }

    [HttpGet("courses/{courseUuid:guid}/quizzes/{quizUuid:guid}")]
    public async Task<IActionResult> GetQuizById([FromRoute] Guid courseUuid, [FromRoute] Guid quizUuid) {
        var course = await courseRepository.GetByUuidAsync(courseUuid);
        if (course == null)
            return NotFound(new { error = "Course not found." });

        var quiz = await db.Quizzes
            .Where(q => q.CourseUuid == courseUuid)
            .Where(q => q.Uuid == quizUuid)
            .Include(q => q.Questions)
            .ThenInclude(qn => qn.Options)
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
        var course = await courseRepository.GetByUuidAsync(courseUuid);
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
                            Quiz = quiz
                        };

                        for (int i = 0; i < dto.Options.Count; i++)
                        {
                            var optionText = dto.Options[i];
                            var option = new QuestionOption
                            {
                                Text = optionText,
                                IsCorrect = i == dto.CorrectIndex,
                                Question = newQuestion
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
                            Question = existingQuestion
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
                            Quiz = quiz
                        };

                        for (int i = 0; i < dto.Options.Count; i++)
                        {
                            var optionText = dto.Options[i];
                            var option = new QuestionOption
                            {
                                Text = optionText,
                                IsCorrect = dto.CorrectIndices.Contains(i),
                                Question = newQuestion
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
                            Question = existingQuestion
                        };
                        
                        db.QuestionOptions.Add(option);
                    }

                    break;
                }

                default:
                    return BadRequest(new { error = $"Unsupported question type: {dtoBase.Type}" });
            }
        }

        await db.SaveChangesAsync();

        return Ok(quiz.ToReadDto());
    }

    [HttpDelete("courses/{courseUuid:guid}/quizzes/{quizUuid:guid}")]
    public async Task<IActionResult> DeleteQuizFromCourse(
        [FromRoute] Guid courseUuid,
        [FromRoute] Guid quizUuid
    ) {
        var course = await courseRepository.GetByUuidAsync(courseUuid);
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
        
        // Remove options and questions
        foreach (var question in quiz.Questions.ToList()) {
            foreach (var option in question.Options.ToList()) {
                db.QuestionOptions.Remove(option);
            }
            db.Questions.Remove(question);
        }
        
        db.Quizzes.Remove(quiz);
        await db.SaveChangesAsync();
        
        return NoContent();
    }
}