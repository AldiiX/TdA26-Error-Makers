using System.ComponentModel.DataAnnotations;

namespace tda26.Server.DTOs.v1;

public class CreateCourseRequest {
    [Required] [MaxLength(128)] public string Name { get; set; } = string.Empty;
    [MaxLength(1048)] public string Description { get; set; } = string.Empty;
}