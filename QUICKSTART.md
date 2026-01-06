# 🚀 Quick Start Guide - SimpleBlog

## Wymagania wstępne

1. **Docker Desktop** - [Pobierz tutaj](https://www.docker.com/products/docker-desktop/)
2. **.NET 9.0 SDK** - [Pobierz tutaj](https://dotnet.microsoft.com/download/dotnet/9.0)
3. **.NET Aspire workload**

```powershell
dotnet workload update
dotnet workload install aspire
```

---

## Uruchomienie aplikacji

### Krok 1: Uruchom bazę danych (PostgreSQL)

```powershell
# Uruchom PostgreSQL i pgAdmin
docker-compose up -d

# Sprawdź status
docker-compose ps
```

### Krok 2: Uruchom aplikację

Aplikacja automatycznie:
- ✅ Połączy się z bazą PostgreSQL
- ✅ Zastosuje wszystkie migracje
- ✅ Ustawi strukturę bazy danych

```powershell
# Opcja 1: Skrypt
.\start.ps1

# Opcja 2: Ręcznie
dotnet run --project SimpleBlog.AppHost
```

### Krok 3: Dostęp do aplikacji

- **Aplikacja web:** Sprawdź URL w Aspire Dashboard
- **Aspire Dashboard:** URL wyświetlony w konsoli
- **pgAdmin:** http://localhost:5050
  - Email: `admin@simpleblog.local`
  - Password: `admin`
- **PostgreSQL:** localhost:5432
  - Database: `simpleblog`
  - User: `simpleblog_user`
  - Password: `simpleblog_dev_password_123`

---

## Zatrzymanie aplikacji

```powershell
# Zatrzymaj aplikację: Ctrl+C w terminalu

# Zatrzymaj PostgreSQL
docker-compose stop

# Lub użyj skryptu
.\stop.ps1
```

---

## Przydatne komendy

```powershell
# Czyste buildy
.\start.ps1 -Clean

# Status Dockera
docker-compose ps

# Logi bazy danych
docker-compose logs -f postgres

# Restart bazy
docker-compose restart postgres
```

---

## Dostęp do bazy danych

### SQLite (domyślnie)
- **Plik:** `SimpleBlog.ApiService/simpleblog.db`
- **Narzędzia:** DB Browser for SQLite, Azure Data Studio

### PostgreSQL (Docker)
- **Host:** localhost:5432
- **Database:** simpleblog
- **User:** simpleblog_user
- **Password:** simpleblog_dev_password_123

**pgAdmin Web UI:**
- URL: http://localhost:5050
- Login: admin@simpleblog.local / admin

---

## Seed Data (testowe dane)

Po pierwszym uruchomieniu, aplikacja tworzy:

**Użytkownicy:**
- Admin: `admin` / `admin123`
- User: `user` / `user123`

**Posty:** 3 przykładowe wpisy blogowe
**Produkty:** 3 przykładowe produkty w sklepie

---

## Rozwiązywanie problemów

### "Port already in use"
```powershell
# Sprawdź co używa portu
netstat -ano | findstr :5433

# Zabij proces
Stop-Process -Id <PID> -Force
```

### Docker nie działa
```powershell
# Sprawdź status
docker info

# Restart Docker Desktop
```

### Baza się nie tworzy
```powershell
# Usuń stare dane i zacznij od nowa
docker-compose down -v
.\start.ps1 -Database postgres -Clean
```

---

## Dokumentacja

- [README.md](README.md) - Główna dokumentacja
- [docker/README.md](docker/README.md) - Docker & PostgreSQL
- [docs/DATABASES.md](docs/DATABASES.md) - Architektura bazy danych
- [docs/GITFLOW.md](docs/GITFLOW.md) - Workflow developmentu
