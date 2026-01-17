using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using SimpleBlog.Common.Interfaces;

namespace SimpleBlog.ApiService.Services;

public sealed class CloudinaryStorageService(
    Cloudinary cloudinary,
    IConfiguration configuration,
    ILogger<CloudinaryStorageService> logger) : IImageStorageService
{
    private readonly string _rootFolder = configuration["Cloudinary:RootFolder"] ?? "simpleblog";

    public async Task<string> UploadImageAsync(
        Stream stream,
        string fileName,
        string folder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(folder);

        try
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream),
                Folder = $"{_rootFolder}/{folder}",
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = true
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams, cancellationToken);

            if (uploadResult.Error is not null)
            {
                logger.LogError(
                    "Cloudinary upload failed: {ErrorMessage}",
                    uploadResult.Error.Message);
                throw new InvalidOperationException(
                    $"Image upload failed: {uploadResult.Error.Message}");
            }

            logger.LogInformation(
                "Image uploaded successfully to Cloudinary: {PublicId}",
                uploadResult.PublicId);

            return uploadResult.SecureUrl.ToString();
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            logger.LogError(
                ex,
                "Unexpected error uploading image to Cloudinary");
            throw new InvalidOperationException(
                "Failed to upload image",
                ex);
        }
    }

    public async Task<bool> DeleteImageAsync(
        string imageUrl,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageUrl);

        try
        {
            // Extract public ID from Cloudinary URL
            // Example: https://res.cloudinary.com/demo/image/upload/v1234567890/simpleblog/logos/logo.jpg
            var publicId = ExtractPublicIdFromUrl(imageUrl);
            
            if (string.IsNullOrEmpty(publicId))
            {
                logger.LogWarning(
                    "Could not extract public ID from URL: {ImageUrl}",
                    imageUrl);
                return false;
            }

            var deletionParams = new DeletionParams(publicId);
            var deletionResult = await cloudinary.DestroyAsync(deletionParams);

            var success = deletionResult.Result == "ok";
            
            if (success)
            {
                logger.LogInformation(
                    "Image deleted successfully from Cloudinary: {PublicId}",
                    publicId);
            }
            else
            {
                logger.LogWarning(
                    "Failed to delete image from Cloudinary: {PublicId}, Result: {Result}",
                    publicId,
                    deletionResult.Result);
            }

            return success;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error deleting image from Cloudinary: {ImageUrl}",
                imageUrl);
            return false;
        }
    }

    private static string? ExtractPublicIdFromUrl(string imageUrl)
    {
        // URL format: https://res.cloudinary.com/{cloud_name}/image/upload/v{version}/{public_id}.{format}
        // We need to extract the public_id part
        
        var uri = new Uri(imageUrl);
        var segments = uri.AbsolutePath.Split('/');
        
        // Find "upload" segment and take everything after version number
        var uploadIndex = Array.IndexOf(segments, "upload");
        if (uploadIndex == -1 || uploadIndex + 2 >= segments.Length)
            return null;

        // Skip "upload" and version (v1234567890)
        var publicIdParts = segments[(uploadIndex + 2)..];
        var publicIdWithExtension = string.Join("/", publicIdParts);
        
        // Remove file extension
        var lastDotIndex = publicIdWithExtension.LastIndexOf('.');
        return lastDotIndex > 0 
            ? publicIdWithExtension[..lastDotIndex] 
            : publicIdWithExtension;
    }
}
