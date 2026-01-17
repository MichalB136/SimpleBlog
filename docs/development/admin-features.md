# Admin Features Guide

> ## metadane dokumentu
> 
> ### ✅ wymagane
> **Tytuł:** Admin Features Guide  
> **Opis:** Przewodnik po funkcjach administracyjnych SimpleBlog  
> **Audience:** administrator, developer  
> **Topic:** development  
> **Last Update:** 2026-01-17
>
> ### 📌 rekomendowane
> **Parent Document:** [README.md](README.md)  
> **Difficulty:** intermediate  
> **Estimated Time:** 30 min  
> **Version:** 1.0.0  
> **Status:** approved
>
> ### 🏷️ opcjonalne
> **Prerequisites:** React, TypeScript, API knowledge  
> **Related Docs:** [react-router-guide.md](react-router-guide.md), [cloudinary-integration.md](../technical/cloudinary-integration.md)  
> **Tags:** `admin`, `settings`, `logo`, `theme`, `management`

---

## 📋 przegląd

Panel administracyjny SimpleBlog dostępny pod `/settings` umożliwia zarządzanie wyglądem strony (motywy, logo) oraz w przyszłości będzie rozszerzany o kolejne funkcje.

---

## 🔐 dostęp do panelu

### Wymagania

- Zalogowany użytkownik
- Rola `Admin`
- Dostęp przez ikonę ⚙️ w prawym górnym rogu lub bezpośrednio `/settings`

### Ochrona Route

```typescript
// SimpleBlog.Web/client/src/App.tsx
<Route path="/settings" element={
  user?.role === 'Admin' ? (
    <div className="container-fluid">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>
          <i className="bi bi-gear-fill me-2"></i>
          Panel Administracyjny
        </h2>
        <button
          className="btn btn-outline-secondary"
          onClick={() => navigate('/')}
        >
          <i className="bi bi-arrow-left me-2"></i>
          Powrót
        </button>
      </div>
      <AdminPanel />
    </div>
  ) : (
    <Navigate to="/" replace />
  )
} />
```

Użytkownicy bez roli Admin są automatycznie przekierowywani na stronę główną.

---

## 🎨 zarządzanie motywami

### Dostępne Motywy

SimpleBlog obsługuje 3 motywy Bootstrap:
- **default** - Standardowy biały motyw
- **cerulean** - Niebieski motyw
- **darkly** - Ciemny motyw

### Implementacja

```typescript
// SimpleBlog.Web/client/src/hooks/useSiteSettings.ts
export function useSiteSettings() {
  const [settings, setSettings] = useState<SiteSettings | null>(null);
  const [availableThemes, setAvailableThemes] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchSettings = async () => {
    try {
      setLoading(true);
      const data = await siteSettingsApi.getSettings();
      setSettings(data);
      
      const themes = await siteSettingsApi.getAvailableThemes();
      setAvailableThemes(themes);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch settings');
    } finally {
      setLoading(false);
    }
  };

  const updateTheme = async (newTheme: string) => {
    try {
      await siteSettingsApi.updateTheme(newTheme);
      await fetchSettings();
    } catch (err) {
      throw err;
    }
  };

  return {
    settings,
    availableThemes,
    loading,
    error,
    updateTheme,
    refresh: fetchSettings,
  };
}
```

### API Endpoints

```typescript
// GET /api/site-settings
{
  "theme": "default",
  "logoUrl": "https://res.cloudinary.com/...",
  "availableThemes": ["default", "cerulean", "darkly"]
}

// PUT /api/site-settings/theme
{
  "theme": "darkly"
}
```

### Backend Implementation

```csharp
// SimpleBlog.ApiService/Endpoints/SiteSettingsEndpoints.cs
private static async Task<IResult> UpdateTheme(
    [FromBody] UpdateThemeRequest request,
    ISiteSettingsRepository repository,
    CancellationToken ct)
{
    var validThemes = new[] { "default", "cerulean", "darkly" };
    if (!validThemes.Contains(request.Theme))
    {
        return Results.BadRequest(new { error = "Invalid theme" });
    }

    var settings = await repository.GetAsync(ct);
    settings = settings with { Theme = request.Theme };
    await repository.UpdateAsync(settings, ct);

    return Results.Ok(settings);
}
```

---

## 🖼️ zarządzanie logo

### Upload Logo

Funkcja pozwala na przesłanie jednego logo strony, które jest wyświetlane na górze każdej strony.

#### Ograniczenia

- **Typ pliku:** `image/jpeg`, `image/png`, `image/gif`, `image/webp`
- **Maksymalny rozmiar:** 5 MB
- **Nazwa pliku:** Zawsze `logo` (nadpisywane)
- **Lokalizacja w Cloudinary:** `{RootFolder}/logos/logo`

#### Implementacja Upload

```typescript
// SimpleBlog.Web/client/src/api/siteSettings.ts
async uploadLogo(file: File): Promise<SiteSettings> {
  const formData = new FormData();
  formData.append('file', file);

  const token = localStorage.getItem('authToken');
  
  const response = await fetch('/api/site-settings/logo', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
    },
    body: formData,
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error || 'Failed to upload logo');
  }

  return await response.json();
}
```

#### Backend Endpoint

```csharp
// POST /api/site-settings/logo
private static async Task<IResult> UploadLogo(
    IFormFile file,
    ISiteSettingsRepository repository,
    IImageStorageService imageStorage,
    ILogger<Program> logger,
    CancellationToken ct)
{
    // Validate file size (5MB)
    if (file.Length > 5 * 1024 * 1024)
    {
        return Results.BadRequest(new { error = "File too large. Maximum size is 5MB." });
    }

    // Validate content type
    var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
    if (!allowedTypes.Contains(file.ContentType))
    {
        return Results.BadRequest(new { error = "Invalid file type. Only images are allowed." });
    }

    var settings = await repository.GetAsync(ct);

    // Delete old logo if exists
    if (!string.IsNullOrEmpty(settings.LogoUrl))
    {
        await imageStorage.DeleteImageAsync(settings.LogoUrl, ct);
    }

    // Upload new logo with fixed name "logo"
    await using var stream = file.OpenReadStream();
    var logoUrl = await imageStorage.UploadImageAsync(stream, "logo", "logos", ct);

    // Update database
    await repository.UpdateLogoAsync(logoUrl, ct);

    var updatedSettings = settings with { LogoUrl = logoUrl };
    logger.LogInformation("Logo uploaded successfully: {LogoUrl}", logoUrl);

    return Results.Ok(updatedSettings);
}
```

### Delete Logo

```typescript
// SimpleBlog.Web/client/src/api/siteSettings.ts
async deleteLogo(): Promise<SiteSettings> {
  const token = localStorage.getItem('authToken');
  
  const response = await fetch('/api/site-settings/logo', {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error || 'Failed to delete logo');
  }

  return await response.json();
}
```

#### Backend Endpoint

```csharp
// DELETE /api/site-settings/logo
private static async Task<IResult> DeleteLogo(
    ISiteSettingsRepository repository,
    IImageStorageService imageStorage,
    ILogger<Program> logger,
    CancellationToken ct)
{
    var settings = await repository.GetAsync(ct);

    if (string.IsNullOrEmpty(settings.LogoUrl))
    {
        return Results.BadRequest(new { error = "No logo to delete." });
    }

    // Delete from Cloudinary
    var deleted = await imageStorage.DeleteImageAsync(settings.LogoUrl, ct);
    if (!deleted)
    {
        logger.LogWarning("Failed to delete logo from Cloudinary: {LogoUrl}", settings.LogoUrl);
    }

    // Update database
    await repository.UpdateLogoAsync(null, ct);

    var updatedSettings = settings with { LogoUrl = null };
    logger.LogInformation("Logo deleted successfully");

    return Results.Ok(updatedSettings);
}
```

---

## 🎯 admin panel component

### Structure

```typescript
// SimpleBlog.Web/client/src/components/admin/AdminPanel.tsx
export function AdminPanel() {
  const {
    settings,
    availableThemes,
    loading,
    error,
    updateTheme,
    uploadLogo,
    deleteLogo,
    refresh,
  } = useSiteSettings();

  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [uploadLoading, setUploadLoading] = useState(false);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [uploadSuccess, setUploadSuccess] = useState(false);

  // File selection handling
  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate file type
    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      setUploadError('Invalid file type. Only images (JPEG, PNG, GIF, WebP) are allowed.');
      return;
    }

    // Validate file size (5MB)
    if (file.size > 5 * 1024 * 1024) {
      setUploadError('File too large. Maximum size is 5MB.');
      return;
    }

    setSelectedFile(file);
    setUploadError(null);

    // Create preview
    const reader = new FileReader();
    reader.onloadend = () => {
      setPreviewUrl(reader.result as string);
    };
    reader.readAsDataURL(file);
  };

  // Upload handler
  const handleUpload = async () => {
    if (!selectedFile) return;

    try {
      setUploadLoading(true);
      setUploadError(null);
      await uploadLogo(selectedFile);
      setUploadSuccess(true);
      setSelectedFile(null);
      setPreviewUrl(null);

      setTimeout(() => setUploadSuccess(false), 3000);
    } catch (err) {
      setUploadError(err instanceof Error ? err.message : 'Failed to upload logo');
    } finally {
      setUploadLoading(false);
    }
  };

  // Delete handler
  const handleDelete = async () => {
    if (!window.confirm('Are you sure you want to delete the logo?')) return;

    try {
      setUploadLoading(true);
      setUploadError(null);
      await deleteLogo();
    } catch (err) {
      setUploadError(err instanceof Error ? err.message : 'Failed to delete logo');
    } finally {
      setUploadLoading(false);
    }
  };

  return (
    <div className="container mt-4">
      <h2 className="mb-4">
        <i className="bi bi-gear-fill me-2"></i>
        Panel Administracyjny
      </h2>

      {/* Theme Selection */}
      <div className="card mb-4">
        <div className="card-header">
          <h5 className="mb-0">
            <i className="bi bi-palette-fill me-2"></i>
            Motyw Strony
          </h5>
        </div>
        <div className="card-body">
          <select
            className="form-select"
            value={settings?.theme || ''}
            onChange={async (e) => {
              try {
                await updateTheme(e.target.value);
              } catch (err) {
                alert('Failed to update theme');
              }
            }}
            disabled={loading}
          >
            {availableThemes.map((theme) => (
              <option key={theme} value={theme}>
                {theme.charAt(0).toUpperCase() + theme.slice(1)}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Logo Management */}
      <div className="card">
        <div className="card-header">
          <h5 className="mb-0">
            <i className="bi bi-image-fill me-2"></i>
            Logo Strony
          </h5>
        </div>
        <div className="card-body">
          {/* Current Logo */}
          {settings?.logoUrl && (
            <div className="mb-3">
              <label className="form-label">Aktywne Logo:</label>
              <div className="border rounded p-2 text-center bg-light">
                <img
                  src={settings.logoUrl}
                  alt="Current Logo"
                  style={{ maxHeight: '120px', maxWidth: '100%', objectFit: 'contain' }}
                />
              </div>
              <button
                className="btn btn-danger btn-sm mt-2"
                onClick={handleDelete}
                disabled={uploadLoading}
              >
                <i className="bi bi-trash me-1"></i>
                Usuń Logo
              </button>
            </div>
          )}

          {/* File Upload */}
          <div className="mb-3">
            <label htmlFor="logoFile" className="form-label">
              {settings?.logoUrl ? 'Zmień Logo:' : 'Dodaj Logo:'}
            </label>
            <input
              type="file"
              className="form-control"
              id="logoFile"
              accept="image/*"
              onChange={handleFileSelect}
              disabled={uploadLoading}
            />
            <small className="form-text text-muted">
              Max 5MB, formaty: JPEG, PNG, GIF, WebP
            </small>
          </div>

          {/* Preview */}
          {previewUrl && (
            <div className="mb-3">
              <label className="form-label">Podgląd:</label>
              <div className="border rounded p-2 text-center bg-light">
                <img
                  src={previewUrl}
                  alt="Preview"
                  style={{ maxHeight: '120px', maxWidth: '100%', objectFit: 'contain' }}
                />
              </div>
            </div>
          )}

          {/* Upload Button */}
          {selectedFile && (
            <button
              className="btn btn-primary"
              onClick={handleUpload}
              disabled={uploadLoading}
            >
              {uploadLoading ? (
                <>
                  <span className="spinner-border spinner-border-sm me-2"></span>
                  Przesyłanie...
                </>
              ) : (
                <>
                  <i className="bi bi-upload me-2"></i>
                  Prześlij Logo
                </>
              )}
            </button>
          )}

          {/* Success Message */}
          {uploadSuccess && (
            <div className="alert alert-success mt-3">
              <i className="bi bi-check-circle me-2"></i>
              Logo zostało pomyślnie przesłane!
            </div>
          )}

          {/* Error Message */}
          {uploadError && (
            <div className="alert alert-danger mt-3">
              <i className="bi bi-exclamation-triangle me-2"></i>
              {uploadError}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
```

---

## 🔄 logo display

### Header Component

```typescript
// SimpleBlog.Web/client/src/components/layout/Header.tsx
import { useSiteSettings } from '@/hooks/useSiteSettings';

interface HeaderProps {
  title: string;
  subtitle?: string;
}

export function Header({ title, subtitle }: HeaderProps) {
  const { settings } = useSiteSettings();

  return (
    <header className="bg-primary text-white py-4 shadow-sm">
      <div className="container text-center">
        {/* Logo Display */}
        {settings?.logoUrl && (
          <div className="mb-3">
            <img
              src={settings.logoUrl}
              alt="Site Logo"
              style={{
                maxHeight: '120px',
                maxWidth: '400px',
                objectFit: 'contain',
                display: 'block',
                margin: '0 auto',
              }}
            />
          </div>
        )}
        
        <h1 className="display-4 fw-bold mb-2">{title}</h1>
        {subtitle && <p className="lead">{subtitle}</p>}
      </div>
    </header>
  );
}
```

Logo wyświetla się:
- **Centered** - `margin: 0 auto`
- **Max height:** 120px
- **Max width:** 400px
- **Object fit:** contain (zachowuje proporcje)
- **Conditional** - tylko gdy `settings.logoUrl` istnieje

---

## 🎯 best practices

### ✅ Zalecane

1. **Walidacja plików** - Sprawdzaj typ i rozmiar zarówno na frontend jak i backend
2. **Feedback użytkownika** - Pokaż loading states, success/error messages
3. **Preview** - Pozwól użytkownikowi zobaczyć plik przed uploadem
4. **Confirmation** - Pytaj przed usunięciem logo
5. **Error handling** - Obsługuj błędy API gracefully
6. **Accessible** - Używaj semantycznych HTML i ARIA labels
7. **Responsive** - Logo powinno dobrze wyglądać na wszystkich urządzeniach

### ❌ Unikaj

1. **Brak walidacji** - Zawsze sprawdzaj pliki przed przesłaniem
2. **Brak feedback** - Użytkownik musi wiedzieć co się dzieje
3. **Hardkodowane wartości** - Użyj konfiguracji dla limitów rozmiaru
4. **Ignorowanie błędów** - Obsłuż wszystkie możliwe scenariusze
5. **Bezpośrednie mutacje stanu** - Używaj setterów i hooks
6. **Nieodpowiednie uprawnienia** - Zawsze weryfikuj rolę Admin

---

## 🔄 przyszłe funkcje

### Planowane rozszerzenia Admin Panel

1. **User Management** - Lista użytkowników, edycja ról, ban
2. **Post Moderation** - Approve/reject comments, pin posts
3. **Analytics Dashboard** - Statystyki odwiedzin, popularne posty
4. **SEO Settings** - Meta tags, descriptions, keywords
5. **Email Templates** - Edycja szablonów emaili
6. **Site Announcements** - Banery informacyjne na stronie
7. **Backup/Restore** - Zarządzanie kopiami zapasowymi
8. **API Keys** - Zarządzanie kluczami dostępu do API

### Gallery Management (Future)

```typescript
// Potential structure for future gallery feature
interface GalleryItem {
  id: string;
  url: string;
  thumbnail: string;
  title: string;
  uploadedAt: Date;
  size: number;
  dimensions: { width: number; height: number };
}

// Bulk upload
async uploadGalleryImages(files: File[]): Promise<GalleryItem[]> {
  // Implementation
}

// Gallery grid display
<div className="gallery-grid">
  {items.map(item => (
    <GalleryCard key={item.id} item={item} />
  ))}
</div>
```

---

## 📚 dodatkowe zasoby

- [Cloudinary Integration](../technical/cloudinary-integration.md) - Szczegóły integracji Cloudinary
- [React Router Guide](./react-router-guide.md) - Routing i nawigacja
- [Authentication Flow](../technical/authentication-flow.md) - JWT authentication
- [API Specification](../technical/api-specification.md) - API endpoints

---

## 🆘 troubleshooting

### Problem: Upload nie działa (401 Unauthorized)

**Przyczyna:** Brak lub nieprawidłowy token JWT

**Rozwiązanie:**
```typescript
// Upewnij się że używasz poprawnego klucza localStorage
const token = localStorage.getItem('authToken'); // Nie 'token'!
```

### Problem: Plik za duży

**Przyczyna:** Przekroczono limit 5MB

**Rozwiązanie:**
```typescript
// Zmniejsz rozmiar pliku lub zwiększ limit na backendzie
if (file.size > 5 * 1024 * 1024) {
  alert('File too large. Please use an image under 5MB.');
  return;
}
```

### Problem: Logo nie wyświetla się

**Przyczyna:** Nieprawidłowy URL lub brak dostępu

**Rozwiązanie:**
```typescript
// Sprawdź console i network tab
console.log('Logo URL:', settings?.logoUrl);

// Upewnij się że Cloudinary jest skonfigurowany
// Sprawdź CLOUDINARY_URL w environment variables
```

### Problem: Route /settings nie działa

**Przyczyna:** Brak React Router lub nieprawidłowa konfiguracja

**Rozwiązanie:**
```typescript
// Upewnij się że App.tsx używa BrowserRouter
import { BrowserRouter } from 'react-router-dom';

function App() {
  return (
    <BrowserRouter>
      <AppContent />
    </BrowserRouter>
  );
}
```
