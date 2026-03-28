using System.ComponentModel.DataAnnotations;
using tda26.Server.Data.Models;

namespace tda26.Server.DTOs.v1;

public class UpdateOrganizationRequest {
    [MaxLength(128)]
    public string? DisplayName { get; set; }

    [MaxLength(64)]
    public string? Country { get; set; }

    [MaxLength(128)]
    public string? City { get; set; }

    [MaxLength(256)]
    public string? Address { get; set; }

    [MaxLength(24)]
    public string? PostalCode { get; set; }

    public OrganizationRegion? Region { get; set; }

    public OrganizationType? Type { get; set; }

    public OrganizationStatus? Status { get; set; }

    public ICollection<Guid>? LecturerUuids { get; set; }

    public ICollection<Guid>? StudentUuids { get; set; }
}
