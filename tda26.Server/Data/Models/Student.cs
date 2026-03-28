using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tda26.Server.Data.Models;

public sealed class Student : Account {
	[MaxLength(32)] public string FirstName { get; set; } = string.Empty;
	[MaxLength(32)] public string? MiddleName { get; set; }
	[MaxLength(32)] public string LastName { get; set; } = string.Empty;
	[MaxLength(1024)] public string? Bio { get; set; }
	[MaxLength(512)] public string? PictureUrl { get; set; }
	public ICollection<Organization> Organizations { get; set; } = new List<Organization>();
	
	[NotMapped]
	public new AccountType Type => AccountType.Student;
}