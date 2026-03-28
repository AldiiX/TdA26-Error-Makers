using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tda26.Server.Data.Models;

public sealed class Lecturer : Account {

    // db props
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
    public ICollection<Organization> Organizations { get; set; } = new List<Organization>();



    // nemapovany props (pouze pro serializaci)
    [NotMapped]
    public new AccountType Type => AccountType.Lecturer;

    [NotMapped]
    public new string FullName {
        get {
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

    [NotMapped]
    public new string FullNameWithoutTitles {
        get {
            var names = new List<string> { FirstName };
            if (!string.IsNullOrWhiteSpace(MiddleName))
                names.Add(MiddleName);
            names.Add(LastName);
            return string.Join(" ", names);
        }
    }
}