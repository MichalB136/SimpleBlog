namespace SimpleBlog.Common.Interfaces;

/// <summary>
/// Service for storing and managing images (logos, post images, etc.)
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// Upload an image and return its public URL
    /// </summary>
    /// <param name="stream">Image file stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="folder">Folder/category for organization (e.g., "logos", "posts")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Public URL of the uploaded image</returns>
    Task<string> UploadImageAsync(
        Stream stream,
        string fileName,
        string folder,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an image by its URL
    /// </summary>
    /// <param name="imageUrl">Public URL of the image to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteImageAsync(
        string imageUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a signed URL for private image access
    /// </summary>
    /// <param name="imageUrl">Original image URL</param>
    /// <param name="expirationMinutes">URL expiration time in minutes (default: 60)</param>
    /// <returns>Signed URL valid for specified duration</returns>
    string GenerateSignedUrl(string imageUrl, int expirationMinutes = 60);
}
