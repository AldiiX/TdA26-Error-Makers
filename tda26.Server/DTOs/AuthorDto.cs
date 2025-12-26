namespace tda26.Server.DTOs;

public class AuthorDto {
	public required Guid Uuid { get; set; }
	public required string Username { get; set; }
	public string? PictureUrl { get; set; }
	public string? TitleBefore { get; set; }
	public string? FirstName { get; set; }
	public string? MiddleName { get; set; }
	public string? LastName { get; set; }
	public string? TitleAfter { get; set; }
	public string? Bio { get; set; }

	public string FullName {
		get {
			if(string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
				return Username;

			var names = new List<string>();
			if (!string.IsNullOrWhiteSpace(TitleBefore))
				names.Add(TitleBefore);
			names.Add(FirstName);
			if (!string.IsNullOrWhiteSpace(MiddleName))
				names.Add(MiddleName);
			names.Add(LastName);
			if (!string.IsNullOrWhiteSpace(TitleAfter))
				names.Add(TitleAfter);
			return string.Join(" ", names);
		}
	}

	public string FullNameWithoutTitles {
		get {
			if(string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
				return Username;

			var names = new List<string> { FirstName };
			if (!string.IsNullOrWhiteSpace(MiddleName))
				names.Add(MiddleName);
			names.Add(LastName);
			return string.Join(" ", names);
		}
	}
}