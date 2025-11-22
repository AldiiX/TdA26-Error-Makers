using System.ComponentModel.DataAnnotations;

namespace tda26.Server.DTOs.v2;

public class AuthLoginRequest {
    [Required] [MaxLength(32)] public string Username { get; set; } = string.Empty;
    [Required] [MaxLength(128)] public string Password { get; set; } = string.Empty;
}