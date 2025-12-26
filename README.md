# SimpleBlog

Aplikacja blogowa zbudowana w oparciu o .NET Aspire.

## Wymagania

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [.NET Aspire workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling)

## Instalacja .NET Aspire

Jeśli nie masz zainstalowanego .NET Aspire workload, uruchom:

```bash
dotnet workload update
dotnet workload install aspire
```

## Uruchomienie aplikacji

### Opcja 1: Przez Visual Studio

1. Otwórz plik `SimpleBlog.sln` w Visual Studio 2022
2. Ustaw projekt `SimpleBlog.AppHost` jako projekt startowy
3. Naciśnij F5 lub kliknij "Start"

### Opcja 2: Przez wiersz poleceń

W katalogu głównym projektu wykonaj:

```bash
dotnet run --project SimpleBlog.AppHost
```

### Opcja 3: Przez PowerShell (w bieżącym katalogu)

```powershell
cd SimpleBlog.AppHost
dotnet run
```

## Po uruchomieniu

Po uruchomieniu aplikacji otworzy się Aspire Dashboard, który pokazuje:
- Status wszystkich uruchomionych serwisów
- Logi aplikacji
- Metryki wydajności
- Distributed tracing

Dashboard będzie dostępny pod adresem wyświetlonym w konsoli (zazwyczaj `http://localhost:15xxx`).

Aplikacja składa się z:
- **SimpleBlog.Web** - Aplikacja Blazor (frontend)
- **SimpleBlog.ApiService** - API REST (backend)

## Struktura projektu

```
SimpleBlog/
├── SimpleBlog.AppHost/          # Orchestrator Aspire
├── SimpleBlog.ApiService/        # API REST
├── SimpleBlog.Web/              # Aplikacja Blazor Web
└── SimpleBlog.ServiceDefaults/   # Wspólne konfiguracje
```

## Technologie

- .NET 9.0
- .NET Aspire
- Blazor
- Entity Framework Core
- ASP.NET Core Web API

## Rozwiązywanie problemów

### Błąd: "Aspire workload not installed"

Zainstaluj workload Aspire:
```bash
dotnet workload install aspire
```

### Błąd: "Port already in use"

Sprawdź czy inny proces nie używa portów aplikacji. Możesz zmienić porty w pliku `Properties/launchSettings.json` w odpowiednich projektach.
