using System.ComponentModel.DataAnnotations;

namespace tda26.Server.DTOs.v1;

public class UpdateMaterialRequest {
    [MaxLength(128)] public string? Name { get; set; }
    [MaxLength(1048)] public string? Description { get; set; }
}