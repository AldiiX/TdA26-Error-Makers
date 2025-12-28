namespace tda26.Server.Services;

public interface IMaterialAccessService {
    /// <summary>
    /// Uploads a file material for a specific course and returns the URL of the uploaded file.
    /// </summary>
    /// <param name="courseUuid">The UUID of the course to which the file material belongs.</param>
    /// <param name="file">The file to be uploaded.</param>
    /// <returns>The URL of the uploaded file material.</returns>
    Task<string> UploadFileMaterialAsync(Guid courseUuid, IFormFile file, CancellationToken ct = default);
    
    Task<string> UploadCourseImageAsync(Guid courseUuid, IFormFile image, CancellationToken ct = default);
    
    Task DeleteFileMaterialAsync(string fileUrl, CancellationToken ct = default);
    
    Task<MemoryStream> DownloadFileMaterialAsync(string fileUrl, CancellationToken ct = default);
}