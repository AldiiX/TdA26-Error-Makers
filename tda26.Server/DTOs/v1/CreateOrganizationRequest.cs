using System.ComponentModel.DataAnnotations;
using tda26.Server.Data.Models;

namespace tda26.Server.DTOs.v1;

public class CreateOrganizationRequest {
    [Required, MaxLength(128)]
    public required string DisplayName { get; set; }

    [Required, MaxLength(64)]
    public required string Country { get; set; }

    [Required, MaxLength(128)]
    public required string City { get; set; }

    [Required, MaxLength(256)]
    public required string Address { get; set; }

    [Required, MaxLength(24)]
    public required string PostalCode { get; set; }

    public OrganizationRegion Region { get; set; } = OrganizationRegion.CentralEurope;

    public OrganizationType Type { get; set; } = OrganizationType.Hq;

    public OrganizationStatus Status { get; set; } = OrganizationStatus.Onboarding;

    public ICollection<Guid> LecturerUuids { get; set; } = new List<Guid>();

    public ICollection<Guid> StudentUuids { get; set; } = new List<Guid>();
}
