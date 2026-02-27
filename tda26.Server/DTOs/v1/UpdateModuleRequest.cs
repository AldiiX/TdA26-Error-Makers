using System.ComponentModel.DataAnnotations;

namespace tda26.Server.DTOs.v1;

public class UpdateModuleRequest {
    [MaxLength(128)]
    public string? Title { get; set; }

    [MaxLength(1048)]
    public string? Description { get; set; }

    public bool? IsVisible { get; set; }

    public int? Order { get; set; }
}
