using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ELibraryManagement.Api.Services.Interfaces;

namespace ELibraryManagement.Api.Services.Implementations
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(Cloudinary cloudinary, ILogger<CloudinaryService> logger)
        {
            _cloudinary = cloudinary;
            _logger = logger;
        }

        public async Task<string?> UploadImageAsync(IFormFile file, string folder = "avatars")
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("No file provided for upload");
                    return null;
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    _logger.LogWarning("Invalid file type: {ContentType}", file.ContentType);
                    return null;
                }

                // Validate file size (5MB max)
                if (file.Length > 5 * 1024 * 1024)
                {
                    _logger.LogWarning("File too large: {Size} bytes", file.Length);
                    return null;
                }

                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = $"elibrary/{folder}",
                    Transformation = new Transformation()
                        .Width(400)
                        .Height(400)
                        .Crop("fill")
                        .Gravity("face")
                        .Quality("auto"),
                    PublicId = $"{folder}_{Guid.NewGuid()}",
                    Format = "webp"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                    return null;
                }

                _logger.LogInformation("Successfully uploaded image: {PublicId}", uploadResult.PublicId);
                return uploadResult.SecureUrl.AbsoluteUri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Cloudinary");
                return null;
            }
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            try
            {
                if (string.IsNullOrEmpty(publicId))
                {
                    return false;
                }

                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from Cloudinary: {PublicId}", publicId);
                return false;
            }
        }
    }
}