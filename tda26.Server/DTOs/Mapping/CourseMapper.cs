using tda26.Server.Data.Models;
using tda26.Server.DTOs.v1;

namespace tda26.Server.DTOs.Mapping;

public static class CourseMapper {
	public static ReadCourseResponse ToReadDto(this Course course, bool extended = false) {
		// Clear circular references to prevent infinite loop during serialization
		// Following the same pattern as APIv1
		if (course.Account != null) {
			course.Account.Ratings = [];
		}

		return new ReadCourseResponse {
			Uuid = course.Uuid,
			Name = course.Name,
			Description = course.Description,
			CreatedAt = course.CreatedAt,
			UpdatedAt = course.UpdatedAt,
			Status = course.Status,
			ScheduledStart = course.ScheduledStart,
			ImageUrl = course.ImageUrl != null && course.ImageUrl.StartsWith("course-images/")
				? $"/api/v1/courses/{course.Uuid}/image"
				: null,
			Lecturer = course.Lecturer,
			Account = course.Account,
			ViewCount = course.ViewCount,
			LikeCount = course.LikeCount,
			CategoryImageUrl = course.CategoryImageUrl,
			Materials = course.Materials.Select(m => m.ToReadDto()).ToList(),
			Quizzes = course.Quizzes.Select(q => q.ToReadDto(extended)).ToList(),
			Modules = course.Modules
				.OrderBy(m => m.Order)
				.Select(m => new ReadModuleResponse {
					Uuid = m.Uuid,
					Title = m.Title,
					Description = m.Description,
					IsVisible = m.IsVisible,
					Order = m.Order,
					CreatedAt = m.CreatedAt,
					UpdatedAt = m.UpdatedAt,
					Materials = m.Materials.OrderBy(mat => mat.Order).Select(mat => mat.ToReadDto()).ToList(),
					Quizzes = m.Quizzes.OrderBy(q => q.Order).Select(q => q.ToReadDto(extended)).ToList()
				})
				.ToList(),
			Feed = course.Feed,
			RatingScore = course.RatingScore,
			Author = course.Author,
			Category = course.Category,
			Tags = course.Tags.ToList()
		};
	}
}