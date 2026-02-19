using System.ComponentModel.DataAnnotations;

namespace tda26.Server.DTOs.v1;

public class UpdateMaterialRequest {
    public Guid? Uuid { get; set; }
    
    [MaxLength(128)] public string? Name { get; set; }
    [MaxLength(1048)] public string? Description { get; set; }
    public bool? IsVisible { get; set; }
}
