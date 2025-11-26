using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace tda26.Server.Data.Models;

public partial class Course {
	[Table("Likes")]
	[Index(nameof(AccountUuid), nameof(CourseUuid), IsUnique = true)]
	public class Like : Auditable {
		[Key]
		public Guid Uuid { get; set; } = Guid.NewGuid();

		[JsonIgnore, Required]
		public Guid AccountUuid { get; set; }

		[ForeignKey("AccountUuid"), Required]
		public Account Account { get; set; } = null!;

		[JsonIgnore, Required]
		public Guid CourseUuid { get; set; }

		[ForeignKey("CourseUuid"), JsonIgnore]
		public Course Course { get; set; } = null!;
	}
}