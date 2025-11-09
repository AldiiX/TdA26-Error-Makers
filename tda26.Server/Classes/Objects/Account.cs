using System.Text.Json.Serialization;

namespace tda26.Server.Classes.Objects;

public class Account : Lecturer {
    public string Username { get; private set; }
    public string Password { get; private set; }

    [JsonConstructor]
    public Account(Guid uuid, string username, string password, string? titleBefore, string firstName, string? middleName, string lastName, string? titleAfter, string? bio, string? pictureUrl, string? claim, ushort pricePerHour, List<string> mobileNumbers, List<string> emails, List<string> tags, string? location, DateTime memberSince) : base(uuid, titleBefore, firstName, middleName, lastName, titleAfter, bio, pictureUrl, claim, pricePerHour, mobileNumbers, emails, tags, location, memberSince) {
        Username = username;
        Password = password;
    }
}