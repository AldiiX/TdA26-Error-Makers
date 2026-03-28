using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace tda26.Server.Data.Models;

[Table("ShopItems")]
[PrimaryKey(nameof(Uuid))]
public class ShopItem : Auditable {

	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Uuid { get; set; }

	[MaxLength(128)]
	public required string Name { get; set; }

	[MaxLength(512)]
	public required string Description { get; set; }

	public required int PriceInDucks { get; set; }

	public ICollection<Account> OwnedByAccounts { get; set; } = new List<Account>();
}