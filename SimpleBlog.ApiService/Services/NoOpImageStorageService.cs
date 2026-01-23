using SimpleBlog.Common.Interfaces;

namespace SimpleBlog.ApiService.Services;

internal sealed class NoOpImageStorageService : IImageStorageService
{
    public Task<string> UploadImageAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        // Return a placeholder or empty string; callers should handle missing uploads gracefully
        return Task.FromResult(string.Empty);
    }

    public Task<bool> DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public string GenerateSignedUrl(string imageUrl, int expirationMinutes = 60)
    {
        return imageUrl;
    }
}
