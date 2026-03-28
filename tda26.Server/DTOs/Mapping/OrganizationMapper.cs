using tda26.Server.Data.Models;
using tda26.Server.DTOs.v1;

namespace tda26.Server.DTOs.Mapping;

public static class OrganizationMapper {
    public static ReadOrganizationResponse ToReadDto(this Organization organization) {
        return new ReadOrganizationResponse {
            Uuid = organization.Uuid,
            DisplayName = organization.DisplayName,
            Country = organization.Country,
            City = organization.City,
            Address = organization.Address,
            PostalCode = organization.PostalCode,
            Region = organization.Region,
            Type = organization.Type,
            Status = organization.Status,
            LecturerUuids = organization.Lecturers.Select(l => l.Uuid).ToList(),
            StudentUuids = organization.Students.Select(s => s.Uuid).ToList()
        };
    }
}
