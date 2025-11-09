using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace tda26.Server.Classes.Objects;

public class Lecturer {
    public Guid Uuid { get; private set; }
    public string? TitleBefore { get; private set; }
    public string FirstName { get; private set; }
    public string? MiddleName { get; private set; }
    public string LastName { get; private set; }
    public string? TitleAfter { get; private set; }
    public string? Bio { get; private set; }
    public string? PictureUrl { get; private set; }
    public string? Claim { get; private set; }
    public ushort PricePerHour { get; private set; }
    public List<string> MobileNumbers { get; private set; }
    public List<string> Emails { get; private set; }
    public List<string> Tags { get; private set; }
    public string? Location { get; private set; }
    public DateTime MemberSince { get; private set; }

    [JsonConstructor]
    public Lecturer(Guid uuid, string? titleBefore, string firstName, string? middleName, string lastName,
        string? titleAfter, string? bio, string? pictureUrl, string? claim, ushort pricePerHour,
        List<string> mobileNumbers, List<string> emails, List<string> tags, string? location, DateTime memberSince)
    {
        Uuid = uuid;
        TitleBefore = titleBefore;
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        TitleAfter = titleAfter;
        Bio = bio;
        PictureUrl = pictureUrl;
        Claim = claim;
        PricePerHour = pricePerHour;
        MobileNumbers = mobileNumbers;
        Emails = emails;
        Tags = tags;
        Location = location;
        MemberSince = memberSince;
    }

    public JsonNode ToJsonNode() {
        var json = new JsonObject {
            ["uuid"] = Uuid.ToString(),
            ["titleBefore"] = TitleBefore,
            ["firstName"] = FirstName,
            ["middleName"] = MiddleName,
            ["lastName"] = LastName,
            ["titleAfter"] = TitleAfter,
            ["bio"] = Bio,
            ["pictureUrl"] = PictureUrl,
            ["claim"] = Claim,
            ["pricePerHour"] = PricePerHour,
            ["mobileNumbers"] = JsonNode.Parse(System.Text.Json.JsonSerializer.Serialize(MobileNumbers)),
            ["emails"] = JsonNode.Parse(System.Text.Json.JsonSerializer.Serialize(Emails)),
            ["tags"] = JsonNode.Parse(System.Text.Json.JsonSerializer.Serialize(Tags)),
            ["location"] = Location,
            ["memberSince"] = MemberSince.ToString("o") // ISO 8601 format
        };

        return json;
    }
}