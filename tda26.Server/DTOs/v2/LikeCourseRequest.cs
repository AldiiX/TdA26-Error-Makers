using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace tda26.Server.DTOs.v2;

public class LikeCourseRequest {
	[MaxLength(64)] public string? Type { get; set; }
}