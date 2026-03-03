using tda26.Server.Data.Models;
using tda26.Server.DTOs.v1;

namespace tda26.Server.DTOs.Mapping;

public static class MaterialMapper
{
    public static ReadMaterialResponse ToReadDto(this Material material) =>
        material switch
        {
            UrlMaterial url => new ReadUrlMaterialResponse
            {
                Uuid = url.Uuid,
                Name = url.Name,
                Description = url.Description,
                Type = "url",
                Url = url.Url,
                FaviconUrl = url.FaviconUrl,
                CreatedAt = url.CreatedAt,
                UpdatedAt = url.UpdatedAt,
                IsVisible = url.IsVisible,
                Order = url.Order
            },

            FileMaterial file => new ReadFileMaterialResponse
            {
                Uuid = file.Uuid,
                Name = file.Name,
                Description = file.Description,
                Type = "file",
                FileUrl = file.FileUrl,
                MimeType = file.MimeType,
                SizeBytes = file.SizeBytes,
                CreatedAt = file.CreatedAt,
                UpdatedAt = file.UpdatedAt,
                IsVisible = file.IsVisible,
                Order = file.Order
            },

            _ => throw new InvalidOperationException($"Unknown material type: {material.GetType().Name}")
        };
}