using System.ComponentModel.DataAnnotations;

namespace tda26.Server.DTOs.v2;

public class AuthRegisterRequest {
    [Required] [MaxLength(64)] public string Username { get; set; } = string.Empty;
    
    [Required] [MaxLength(64)] public string Email { get; set; } = string.Empty;
    [Required] [MaxLength(64)] public string Password { get; set; } = string.Empty;
}