namespace ELibraryManagement.Api.Services.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string?> UploadImageAsync(IFormFile file, string folder = "avatars");
        Task<bool> DeleteImageAsync(string publicId);
    }
}