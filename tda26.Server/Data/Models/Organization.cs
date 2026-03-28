using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using tda26.Server.DTOs.Converters;

namespace tda26.Server.Data.Models;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum OrganizationRegion {
    CentralEurope,
    Europe,
    NorthAmerica,
    SouthAmerica,
    Asia,
    Africa,
    Oceania,
    MiddleEast,
    Global
}

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum OrganizationType {
    Hq,
    Branch
}

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum OrganizationStatus {
    Active,
    Onboarding,
    Waiting
}

public sealed class Organization {
    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Country { get; set; } = string.Empty;

    [MaxLength(128)]
    public string City { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(24)]
    public string PostalCode { get; set; } = string.Empty;

    public OrganizationRegion Region { get; set; } = OrganizationRegion.CentralEurope;

    public OrganizationType Type { get; set; } = OrganizationType.Hq;

    public OrganizationStatus Status { get; set; } = OrganizationStatus.Onboarding;

}