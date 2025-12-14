using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tda26.Server.Data.Models;

[Table("Categories")]
public class Category : Auditable {
	[Key]
	public Guid Uuid { get; set; } = Guid.NewGuid();

	[MaxLength(64)]
	public string Label { get; set; } = string.Empty;
}