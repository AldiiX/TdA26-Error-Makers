using System.ComponentModel.DataAnnotations;

namespace tda26.Server.DTOs.v1;

public class CreateModuleRequest {
    [Required, MaxLength(128)]
    public required string Title { get; set; }

    [MaxLength(1048)]
    public string? Description { get; set; }

    public bool IsVisible { get; set; } = false;

    public int Order { get; set; } = 0;
}
