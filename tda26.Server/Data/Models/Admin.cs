using System.ComponentModel.DataAnnotations.Schema;

namespace tda26.Server.Data.Models;

public sealed class Admin : Account {
	[NotMapped]
	public new AccountType Type => AccountType.Admin;
}