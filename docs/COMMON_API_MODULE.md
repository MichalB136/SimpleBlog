# SimpleBlog.Common.Api

Wspólny moduł dla aplikacji API. Zawiera konfigurację endpointów i autoryzacji z obsługą hierarchii konfiguracji.

## Struktura

```
SimpleBlog.Common.Api/
├── Configuration/
│   └── ApiConfiguration.cs      # Strongly-typed config classes
├── Extensions/
│   └── ConfigurationExtensions.cs  # Extension methods dla IConfigurationBuilder
└── GlobalUsings.cs              # Global namespace imports
```

## Konfiguracja - Hierarchia

Konfiguracja jest ładowana w tej kolejności (od najniższego do najwyższego priorytetu):

### 1. `appsettings.shared.json` (Base)
Obligatoryjny plik z podstawową konfiguracją endpointów.

**Lokalizacja:** Root projektu

**Zawartość:**
```json
{
  "Endpoints": {
    "Login": "/login",
    "Posts": { "Base": "/posts", ... },
    "Products": { "Base": "/products", ... },
    "Orders": { "Base": "/orders", ... }
  },
  "Authorization": {
    "RequireAdminForPostCreate": true,
    "TokenExpirationHours": 8
  }
}
```

### 2. `appsettings.shared.{Environment}.json` (Environment Override)
Opcjonalny plik z overrideami dla konkretnego środowiska.

**Lokalizacja:** Root projektu

**Przykłady:**
- `appsettings.shared.Development.json` (dev overrides)
- `appsettings.shared.Production.json` (production overrides)
- `appsettings.shared.Staging.json` (staging overrides)

**Zawartość - Development:**
```json
{
  "Authorization": {
    "TokenExpirationHours": 8
  }
}
```

**Zawartość - Production:**
```json
{
  "Authorization": {
    "RequireAdminForOrderView": true,
    "TokenExpirationHours": 12
  }
}
```

### 3. Environment Variables (Highest Priority)
Zmienne środowiskowe z prefiksem `SimpleBlog_`

**Format:** `SimpleBlog_{sekcja}__{klucz}`

**Przykłady:**
```bash
# Override token expiration
SimpleBlog_Authorization__TokenExpirationHours=24

# Override endpoint path
SimpleBlog_Endpoints__Posts__Base=/blog

# Override admin requirement
SimpleBlog_Authorization__RequireAdminForPostCreate=false
```

## API Konfiguracyjne

### ConfigurationExtensions

#### `AddSharedConfiguration(IConfigurationBuilder, string environment, string? basePath = null)`

Dodaje konfigurację ze wspólnej hierarchii.

**Parametry:**
- `configBuilder` - Configuration builder
- `environment` - Nazwa środowiska (Development, Production, etc.)
- `basePath` - Opcjonalna ścieżka bazowa (domyślnie: bieżący katalog)

**Przykład:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Ładuje:
// 1. appsettings.shared.json
// 2. appsettings.shared.{Environment}.json
// 3. Environment variables (SimpleBlog_*)
builder.Configuration.AddSharedConfiguration(
    builder.Environment.EnvironmentName);
```

#### `LoadApiConfigurations(IConfiguration)`

Ładuje i zwraca configuration obiekty.

**Zwraca:** Tuple `(EndpointConfiguration, AuthorizationConfiguration)`

**Przykład:**
```csharp
var config = builder.Configuration;
var (endpoints, auth) = config.LoadApiConfigurations();

// Manualny zapis do DI
app.Services.AddSingleton(endpoints);
app.Services.AddSingleton(auth);
```

#### `AddApiConfigurations(IServiceCollection, IConfiguration)`

Automatycznie ładuje i rejestruje konfiguracje w DI container.

**Parametry:**
- `services` - Service collection
- `configuration` - Configuration source

**Przykład:**
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddSharedConfiguration(builder.Environment.EnvironmentName);

// Automatycznie ładuje i rejestruje
builder.Services.AddApiConfigurations(builder.Configuration);
```

## Klasy Konfiguracji

### `EndpointConfiguration`

Ścieżki wszystkich endpointów API.

**Właściwości:**
- `Login` - Ścieżka do endpointu logowania
- `Posts` - Konfiguracja endpointów postów (PostsEndpoints)
- `Products` - Konfiguracja endpointów produktów (ProductsEndpoints)
- `Orders` - Konfiguracja endpointów zamówień (OrdersEndpoints)
- `Health` - Ścieżka health check'u
- `OpenApi` - Ścieżka OpenAPI schematu

### `AuthorizationConfiguration`

Ustawienia autoryzacji i bezpieczeństwa.

**Właściwości:**
- `RequireAdminForPostCreate` - Wymaga Admin do tworzenia postów
- `RequireAdminForPostDelete` - Wymaga Admin do usuwania postów
- `RequireAdminForProductCreate` - Wymaga Admin do tworzenia produktów
- `RequireAdminForProductUpdate` - Wymaga Admin do edycji produktów
- `RequireAdminForProductDelete` - Wymaga Admin do usuwania produktów
- `RequireAdminForOrderView` - Wymaga Admin do przeglądania zamówień
- `TokenExpirationHours` - Czas wygaśnięcia JWT tokenu (w godzinach)

## Użycie w ApiService

### Setup

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Ładowanie konfiguracji z hierarchii
builder.Configuration.AddSharedConfiguration(
    builder.Environment.EnvironmentName);

// Dodawanie do DI container
builder.Services.AddApiConfigurations(builder.Configuration);
```

### Pobranie w Endpointach

```csharp
// Po build'zie aplikacji
var app = builder.Build();

// Pobierz z DI container
var endpointConfig = app.Services.GetRequiredService<EndpointConfiguration>();
var authConfig = app.Services.GetRequiredService<AuthorizationConfiguration>();

// Używaj w endpointach
app.MapPost(endpointConfig.Login, (LoginRequest request, ...) => {
    var expiration = DateTime.UtcNow.AddHours(authConfig.TokenExpirationHours);
    // ...
});

var posts = app.MapGroup(endpointConfig.Posts.Base);
posts.MapGet(endpointConfig.Posts.GetAll, ...);
posts.MapPost(endpointConfig.Posts.Create, (...) => {
    if (authConfig.RequireAdminForPostCreate && !context.User.IsInRole("Admin")) {
        return Results.Forbid();
    }
    // ...
});
```

## Scenariusze

### Zmiana ścieżki Endpointu

Edytuj `appsettings.shared.json`:

```json
{
  "Endpoints": {
    "Posts": {
      "Base": "/blog"  // Zmienione z "/posts"
    }
  }
}
```

Aplikacja automatycznie będzie używać `/blog`.

### Environment-Specific Konfiguracja

Stwórz `appsettings.shared.Production.json`:

```json
{
  "Authorization": {
    "RequireAdminForPostCreate": true,
    "RequireAdminForPostDelete": true,
    "TokenExpirationHours": 12
  }
}
```

W produkcji będą używane te ustawienia zamiast default'ów.

### Runtime Override z Env Variables

```bash
# Wyłącz wymaganie Admin dla tworzenia postów
export SimpleBlog_Authorization__RequireAdminForPostCreate=false

# Zmień czas wygaśnięcia tokenu
export SimpleBlog_Authorization__TokenExpirationHours=24

# Zmień endpoint path
export SimpleBlog_Endpoints__Products__Base=/shop
```

## Rozszerzanie

Aby dodać nowe konfiguracje:

1. **Dodaj klasy w `Configuration/ApiConfiguration.cs`:**
```csharp
public class EmailConfiguration
{
    public string SmtpServer { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 587;
}
```

2. **Zaktualizuj `ConfigurationExtensions.cs`:**
```csharp
public static (
    EndpointConfiguration Endpoints,
    AuthorizationConfiguration Authorization,
    EmailConfiguration Email) LoadApiConfigurations(
    this IConfiguration configuration)
{
    var email = new EmailConfiguration();
    configuration.GetSection("Email").Bind(email);
    
    return (endpoints, auth, email);
}
```

3. **Dodaj do `appsettings.shared.json`:**
```json
{
  "Email": {
    "SmtpServer": "smtp.example.com",
    "SmtpPort": 587
  }
}
```

## Zależności

- `Microsoft.Extensions.Configuration` 9.0.0
- `Microsoft.Extensions.Configuration.Json` 9.0.0
- `Microsoft.Extensions.Configuration.EnvironmentVariables` 9.0.0
- `Microsoft.Extensions.Configuration.Binder` 9.0.0
- `Microsoft.Extensions.DependencyInjection` 9.0.0
- `SimpleBlog.Common`

## Powiązane Dokumenty

- [ENDPOINT_CONFIGURATION.md](../ENDPOINT_CONFIGURATION.md) - Szczegółowy przewodnik endpointów
- [GITFLOW.md](../GITFLOW.md) - GitFlow branching strategy
