using System.ComponentModel.DataAnnotations;

namespace tda26.Server.DTOs.v1;

public class LikeCourseRequest {
	[MaxLength(64)] public string? Type { get; set; }
}