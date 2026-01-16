using System.ComponentModel.DataAnnotations;

namespace tda26.Server.DTOs;

public class AccountSessionDto {
    public Guid Uuid { get; set; } = Guid.NewGuid();
    [MaxLength(32)] public string Username { get; set; } = string.Empty;
    [MaxLength(512)] public string Password { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}