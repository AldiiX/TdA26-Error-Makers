using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using tda26.Server.Data.Models;

namespace tda26.Server.DTOs.v1;

public class UpdateMaterialRequest {
    [MaxLength(128)] public string? Name { get; set; }
    [MaxLength(1048)] public string? Description { get; set; }
}