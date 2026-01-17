# Cloudinary Integration

> ## metadane dokumentu
> 
> ### ✅ wymagane
> **Tytuł:** Cloudinary Integration  
> **Opis:** Kompleksowy przewodnik po integracji Cloudinary dla zarządzania obrazami w SimpleBlog  
> **Audience:** developer, architect  
> **Topic:** technical  
> **Last Update:** 2026-01-17
>
> ### 📌 rekomendowane
> **Parent Document:** [README.md](./README.md)  
> **Difficulty:** intermediate  
> **Estimated Time:** 25 min  
> **Version:** 1.0.0  
> **Status:** approved
>
> ### 🏷️ opcjonalne
> **Prerequisites:** Podstawowa znajomość .NET, REST APIs, cloud storage  
> **Related Docs:** [architecture-overview.md](./architecture-overview.md)  
> **Tags:** `cloudinary`, `image-storage`, `cloud`, `cdn`, `media-management`

---

## 📋 przegląd

SimpleBlog wykorzystuje Cloudinary jako zewnętrzny serwis do przechowywania i dostarczania obrazów. Integracja obejmuje upload logo strony oraz obrazów do postów, z automatycznym CDN i optymalizacją.

---

## 🎯 architektura

```mermaid
graph LR
    subgraph "SimpleBlog Application"
        Admin[Admin Panel]
        Post[Post Editor]
        API[ApiService]
    end
    
    subgraph "Storage Layer"
        Interface[IImageStorageService]
        Service[CloudinaryStorageService]
    end
    
    subgraph "Cloudinary Cloud"
        Upload[Upload API]
        Storage[Cloud Storage]
        CDN[Global CDN]
    end
    
    Admin --> API
    Post --> API
    API --> Interface
    Interface --> Service
    Service --> Upload
    Upload --> Storage
    Storage --> CDN
    CDN --> Users[End Users]
```

---

## 🔧 konfiguracja

### Metoda 1: CLOUDINARY_URL (zalecana)

Najprostszy sposób - pojedyncza zmienna środowiskowa:

```powershell
# Format: cloudinary://api_key:api_secret@cloud_name
$ENV:CLOUDINARY_URL = 'cloudinary://882298434744922:YOUR_API_SECRET@dy4qslml5'
```

### Metoda 2: Osobne zmienne

Alternatywny sposób z osobnymi zmiennymi:

```powershell
$ENV:SimpleBlog_Cloudinary__CloudName = 'dy4qslml5'
$ENV:SimpleBlog_Cloudinary__ApiKey = 'YOUR_API_KEY'
$ENV:SimpleBlog_Cloudinary__ApiSecret = 'YOUR_API_SECRET'
$ENV:SimpleBlog_Cloudinary__RootFolder = 'simpleblog'  # opcjonalne
```

### appsettings.json

```json
{
  "Cloudinary": {
    "CloudName": "dy4qslml5",
    "ApiKey": "882298434744922",
    "ApiSecret": "",
    "RootFolder": "simpleblog"
  }
}
```

⚠️ **Uwaga:** Nie umieszczaj API Secret w repozytorium! Używaj zmiennych środowiskowych.

---

## 💻 implementacja

### IImageStorageService Interface

Interfejs abstrakcji dla storage'u obrazów:

```csharp
// SimpleBlog.Common/Interfaces/IImageStorageService.cs
public interface IImageStorageService
{
    /// <summary>
    /// Uploads an image to cloud storage
    /// </summary>
    /// <param name="stream">Image stream</param>
    /// <param name="fileName">File name (without extension if unique)</param>
    /// <param name="folder">Subfolder (e.g., "logos", "posts")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Public URL of uploaded image</returns>
    Task<string> UploadImageAsync(
        Stream stream, 
        string fileName, 
        string folder, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an image from cloud storage
    /// </summary>
    /// <param name="imageUrl">Full image URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteImageAsync(
        string imageUrl, 
        CancellationToken cancellationToken = default);
}
```

### CloudinaryStorageService Implementation

```csharp
// SimpleBlog.ApiService/Services/CloudinaryStorageService.cs
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
                UniqueFilename = false,  // For fixed names like "logo"
                Overwrite = true         // Replace existing file
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams, cancellationToken);

            if (uploadResult.Error is not null)
            {
                logger.LogError("Cloudinary upload failed: {ErrorMessage}", 
                    uploadResult.Error.Message);
                throw new InvalidOperationException(
                    $"Image upload failed: {uploadResult.Error.Message}");
            }

            logger.LogInformation("Image uploaded successfully to Cloudinary: {PublicId}", 
                uploadResult.PublicId);

            return uploadResult.SecureUrl.ToString();
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            logger.LogError(ex, "Unexpected error uploading image to Cloudinary");
            throw new InvalidOperationException("Failed to upload image", ex);
        }
    }

    public async Task<bool> DeleteImageAsync(
        string imageUrl,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageUrl);

        try
        {
            var publicId = ExtractPublicIdFromUrl(imageUrl);
            if (string.IsNullOrEmpty(publicId))
            {
                logger.LogWarning("Could not extract public ID from URL: {Url}", imageUrl);
                return false;
            }

            var deletionParams = new DeletionParams(publicId);
            var result = await cloudinary.DestroyAsync(deletionParams);

            logger.LogInformation("Image deleted from Cloudinary: {PublicId}, Result: {Result}", 
                publicId, result.Result);

            return result.Result == "ok";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting image from Cloudinary: {Url}", imageUrl);
            return false;
        }
    }

    private static string? ExtractPublicIdFromUrl(string imageUrl)
    {
        // URL format: https://res.cloudinary.com/{cloud_name}/image/upload/v{version}/{folder}/{public_id}.{ext}
        var uri = new Uri(imageUrl);
        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        var uploadIndex = Array.IndexOf(segments, "upload");
        if (uploadIndex < 0 || uploadIndex + 2 >= segments.Length)
            return null;

        // Skip version segment (v1234567890) and get remaining path
        var pathSegments = segments.Skip(uploadIndex + 2);
        var publicIdWithExt = string.Join("/", pathSegments);
        
        // Remove file extension
        var lastDotIndex = publicIdWithExt.LastIndexOf('.');
        return lastDotIndex > 0 ? publicIdWithExt[..lastDotIndex] : publicIdWithExt;
    }
}
```

### Dependency Injection Setup

```csharp
// SimpleBlog.ApiService/Program.cs
var cloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL");
Cloudinary? cloudinary = null;
var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

if (!string.IsNullOrEmpty(cloudinaryUrl))
{
    // Use CLOUDINARY_URL format: cloudinary://api_key:api_secret@cloud_name
    cloudinary = new Cloudinary(cloudinaryUrl);
    cloudinary.Api.Secure = true; // Use HTTPS URLs
    builder.Services.AddSingleton(cloudinary);
    builder.Services.AddScoped<IImageStorageService, CloudinaryStorageService>();
    logger.LogInformation("Cloudinary configured from CLOUDINARY_URL");
}
else
{
    // Fallback to individual settings
    var cloudName = builder.Configuration["Cloudinary:CloudName"];
    var apiKey = builder.Configuration["Cloudinary:ApiKey"];
    var apiSecret = builder.Configuration["Cloudinary:ApiSecret"];

    if (!string.IsNullOrEmpty(cloudName) && !string.IsNullOrEmpty(apiKey) && 
        !string.IsNullOrEmpty(apiSecret))
    {
        var account = new Account(cloudName, apiKey, apiSecret);
        cloudinary = new Cloudinary(account);
        cloudinary.Api.Secure = true;
        builder.Services.AddSingleton(cloudinary);
        builder.Services.AddScoped<IImageStorageService, CloudinaryStorageService>();
        logger.LogInformation("Cloudinary configured with CloudName: {CloudName}", cloudName);
    }
    else
    {
        logger.LogWarning("Cloudinary not configured. Image upload features will not be available.");
    }
}
```

---

## 📁 struktura folderów

Obrazy są organizowane w następującej strukturze:

```
cloudinary://dy4qslml5/
└── simpleblog/              (RootFolder - konfigurowalne)
    ├── logos/               (Logo strony)
    │   └── logo             (Stała nazwa, zawsze jeden plik)
    └── posts/               (Obrazy w postach)
        └── {unique_id}      (Unikalne nazwy dla każdego obrazu)
```

### Logo Management

Logo zawsze ma **stałą nazwę** `"logo"` i jest **automatycznie nadpisywane**:

```csharp
// SimpleBlog.ApiService/Endpoints/SiteSettingsEndpoints.cs
private static async Task<IResult> UploadLogo(
    IFormFile file,
    ISiteSettingsRepository repository,
    IImageStorageService imageStorage,
    HttpContext context,
    ILogger<Program> logger,
    CancellationToken ct)
{
    // Upload new logo with fixed name 'logo'
    await using var stream = file.OpenReadStream();
    var logoUrl = await imageStorage.UploadImageAsync(stream, "logo", "logos", ct);
    
    var username = context.User.Identity?.Name ?? "Unknown";
    var settings = await repository.UpdateLogoAsync(logoUrl, username, ct);
    
    return Results.Ok(settings);
}
```

**Zalety tego podejścia:**
- ✅ Zawsze jedno aktywne logo
- ✅ Proste URL: `simpleblog/logos/logo`
- ✅ Brak niepotrzebnych starych wersji
- ✅ Automatyczne nadpisywanie

### Post Images

Obrazy do postów używają **unikalnych nazw** z losowym identyfikatorem:

```csharp
// UniqueFilename = true dla obrazów w postach
var uploadParams = new ImageUploadParams
{
    File = new FileDescription(fileName, stream),
    Folder = $"{_rootFolder}/posts",
    UseFilename = true,
    UniqueFilename = true,    // Dodaje losowy sufiks
    Overwrite = false         // Nie nadpisuje istniejących
};
```

---

## 🔒 walidacja i bezpieczeństwo

### Upload Constraints

```csharp
// Logo upload limits
if (file.Length > 5 * 1024 * 1024) // 5 MB
    return Results.BadRequest(new { error = "File size cannot exceed 5 MB" });

// Post image limits
if (file.Length > 10 * 1024 * 1024) // 10 MB
    return Results.BadRequest(new { error = "File size cannot exceed 10 MB" });

// Allowed MIME types
var allowedTypes = new[] { 
    "image/jpeg", 
    "image/jpg", 
    "image/png", 
    "image/gif", 
    "image/webp" 
};

if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
    return Results.BadRequest(new { error = "Invalid file type" });
```

### Authorization

Upload endpointy wymagają autoryzacji:

```csharp
// Logo upload - tylko dla administratorów
siteSettings.MapPost("/logo", UploadLogo)
    .DisableAntiforgery()
    .RequireAuthorization("AdminOnly");

// Post image upload - dla zalogowanych użytkowników
posts.MapPost("/image", UploadPostImage)
    .DisableAntiforgery()
    .RequireAuthorization();
```

---

## 🌐 frontend integration

### Logo Upload (Admin Panel)

```typescript
// SimpleBlog.Web/client/src/api/siteSettings.ts
export const siteSettingsApi = {
  uploadLogo: async (file: File): Promise<SiteSettings> => {
    const formData = new FormData();
    formData.append('file', file);
    
    const token = localStorage.getItem('authToken');
    const headers: Record<string, string> = {};
    
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }
    
    const response = await fetch('/api/site-settings/logo', {
      method: 'POST',
      headers,
      body: formData,
    });
    
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || 'Failed to upload logo');
    }
    
    return response.json();
  },

  deleteLogo: () => apiClient.delete<SiteSettings>('/site-settings/logo'),
};
```

### Logo Display

```typescript
// SimpleBlog.Web/client/src/components/layout/Header.tsx
import { useSiteSettings } from '@/hooks/useSiteSettings';

export function Header({ title, subtitle }: HeaderProps) {
  const { settings } = useSiteSettings();

  return (
    <div className="text-center mb-4">
      {settings?.logoUrl && (
        <div className="mb-3">
          <img
            src={settings.logoUrl}
            alt="Logo"
            style={{ 
              maxHeight: '120px', 
              maxWidth: '400px', 
              objectFit: 'contain' 
            }}
            className="img-fluid"
          />
        </div>
      )}
      {/* ... rest of header ... */}
    </div>
  );
}
```

---

## 📊 cdn i optymalizacja

### Automatyczne Optymalizacje

Cloudinary automatycznie:
- ✅ Kompresuje obrazy bez utraty jakości
- ✅ Konwertuje do nowoczesnych formatów (WebP, AVIF)
- ✅ Dostarcza przez globalny CDN
- ✅ Lazy loading i progressive loading
- ✅ Responsive images (różne rozmiary)

### Transformacje URL

Możesz modyfikować obrazy przez URL:

```
# Oryginalny URL
https://res.cloudinary.com/dy4qslml5/image/upload/simpleblog/logos/logo

# Zmiana rozmiaru (szerokość 300px)
https://res.cloudinary.com/dy4qslml5/image/upload/w_300/simpleblog/logos/logo

# Automatyczna jakość i format
https://res.cloudinary.com/dy4qslml5/image/upload/q_auto,f_auto/simpleblog/logos/logo

# Kombinacja transformacji
https://res.cloudinary.com/dy4qslml5/image/upload/w_300,h_200,c_fill,q_auto,f_auto/simpleblog/logos/logo
```

---

## 🔄 migracja i backup

### Backup Obrazów

```csharp
// Przykładowy endpoint do backupu wszystkich URL-i
public async Task<List<string>> BackupImageUrls(IPostRepository postRepo)
{
    var posts = await postRepo.GetAllAsync();
    var imageUrls = posts
        .Where(p => !string.IsNullOrEmpty(p.ImageUrl))
        .Select(p => p.ImageUrl!)
        .ToList();
    
    // Zapisz do pliku lub wysyłaj gdzie indziej
    return imageUrls;
}
```

### Bulk Delete

```csharp
public async Task<int> DeleteOrphanedImages(
    IImageStorageService imageStorage,
    IPostRepository postRepo)
{
    var activeUrls = new HashSet<string>(
        (await postRepo.GetAllAsync())
            .Where(p => !string.IsNullOrEmpty(p.ImageUrl))
            .Select(p => p.ImageUrl!)
    );
    
    // Pobierz wszystkie obrazy z Cloudinary
    var allImages = await GetAllCloudinaryImages();
    
    int deletedCount = 0;
    foreach (var image in allImages)
    {
        if (!activeUrls.Contains(image.Url))
        {
            await imageStorage.DeleteImageAsync(image.Url);
            deletedCount++;
        }
    }
    
    return deletedCount;
}
```

---

## 🎯 best practices

### ✅ Zalecane

1. **Używaj CLOUDINARY_URL** - prostsze zarządzanie credentials
2. **Waliduj pliki** - typ, rozmiar, rozszerzenie
3. **Używaj HTTPS** - `cloudinary.Api.Secure = true`
4. **Loguj operacje** - upload, delete, błędy
5. **Usuwaj stare obrazy** - przy aktualizacji/usunięciu postów
6. **Używaj CDN URLs** - automatyczna optymalizacja
7. **Testuj z małymi plikami** - w development

### ❌ Unikaj

1. **Hardkodowania credentials** - używaj zmiennych środowiskowych
2. **Uploadowania bez walidacji** - sprawdzaj typ i rozmiar
3. **Pozostawiania orphaned images** - usuwaj nieużywane
4. **Uploadowania gigantycznych plików** - ustaw limity
5. **Synchronicznego przetwarzania** - używaj async/await
6. **Ignorowania błędów** - zawsze obsługuj wyjątki

---

## 📈 monitoring i analytics

### Dashboard Cloudinary

Dostępny pod: https://console.cloudinary.com/

Monitoruj:
- 📊 Storage usage (GB)
- 📈 Bandwidth usage (transformations, deliveries)
- 🔢 Number of images
- 📉 Credits usage (free tier: 25 credits/month)

### Application Insights

```csharp
// Metryki w aplikacji
private readonly Counter<int> _uploadCounter;

public CloudinaryStorageService(IMeterFactory meterFactory, ...)
{
    var meter = meterFactory.Create("SimpleBlog.Images");
    _uploadCounter = meter.CreateCounter<int>("images.uploaded");
}

public async Task<string> UploadImageAsync(...)
{
    // ... upload logic ...
    
    _uploadCounter.Add(1, 
        new KeyValuePair<string, object?>("folder", folder),
        new KeyValuePair<string, object?>("size_kb", file.Length / 1024));
    
    return imageUrl;
}
```

---

## 🆘 troubleshooting

### Problem: "Invalid api_key"

**Przyczyna:** Nieprawidłowy lub brakujący API Key

**Rozwiązanie:**
```powershell
# Sprawdź czy zmienna jest ustawiona
$ENV:CLOUDINARY_URL

# Ustaw poprawny URL
$ENV:CLOUDINARY_URL = 'cloudinary://YOUR_KEY:YOUR_SECRET@YOUR_CLOUD'

# Zrestartuj aplikację
```

### Problem: "Request Entity Too Large"

**Przyczyna:** Plik przekracza dozwolony rozmiar

**Rozwiązanie:**
1. Sprawdź limity w kodzie (5MB dla logo, 10MB dla postów)
2. Skompresuj obraz przed uploadem
3. Zwiększ limit jeśli to konieczne

### Problem: Logo nie wyświetla się

**Przyczyna:** CORS, nieprawidłowy URL, lub brak uprawnień

**Rozwiązanie:**
1. Sprawdź URL w Network tab (Developer Tools)
2. Zweryfikuj `settings.logoUrl` w API response
3. Upewnij się że Cloudinary URL jest HTTPS
4. Sprawdź konfigurację CORS w Cloudinary Dashboard

### Problem: Stare obrazy się kumulują

**Przyczyna:** Brak czyszczenia przy aktualizacji/usunięciu

**Rozwiązanie:**
```csharp
// Dodaj cleanup przed uploadem nowego
if (currentPost?.ImageUrl is not null)
{
    await imageStorage.DeleteImageAsync(currentPost.ImageUrl, ct);
}
```

---

## 📚 dodatkowe zasoby

- [Cloudinary .NET Documentation](https://cloudinary.com/documentation/dotnet_integration)
- [Cloudinary Upload API](https://cloudinary.com/documentation/image_upload_api_reference)
- [Image Transformations](https://cloudinary.com/documentation/image_transformations)
- [URL Parameters Reference](https://cloudinary.com/documentation/transformation_reference)
- [Free Tier Limits](https://cloudinary.com/pricing)

---

## 🔄 plan rozwoju

### Przyszłe usprawnienia

1. **Lazy loading** - progressive image loading
2. **Responsive images** - `srcset` dla różnych rozdzielczości
3. **Image moderation** - automatyczne wykrywanie nieodpowiednich treści
4. **Bulk upload** - upload wielu obrazów naraz
5. **Gallery management** - zarządzanie biblioteką obrazów
6. **Video support** - rozszerzenie o upload video
