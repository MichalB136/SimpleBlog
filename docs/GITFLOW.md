# GitFlow - Strategia Branchy SimpleBlog

## Spis treści
1. [Wprowadzenie](#wprowadzenie)
2. [Główne Branchy](#główne-branchy)
3. [Wspomagające Branchy](#wspomagające-branchy)
4. [Konwencje Nazewnictwa](#konwencje-nazewnictwa)
5. [Workflow](#workflow)
6. [Przykłady](#przykłady)
7. [Best Practices](#best-practices)
8. [CI/CD Integration](#cicd-integration)

---

## Wprowadzenie

SimpleBlog stosuje **GitFlow branching strategy** - ustrukturyzowany model gałęzi git umożliwiający:
- Równoległy rozwój nowych funkcji
- Planowane wydania produkcyjne
- Szybkie hotfixy w produkcji
- Stabilną wersję produkcyjną

GitFlow bazuje na dwóch głównych gałęziach (`main` i `develop`) oraz trzech typach gałęzi wspomagających.

**Zasoby:**
- [Original GitFlow Model](https://nvie.com/posts/a-successful-git-branching-model/)
- [GitVersion - GitFlow Documentation](https://gitversion.net/docs/learn/branching-strategies/gitflow/)
- [Semantic Versioning](https://semver.org/)

---

## Główne Branchy

### 🔴 `main` (Production)

- **Przeznaczenie:** Zawiera tylko kod gotowy do produkcji
- **Źródło:** Merge z `release-*` lub `hotfix-*`
- **Wersjonowanie:** Tagi semantyczne (np. `1.0.0`, `1.0.1`, `1.1.0`)
- **Dostęp:** Protected - wymaga Pull Request z review
- **Deployment:** Automatyczne do produkcji (Render/Cloud)

**Reguły:**
- Nigdy nie commituj bezpośrednio
- Wszystkie mergepull requests do `main` muszą przejść code review
- Każdy merge tworzy nową wersję (tag)

### 🟢 `develop` (Development)

- **Przeznaczenie:** Integracyjna gałąź dla nowych funkcji
- **Źródło:** Merge z `feature/*` i `release-*` / `hotfix-*`
- **Wersjonowanie:** Pre-release (np. `1.2.0-alpha.123`)
- **Dostęp:** Protected - wymaga Pull Request
- **Deployment:** Automatyczne do środowiska staging

**Reguły:**
- Główna linia rozwojowa
- Zawsze w stanie do testowania
- Punkt wyjścia dla feature branchy
- Merging z feature branches wymagany

---

## Wspomagające Branchy

### 🔵 `feature/*` (Feature Branches)

**Konwencja:** `feature/<ticket-id>-<krótki-opis>`

**Przykłady:**
- `feature/BLOG-42-add-comments`
- `feature/SHOP-15-cart-validation`
- `feature/AUTH-8-two-factor`

**Życycl:**
```
develop → feature/BLOG-42-add-comments → develop
         (checkout)                      (merge)
```

**Procedura:**

```bash
# 1. Utwórz feature branch
git checkout develop
git pull origin develop
git checkout -b feature/BLOG-42-add-comments

# 2. Pracuj nad funkcją
git add .
git commit -m "feat: add comment system to posts

- Implement comment creation endpoint
- Add comment validation
- Create comment display UI"

# 3. Push i utwórz Pull Request
git push -u origin feature/BLOG-42-add-comments

# 4. Po zatwierdzeniu PR - merge do develop
# (wykonywane przez CI/CD lub ręcznie)
git checkout develop
git pull origin develop
git merge --no-ff feature/BLOG-42-add-comments
git push origin develop

# 5. Usuń feature branch
git push origin --delete feature/BLOG-42-add-comments
git branch -d feature/BLOG-42-add-comments
```

### 🟡 `release-*` (Release Branches)

**Konwencja:** `release-<major>.<minor>` (np. `release-1.2`)

**Przeznaczenie:**
- Przygotowanie nowej wersji produkcyjnej
- Testy przed wydaniem
- Ostateczne poprawki
- Bugfixy specificzne dla wydania

**Źródło:** `develop`

**Cel:** `main` i z powrotem do `develop`

**Procedura:**

```bash
# 1. Utwórz release branch z develop
git checkout develop
git pull origin develop
git checkout -b release-1.2

# 2. Aktualizuj version numbers (jeśli użyjesz.)
# W SimpleBlog: zaktualizuj version w project files
# .csproj, package.json, docs itp.

# 3. Commit version bump
git commit -am "chore: bump version to 1.2.0"

# 4. Tag inicial (alpha)
git tag 1.2.0-alpha.1
git push origin release-1.2 --tags

# 5. Testing phase
# - Build i test na CI/CD
# - Bugfixy: commit do release branch
# - Tag nowe RC versions: 1.2.0-alpha.2, itd.

# 6. Po zatwierdzeniu release
git tag -a 1.2.0 -m "Release version 1.2.0"
git push origin --tags

# 7. Merge do main
git checkout main
git pull origin main
git merge --no-ff release-1.2
git push origin main

# 8. Merge z powrotem do develop
git checkout develop
git pull origin develop
git merge --no-ff release-1.2
git push origin develop

# 9. Usuń release branch
git push origin --delete release-1.2
git branch -d release-1.2
```

### 🔴 `hotfix-*` (Hotfix Branches)

**Konwencja:** `hotfix-<major>.<minor>.<patch>` (np. `hotfix-1.2.1`)

**Przeznaczenie:**
- Szybkie naprawy błędów w produkcji
- Natychmiast merguje do `main` i `develop`
- Nie czeka na release cycle

**Źródło:** `main`

**Cel:** `main` i `develop`

**Procedura:**

```bash
# 1. Utwórz hotfix z main
git checkout main
git pull origin main
git checkout -b hotfix-1.2.1

# 2. Napraw bug
git add .
git commit -m "fix: critical bug in comment validation

- Validate empty comments
- Prevent injection attack
- Add test case"

# 3. Aktualizuj patch version
# .csproj, CHANGELOG.md itp.
git commit -am "chore: bump version to 1.2.1"

# 4. Tag i push
git tag -a 1.2.1 -m "Hotfix for critical validation bug"
git push origin hotfix-1.2.1 --tags

# 5. Merge do main
git checkout main
git pull origin main
git merge --no-ff hotfix-1.2.1
git push origin main

# 6. Merge do develop (aby była tam też)
git checkout develop
git pull origin develop
git merge --no-ff hotfix-1.2.1
git push origin develop

# 7. Usuń hotfix branch
git push origin --delete hotfix-1.2.1
git branch -d hotfix-1.2.1
```

### 🟣 `bugfix/*` (Bugfix Branches - opcjonalnie)

**Konwencja:** `bugfix/<ticket-id>-<opis>`

**Przykłady:**
- `bugfix/SHOP-89-cart-calculation`
- `bugfix/API-12-auth-timeout`

**Gdy używać:**
- Bugfixy do nowych funkcji w develop (nie w produkcji)
- Traktować jak feature branches
- Merge do develop (nie main)

---

## Konwencje Nazewnictwa

### Commit Messages

Stosuj [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat:` - Nowa funkcja
- `fix:` - Naprava błędu
- `chore:` - Aktualizacja dependencies, version itp.
- `docs:` - Dokumentacja
- `style:` - Formatowanie kodu
- `refactor:` - Zmiana struktury bez nowych funkcji
- `perf:` - Poprawa wydajności
- `test:` - Dodawanie/zmiana testów
- `ci:` - CI/CD konfiguracja

**Przykłady:**

```bash
feat(blog): add comment system to posts

- Implement POST /api/posts/{id}/comments endpoint
- Add comment validation and sanitization
- Display comments in post detail view

Closes #42
```

```bash
fix(shop): correct cart total calculation

Previously applied discount twice for bulk orders.

Closes #89
Refs: SHOP-89
```

### Branch Names

- Małe litery
- Hyphen separator: `-`
- Ticket ID jeśli dostępny
- Opisowy suffix

**Niepoprawne:**
- `feature/add-comments` ❌ (brak ID)
- `Feature/BLOG-42-Add-Comments` ❌ (wielkie litery)
- `feature_BLOG_42_add_comments` ❌ (underscore)

**Poprawne:**
- `feature/BLOG-42-add-comments` ✅
- `bugfix/SHOP-89-discount` ✅
- `hotfix-1.2.1` ✅

### Tag Names

Semantyczne Versioning (SemVer): `MAJOR.MINOR.PATCH[-PRERELEASE]`

```
1.0.0              # First release
1.0.1              # Patch (bugfix)
1.1.0              # Minor (new feature)
2.0.0              # Major (breaking change)

1.2.0-alpha.1      # Alpha pre-release
1.2.0-rc.1         # Release Candidate
```

---

## Workflow

### Typowy Workflow Rozwojowy

```
┌─────────────────────────────────────────────────────┐
│                   main (production)                 │
│                    v1.2.0 (tag)                     │
└─────────────────────────────────────────────────────┘
                          ▲
                          │ merge & tag
                          │
┌─────────────────────────────────────────────────────┐
│                   develop (staging)                 │
│         v1.3.0-alpha.456 (pre-release)             │
└─────────────────────────────────────────────────────┘
     ▲          ▲                        ▲
     │ merge    │ merge                  │ merge
     │          │                        │
┌────┴──┐  ┌───┴──────┐          ┌──────┴────┐
│feature│  │feature   │          │release-   │
│BLOG-42│  │SHOP-15   │          │1.3        │
└───────┘  └──────────┘          └───────────┘
```

### Cykl Release

```
1. Feature Development
   develop → feature/BLOG-42-... → PR → merge to develop

2. Release Preparation
   develop → release-1.3 → testing → bug fixes

3. Release
   release-1.3 → tag 1.3.0 → merge to main & develop

4. Production Monitoring
   main (v1.3.0 in production)

5. Hotfix (if needed)
   main → hotfix-1.3.1 → tag 1.3.1 → merge to main & develop
```

---

## Przykłady

### Przykład 1: Dodanie nowej funkcji (Feature)

**Scenario:** Dodaj system komentarzy do postów bloga

```bash
# 1. Przygotowanie
git clone https://github.com/yourorg/simpleblog.git
cd simpleblog
git checkout develop
git pull origin develop

# 2. Utwórz feature branch
git checkout -b feature/BLOG-42-add-comments

# 3. Realizuj feature
# ... edit code ...
# ... run tests: dotnet test ...
# ... build: dotnet build ...

# 4. Commit
git add .
git commit -m "feat(blog): add comment system

- Create Comment entity with validation
- Implement POST /api/posts/{id}/comments endpoint
- Add comment display in post detail view
- Include delete comment for authors

Closes #42"

# 5. Push do remote
git push -u origin feature/BLOG-42-add-comments

# 6. Create Pull Request na GitHub
#    - Opisz zmiany
#    - Link ticket
#    - Request review

# 7. Po zatwierdzeniu (automatycznie przez CI/CD)
#    - Merge to develop
#    - Feature branch deleted
```

### Przykład 2: Release do produkcji

**Scenario:** Wydanie v1.2.0

```bash
# 1. Przygotowanie release
git checkout develop
git pull origin develop
git checkout -b release-1.2

# 2. Aktualizacja version (if needed)
# SimpleBlog.Web/SimpleBlog.Web.csproj
# <Version>1.2.0</Version>

# 3. Update CHANGELOG
# docs/CHANGELOG.md
git add .
git commit -m "chore: prepare v1.2.0 release"

# 4. Tag alpha
git tag -a 1.2.0-alpha.1 -m "Alpha release for 1.2.0"
git push origin release-1.2 --tags

# 5. Testing w CI/CD
#    - Automated tests
#    - Build Docker images
#    - Deploy to staging

# 6. Hotfixy (if needed)
git commit -m "fix: correct product filter in shop

Closes #156"
git tag -a 1.2.0-alpha.2
git push origin --tags

# 7. Final tag
git tag -a 1.2.0 -m "Release version 1.2.0

Features:
- Comment system (#42)
- Product filters (#89)
- Performance improvements (#123)"

git push origin --tags

# 8. Merge to main
git checkout main
git pull origin main
git merge --no-ff release-1.2 -m "Merge release 1.2.0 to main"
git push origin main

# 9. Merge back to develop
git checkout develop
git pull origin develop
git merge --no-ff release-1.2 -m "Merge release 1.2.0 back to develop"
git push origin develop

# 10. Cleanup
git push origin --delete release-1.2
git branch -d release-1.2
```

### Przykład 3: Hotfix produkcji

**Scenario:** Krytyczna usterka w auth - v1.2.1

```bash
# 1. Utwórz hotfix z main
git checkout main
git pull origin main
git checkout -b hotfix-1.2.1

# 2. Napraw bug
# SimpleBlog.ApiService/Program.cs
# - Fix JWT validation timeout
git add .
git commit -m "fix(auth): extend JWT token timeout

Prevents legitimate users from being logged out.
Previously 1 hour, now 24 hours.

Closes #198"

# 3. Tag
git tag -a 1.2.1 -m "Hotfix: JWT timeout issue"
git push origin hotfix-1.2.1 --tags

# 4. Merge to main
git checkout main
git merge --no-ff hotfix-1.2.1 -m "Hotfix 1.2.1: JWT timeout"
git push origin main

# 5. Merge to develop (important!)
git checkout develop
git merge --no-ff hotfix-1.2.1 -m "Hotfix 1.2.1: JWT timeout"
git push origin develop

# 6. Cleanup
git push origin --delete hotfix-1.2.1
git branch -d hotfix-1.2.1
```

---

## Best Practices

### ✅ DO

1. **Zawsze pull przed pracą**
   ```bash
   git checkout develop
   git pull origin develop
   ```

2. **Utwórz feature branch z develop**
   ```bash
   git checkout -b feature/TICKET-ID-description
   ```

3. **Commit często z dobrymi wiadomościami**
   ```bash
   git commit -m "feat: add validation to order form"
   ```

4. **Push regularnie**
   ```bash
   git push origin feature/TICKET-ID-description
   ```

5. **Utwórz Pull Request do review**
   - Opisz zmiany
   - Link issue/ticket
   - Request 1-2 reviewerów

6. **Testuj przed merge**
   ```bash
   dotnet build
   dotnet test
   ```

7. **Używaj merge commits (nie squash)**
   ```bash
   git merge --no-ff feature/...
   ```

### ❌ DON'T

1. **Nie commituj bezpośrednio do main** ❌
   ```bash
   # Zakazane:
   git checkout main && git commit ...
   ```

2. **Nie commituj bezpośrednio do develop** (oprócz small hotfixy) ❌
   ```bash
   # Zakazane:
   git checkout develop && git commit ...
   ```

3. **Nie mieszaj wielu funkcji w jednym PR** ❌
   ```bash
   # Zakazane:
   feature/BLOG-42-SHOP-15-AUTH-8-multiple-features
   ```

4. **Nie pisz niejasnych commit messages** ❌
   ```bash
   # Zakazane:
   git commit -m "fix stuff"
   git commit -m "wip"
   git commit -m "asdf"
   ```

5. **Nie force-push do shared branches** ❌
   ```bash
   # Zakazane:
   git push -f origin develop
   ```

6. **Nie ignoruj code review comments** ❌
   - Zaadresuj wszystkie uwagi
   - Commituj poprawy do tego samego PR
   - Nie usuwaj branch aż do merge

7. **Nie usuwaj release/hotfix branches przed merge** ❌

---

## CI/CD Integration

### Automated Workflows

```yaml
# .github/workflows/git-flow.yml
name: GitFlow Management

on:
  pull_request:
    branches: [develop, main]
  push:
    branches: [develop, main]
    tags: ['*']

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Build
        run: dotnet build SimpleBlog.sln
      
      - name: Test
        run: dotnet test SimpleBlog.sln --no-build
      
      - name: Publish Release
        if: startsWith(github.ref, 'refs/tags/')
        run: |
          dotnet publish -c Release -o ./publish
          # Deploy to Render/Production
```

### Branch Protection Rules

**main:**
- ✅ Require pull request reviews (≥1)
- ✅ Require status checks before merging
- ✅ Require branches up to date
- ✅ Restrict deletions
- ✅ Require signed commits (optional)

**develop:**
- ✅ Require pull request reviews (≥1)
- ✅ Require status checks
- ✅ Restrict deletions

**Inne branchy:**
- ℹ️ No restrictions (developers manage themselves)

### Version Tagging in CI/CD

```bash
# Auto-tag na releases
git tag -a v1.2.0 -m "Release version 1.2.0"
git push origin v1.2.0

# Trigger deployment
# - Docker build & push
# - Deploy to Render
# - Run smoke tests
```

---

## Przydatne Komendy

```bash
# List all branches
git branch -a

# List remote branches
git branch -r

# List tags
git tag -l

# Show branch graph
git log --graph --oneline --all

# Delete local branch
git branch -d feature-name

# Delete remote branch
git push origin --delete feature-name

# Rename branch locally and push
git branch -m feature-old feature-new
git push origin -u feature-new

# Rebase feature onto develop (interactive)
git checkout feature/BLOG-42
git rebase -i develop

# Abort rebase if conflicts
git rebase --abort

# Continue after resolving conflicts
git rebase --continue
```

---

## Kontakt i Pytania

Jeśli masz pytania dotyczące GitFlow w projekcie SimpleBlog:

1. Przeczytaj ten dokument
2. Sprawdź [GitFlow Examples](https://gitversion.net/docs/learn/branching-strategies/gitflow/examples)
3. Zapytaj w team chat (#development)
4. Otwórz issue z tagiem `documentation`

---

**Ostatnia aktualizacja:** 2026-01-04  
**Wersja:** 1.0  
**Autor:** SimpleBlog Team  
**GitFlow Spec:** [Vincent Driessen's Git Flow Model](https://nvie.com/posts/a-successful-git-branching-model/)
