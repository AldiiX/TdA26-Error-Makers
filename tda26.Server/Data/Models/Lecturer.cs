using System.ComponentModel.DataAnnotations;

namespace tda26.Server.Data.Models;

public class Lecturer : Account {
    [MaxLength(10)] public string? TitleBefore { get; set; }
    [MaxLength(32)] public string FirstName { get; set; } = string.Empty;
    [MaxLength(32)] public string? MiddleName { get; set; }
    [MaxLength(32)] public string LastName { get; set; } = string.Empty;
    [MaxLength(10)] public string? TitleAfter { get; set; }
    [MaxLength(1024)] public string? Bio { get; set; }
    [MaxLength(512)] public string? PictureUrl { get; set; }
    [MaxLength(128)] public string? Claim { get; set; }
    [MaxLength(100)] public string? Location { get; set; }
    public ushort PricePerHour { get; set; }
    public ICollection<string> MobileNumbers { get; set; } = new List<string>();
    public ICollection<string> Emails { get; set; } =  new List<string>();
    public ICollection<string> Tags { get; set; } =  new List<string>();
}