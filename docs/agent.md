# Logging w .NET – instrukcje dla agenta

> ## Document Metadata
> 
> ### ✅ Required
> **Title:** Logging w .NET – instrukcje dla agenta
> **Description:** Zwięzłe i praktyczne podsumowanie oficjalnych zasad logowania w .NET (Microsoft Learn). Dokument jest napisany tak, aby agent automatycznie stosował właściwe wzorce: DI, konfigurację, kategorie, filtry, message templates, scope i wydajność.
> **Audience:** contributor
> **Topic:** technical
> **Last Update:** 2026-02-01
>
> ### 📌 Recommended
> **Parent Document:** [README.md](./README.md)
> **Difficulty:** beginner
> **Estimated Time:** 10 min
> **Version:** 1.0.0
> **Status:** draft
>
> ### 🏷️ Optional
> **Tags:** `logging`, `dotnet`, `best-practices`, `agent`

---

## 🎯 Cel
Ten dokument ma ułatwić agentowi konsekwentne stosowanie oficjalnych zasad logowania w .NET. Priorytetem jest czytelność, możliwość filtrowania oraz niskie koszty utrzymania logów.

---

## ✅ Najważniejsze zasady (do automatycznego stosowania)
- Zawsze korzystaj z `ILogger<T>` z DI (nie twórz loggerów ręcznie w kodzie aplikacji).
- Loguj **strukturalnie** przez message templates, bez interpolacji stringów.
- Włączaj logi i poziomy przez **konfigurację** (appsettings / env vars), a nie na stałe w kodzie.
- Stosuj **kategorie** oparte o pełną nazwę typu (domyślnie zapewnia to `ILogger<T>`).
- Dodawaj **EventId**, gdy logi mają być później grupowane lub agregowane.
- Używaj **scope** do korelacji (np. `CorrelationId`, `TransactionId`).
- Dla hot‑pathów używaj **source generatora** (`LoggerMessage`).

---

## 📌 Konfiguracja (reguły)
- Konfiguracja logów powinna być trzymana w `appsettings.{Environment}.json`.
- Filtry ustawiaj **per kategoria** oraz **per provider**.
- Preferuj zmiany poziomów przez konfigurację zewnętrzną (np. zmienne środowiskowe), aby nie wymagać przebudowy aplikacji.

---

## 🧩 Kluczowe pojęcia

### 1) Kategorie
- `ILogger<T>` tworzy kategorię na podstawie pełnej nazwy typu.
- Dla dodatkowego grupowania można użyć `ILoggerFactory.CreateLogger("Namespace.Component.Subcategory")`.

### 2) Poziomy logów
- `Trace` / `Debug` – tylko do analizy, zwykle wyłączone w prod.
- `Information` – normalny przepływ i ważne zdarzenia biznesowe.
- `Warning` – nietypowe sytuacje, które nie przerywają działania.
- `Error` – błąd obsłużony (z wyjątkiem).
- `Critical` – awaria systemu.

### 3) Message templates
- Zawsze używaj placeholderów `{Name}` i przekazuj wartości jako argumenty.
- Nie używaj interpolacji: to utrudnia filtrowanie i jest wolniejsze.

### 4) EventId
- Stosuj, gdy zdarzenia mają mieć stałe identyfikatory (np. CRUD, integracje, rejestry zdarzeń).

### 5) Scope
- Zakładaj scope przy obsłudze żądań i ważnych transakcji.
- Scope powinien zawierać kluczowe identyfikatory (np. `CorrelationId`).

---

## ⚙️ Wydajność
- Dla krytycznych ścieżek używaj `LoggerMessage` (source generator) zamiast `LogInformation`.
- Logowanie powinno być synchroniczne; jeśli docelowy storage jest wolny, logi należy buforować/asynchronicznie eksportować poza krytyczną ścieżką.

---

## ✅ Checklist dla agenta
- [ ] Używam `ILogger<T>` z DI
- [ ] Message templates zamiast interpolacji
- [ ] Poziomy logów mają sens (bez nadmiaru `Information`)
- [ ] Scope z `CorrelationId` w ścieżkach żądań
- [ ] `EventId` tam, gdzie to pomaga w analizie
- [ ] Wysoka wydajność: `LoggerMessage` na hot‑pathach
- [ ] Konfiguracja poziomów w appsettings/env vars

---

## 🚫 Czego unikać
- Nie loguj sekretów, tokenów ani danych wrażliwych.
- Nie mieszaj logów z metrykami (log ≠ metryka).
- Nie loguj wszystkiego na `Information`.
- Nie twórz loggerów bez DI.
