using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using tda26.Server.Data.Models;

namespace tda26.Server.DTOs.v1;

public class CreateMaterialRequest {
    [Required] [MaxLength(128)] public required string Name { get; set; }
    [MaxLength(1048)] public string? Description { get; set; }
    [Required] public required string Type { get; set; }
}