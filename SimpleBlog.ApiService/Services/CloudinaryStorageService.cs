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
                Overwrite = true,
                PublicId = Path.GetFileNameWithoutExtension(fileName),
                Type = "private"  // Private upload type - requires signed URLs
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

            // For private images, return only the public_id with a marker prefix
            // We'll generate signed URLs on-demand when retrieving settings
            // Format: cloudinary://public_id
            return uploadResult.Type == "private" 
                ? $"cloudinary://{uploadResult.PublicId}"
                : uploadResult.SecureUrl.ToString();
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
            // Check if this is our internal format (cloudinary://public_id) or legacy format
            var publicId = imageUrl.StartsWith("cloudinary://")
                ? imageUrl["cloudinary://".Length..]
                : ExtractPublicIdFromUrl(imageUrl) ?? string.Empty;
            
            if (string.IsNullOrEmpty(publicId))
            {
                logger.LogWarning(
                    "Could not extract public ID from URL: {ImageUrl}",
                    imageUrl);
                return false;
            }

            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image,
                Type = "private" // Try private first, will fall back if needed
            };
            
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

    public string GenerateSignedUrl(string imageUrl, int expirationMinutes = 60)
    {
        ArgumentNullException.ThrowIfNull(imageUrl);

        try
        {
            string publicId;
            string? format = null;
            string deliveryType;

            // Check if this is our internal format (cloudinary://public_id)
            if (imageUrl.StartsWith("cloudinary://"))
            {
                publicId = imageUrl["cloudinary://".Length..];
                deliveryType = "private"; // Our internal format is always private
                
                // Extract format if present in public_id
                var lastDot = publicId.LastIndexOf('.');
                if (lastDot > 0)
                {
                    format = publicId[(lastDot + 1)..];
                    publicId = publicId[..lastDot];
                }
            }
            else
            {
                // Legacy format - full URL
                publicId = ExtractPublicIdFromUrl(imageUrl) ?? string.Empty;
                if (string.IsNullOrEmpty(publicId))
                {
                    logger.LogWarning(
                        "Could not extract public ID from URL for signing: {ImageUrl}",
                        imageUrl);
                    return imageUrl; // Fallback to original URL
                }

                deliveryType = ExtractDeliveryTypeFromUrl(imageUrl);
                format = ExtractFormatFromUrl(imageUrl);
            }

            // Generate signed URL using Cloudinary SDK
            var urlBuilder = cloudinary.Api.UrlImgUp
                .Type(deliveryType)
                .Secure()
                .Signed(true);

            // Add format if available
            if (!string.IsNullOrEmpty(format))
            {
                urlBuilder = urlBuilder.Format(format);
            }

            var signedUrl = urlBuilder.BuildUrl(publicId);

            logger.LogDebug(
                "Generated signed URL for {PublicId} with type {Type}",
                publicId,
                deliveryType);

            return signedUrl;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error generating signed URL for: {ImageUrl}",
                imageUrl);
            return imageUrl; // Fallback to original URL
        }
    }

    private static string ExtractDeliveryTypeFromUrl(string imageUrl)
    {
        // Extract delivery type (upload, private, authenticated) from URL
        var uri = new Uri(imageUrl);
        var segments = uri.AbsolutePath.Split('/');
        
        var typeIndex = Array.FindIndex(segments, s => s is "upload" or "private" or "authenticated");
        return typeIndex >= 0 ? segments[typeIndex] : "upload";
    }

    private static string? ExtractPublicIdFromUrl(string imageUrl)
    {
        // URL format: https://res.cloudinary.com/{cloud_name}/image/upload/v{version}/{public_id}.{format}
        // or for private: https://res.cloudinary.com/{cloud_name}/image/private/v{version}/{public_id}.{format}
        // We need to extract the public_id part (without version and extension)
        
        var uri = new Uri(imageUrl);
        var segments = uri.AbsolutePath.Split('/');
        
        // Find "upload" or "private" segment
        var typeIndex = Array.FindIndex(segments, s => s is "upload" or "private");
        if (typeIndex == -1 || typeIndex + 1 >= segments.Length)
            return null;

        // Collect all parts after type (including version)
        // Example: ["v1768682471", "dev_simpleblog", "logos", "1_ettnpg.png"]
        var parts = segments[(typeIndex + 1)..];
        
        // Skip version segment (starts with 'v' followed by digits)
        var publicIdParts = parts.Length > 0 && parts[0].StartsWith('v') && parts[0].Length > 1
            ? parts[1..]
            : parts;

        var publicIdWithExtension = string.Join("/", publicIdParts);
        
        // Remove file extension
        var lastDotIndex = publicIdWithExtension.LastIndexOf('.');
        return lastDotIndex > 0 
            ? publicIdWithExtension[..lastDotIndex] 
            : publicIdWithExtension;
    }

    private static string? ExtractFormatFromUrl(string imageUrl)
    {
        // Extract file extension (format) from URL
        var uri = new Uri(imageUrl);
        var path = uri.AbsolutePath;
        var lastDotIndex = path.LastIndexOf('.');
        
        if (lastDotIndex > 0 && lastDotIndex < path.Length - 1)
        {
            return path[(lastDotIndex + 1)..];
        }
        
        return null;
    }
}
