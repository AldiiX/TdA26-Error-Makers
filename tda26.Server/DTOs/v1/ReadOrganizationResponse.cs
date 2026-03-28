using tda26.Server.Data.Models;

namespace tda26.Server.DTOs.v1;

public class ReadOrganizationResponse {
    public required Guid Uuid { get; set; }

    public required string DisplayName { get; set; }

    public required string Country { get; set; }

    public required string City { get; set; }

    public required string Address { get; set; }

    public required string PostalCode { get; set; }

    public required OrganizationRegion Region { get; set; }

    public required OrganizationType Type { get; set; }

    public required OrganizationStatus Status { get; set; }

    public required ICollection<Guid> LecturerUuids { get; set; }

    public required ICollection<Guid> StudentUuids { get; set; }
}
