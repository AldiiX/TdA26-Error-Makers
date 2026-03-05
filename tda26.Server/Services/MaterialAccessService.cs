using System.Reactive.Linq;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using tda26.Server.Options;

namespace tda26.Server.Services;

public sealed class MaterialAccessService(
	IMinioClient minioClient,
	IOptions<CustomMinioOptions> minioOptions
) : IMaterialAccessService {
	private const string PublicUrlPrefix = "/files/";

	public async Task<string> UploadFileMaterialAsync(Guid courseUuid, IFormFile file, CancellationToken ct = default) {
		var fileUuid = Guid.NewGuid();
		var fileExtension = Path.GetExtension(file.FileName);
		var objectName = $"materials/{courseUuid}/{fileUuid}{fileExtension}";

		try {
			var args = new PutObjectArgs()
				.WithBucket(minioOptions.Value.BucketName)
				.WithObject(objectName)
				.WithStreamData(file.OpenReadStream())
				.WithObjectSize(file.Length)
				.WithContentType(file.ContentType);

			await minioClient.PutObjectAsync(args, ct).ConfigureAwait(false);
		} catch (Exception ex) {
			throw new Exception($"File upload to MinIO failed: {ex.Message}", ex);
		}

		return objectName;
	}

	public async Task<string> UploadCourseImageAsync(Guid courseUuid, IFormFile image, CancellationToken ct = default) {
		var fileExtension = Path.GetExtension(image.FileName);
		var objectName = $"course-images/{courseUuid}{fileExtension}";

		try {
			var args = new PutObjectArgs()
				.WithBucket(minioOptions.Value.BucketName)
				.WithObject(objectName)
				.WithStreamData(image.OpenReadStream())
				.WithObjectSize(image.Length)
				.WithContentType(image.ContentType);

			await minioClient.PutObjectAsync(args, ct).ConfigureAwait(false);
		} catch (Exception ex) {
			throw new Exception($"Course image upload to MinIO failed: {ex.Message}", ex);
		}

		return objectName;
	}

	public async Task DeleteFileMaterialAsync(string fileUrl, CancellationToken ct = default) {
		await minioClient.RemoveObjectAsync(
			new RemoveObjectArgs()
				.WithBucket(minioOptions.Value.BucketName)
				.WithObject(fileUrl),
			ct
		);
	}

	public async Task<MemoryStream> DownloadFileMaterialAsync(string fileUrl, CancellationToken ct = default) {
		// Retry logic for transient access denied errors (for some reason after upload it doesn't have access for the first access)
		var retries = 3;
		var delay = TimeSpan.FromMilliseconds(150);

		while (true)
			try {
				var ms = new MemoryStream();

				await minioClient.GetObjectAsync(
					new GetObjectArgs()
						.WithBucket(minioOptions.Value.BucketName)
						.WithObject(fileUrl)
						.WithCallbackStream(stream => stream.CopyTo(ms)),
					ct
				);

				ms.Position = 0;
				return ms;
			} catch (Minio.Exceptions.AccessDeniedException) when (retries-- > 0) {
				await Task.Delay(delay, ct);
				delay += delay;
			} catch (Minio.Exceptions.ObjectNotFoundException) {
				Console.WriteLine("Object not found in MinIO: " + fileUrl);
			}
	}

	public async Task CopyCourseMaterialsDirectoryAsync(Guid sourceCourseUuid, Guid targetCourseUuid, CancellationToken ct = default) {
		var sourcePrefix = $"materials/{sourceCourseUuid}/";
		var targetPrefix = $"materials/{targetCourseUuid}/";

		var listArgs = new ListObjectsArgs()
			.WithBucket(minioOptions.Value.BucketName)
			.WithPrefix(sourcePrefix)
			.WithRecursive(true);

		var items = await minioClient
			.ListObjectsAsync(listArgs)
			.Where(i => !i.IsDir && !string.IsNullOrEmpty(i.Key))
			.ToList();

		foreach (var item in items) {
			var targetKey = targetPrefix + item.Key[sourcePrefix.Length..];

			var copySourceArgs = new CopySourceObjectArgs()
				.WithBucket(minioOptions.Value.BucketName)
				.WithObject(item.Key);

			var copyArgs = new CopyObjectArgs()
				.WithBucket(minioOptions.Value.BucketName)
				.WithObject(targetKey)
				.WithCopyObjectSource(copySourceArgs);

			await minioClient.CopyObjectAsync(copyArgs, ct);
		}
	}

	public async Task<string> CopyFileAsync(string sourceKey, string targetKey, CancellationToken ct = default) {
		if (string.IsNullOrWhiteSpace(sourceKey)) {
			throw new ArgumentException("Source key is required.", nameof(sourceKey));
		}

		if (string.IsNullOrWhiteSpace(targetKey)) {
			throw new ArgumentException("Target key is required.", nameof(targetKey));
		}

		var copySourceArgs = new CopySourceObjectArgs()
			.WithBucket(minioOptions.Value.BucketName)
			.WithObject(sourceKey);

		var copyArgs = new CopyObjectArgs()
			.WithBucket(minioOptions.Value.BucketName)
			.WithObject(targetKey)
			.WithCopyObjectSource(copySourceArgs);

		await minioClient.CopyObjectAsync(copyArgs, ct);

		return targetKey;
	}

}