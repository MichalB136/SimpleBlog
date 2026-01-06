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
- **PostgreSQL** - Baza danych (kontener Docker via docker-compose)

## Baza danych

### Wymagania

Projekt wymaga **PostgreSQL** uruchomionego przez **docker-compose** przed startem aplikacji.

**WAŻNE: Musisz ręcznie uruchomić PostgreSQL przed aplikacją!**

```powershell
# Uruchom PostgreSQL i pgAdmin
docker-compose up -d

# Sprawdź status
docker-compose ps
```

### Problem: Brak dostępu do Docker Hub

Jeśli otrzymasz błąd "failed to authorize" przy `docker-compose up -d`:

**Tymczasowe rozwiązanie - używaj SQLite:**
```powershell
# Uruchom z SQLite (bez Docker)
.\start.ps1 -UseSqlite
```

Potem gdy będziesz mieć dostęp do Docker Hub:
```powershell
# Zaloguj się do Docker (jeśli potrzebujesz)
docker login

# Lub ustaw proxy jeśli jesteś za firewallem
# Zobacz: https://docs.docker.com/config/daemon/
```

### Pierwszy uruchomienie

Po uruchomieniu PostgreSQL, aplikacja automatycznie:
- ✅ Zastosuje wszystkie migracje Entity Framework
- ✅ Stworzy strukturę bazy danych
- ✅ Będzie gotowa do pracy

Wystarczy uruchomić aplikację:

```powershell
dotnet run --project SimpleBlog.AppHost
# lub
.\start.ps1
```

### Dostęp do bazy danych

**PostgreSQL:**
- Host: `localhost:5432`
- Database: `simpleblog`
- User: `simpleblog_user`
- Password: `simpleblog_dev_password_123`

**pgAdmin:** http://localhost:5050
- Email: `admin@simpleblog.local`
- Password: `admin`

Więcej informacji w [docs/DATABASES.md](docs/DATABASES.md)

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
- .NET Aspire 13.1.0
- Blazor
- Entity Framework Core 9.0.10
- PostgreSQL (Npgsql 9.0.4) via docker-compose
- ASP.NET Core Web API
- Docker & Docker Compose

## Rozwiązywanie problemów

### Błąd: "Aspire workload not installed"

Zainstaluj workload Aspire:
```bash
dotnet workload install aspire
```

### Błąd: "Port already in use"

Sprawdź czy inny proces nie używa portów aplikacji. Możesz zmienić porty w pliku `Properties/launchSettings.json` w odpowiednich projektach.
