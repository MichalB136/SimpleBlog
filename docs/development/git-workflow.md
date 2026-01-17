# Git Workflow Guide

> ## Document Metadata
> 
> ### ✅ Required
> **Title:** Git Workflow Guide - GitFlow Strategy for SimpleBlog  
> **Description:** Complete guide to GitFlow branching strategy including feature development, releases, hotfixes, and CI/CD integration  
> **Audience:** developer  
> **Topic:** development  
> **Last Update:** 2026-01-17
>
> ### 📌 Recommended
> **Parent Document:** [README.md](./README.md)  
> **Difficulty:** intermediate  
> **Estimated Time:** 30 min  
> **Version:** 1.0.0  
> **Status:** approved
>
> ### 🏷️ Optional
> **Prerequisites:** Git installed, Understanding of branching concepts, Familiarity with Pull Requests  
> **Related Docs:** [getting-started.md](./getting-started.md), [../deployment/render-guide.md](../deployment/render-guide.md)  
> **Tags:** `git`, `gitflow`, `branching`, `workflow`, `ci-cd`, `version-control`

---

## 📋 Table of Contents

1. [Introduction](#-introduction)
2. [Main Branches](#-main-branches)
3. [Supporting Branches](#-supporting-branches)
4. [Naming Conventions](#-naming-conventions)
5. [Workflow Diagrams](#-workflow-diagrams)
6. [Practical Examples](#-practical-examples)
7. [Best Practices](#-best-practices)
8. [CI/CD Integration](#-cicd-integration)
9. [Useful Commands](#-useful-commands)

---

## 🎯 Introduction

SimpleBlog uses **GitFlow branching strategy** - a structured Git branching model that enables:

- ✅ Parallel development of new features
- ✅ Planned production releases
- ✅ Fast hotfixes in production
- ✅ Stable production version

GitFlow is based on **two main branches** (`main` and `develop`) and **three types of supporting branches**.

### Key Resources

- [Original GitFlow Model](https://nvie.com/posts/a-successful-git-branching-model/) by Vincent Driessen
- [GitVersion - GitFlow Documentation](https://gitversion.net/docs/learn/branching-strategies/gitflow/)
- [Semantic Versioning](https://semver.org/)

---

## 🔴 Main Branches

### `main` (Production)

**Purpose:** Contains only production-ready code

| Property | Value |
|----------|-------|
| **Source** | Merge from `release-*` or `hotfix-*` |
| **Versioning** | Semantic tags (e.g., `1.0.0`, `1.0.1`, `1.1.0`) |
| **Access** | Protected - requires Pull Request with review |
| **Deployment** | Automatic to production (Render/Cloud) |

**Rules:**
- ❌ Never commit directly
- ✅ All merge requests to `main` must pass code review
- ✅ Each merge creates a new version (tag)

---

### `develop` (Development)

**Purpose:** Integration branch for new features

| Property | Value |
|----------|-------|
| **Source** | Merge from `feature/*` and `release-*` / `hotfix-*` |
| **Versioning** | Pre-release (e.g., `1.2.0-alpha.123`) |
| **Access** | Protected - requires Pull Request |
| **Deployment** | Automatic to staging environment |

**Rules:**
- ✅ Main development line
- ✅ Always in testable state
- ✅ Starting point for feature branches
- ✅ Merging from feature branches required

---

## 🔵 Supporting Branches

### `feature/*` (Feature Branches)

**Convention:** `feature/<ticket-id>-<short-description>`

**Examples:**
- `feature/BLOG-42-add-comments`
- `feature/SHOP-15-cart-validation`
- `feature/AUTH-8-two-factor`

**Lifecycle:**
```
develop → feature/BLOG-42-add-comments → develop
         (checkout)                      (merge)
```

#### Procedure:

```bash
# 1. Create feature branch
git checkout develop
git pull origin develop
git checkout -b feature/BLOG-42-add-comments

# 2. Work on feature
git add .
git commit -m "feat: add comment system to posts

- Implement comment creation endpoint
- Add comment validation
- Create comment display UI"

# 3. Push and create Pull Request
git push -u origin feature/BLOG-42-add-comments

# 4. After PR approval - merge to develop
# (executed by CI/CD or manually)
git checkout develop
git pull origin develop
git merge --no-ff feature/BLOG-42-add-comments
git push origin develop

# 5. Delete feature branch
git push origin --delete feature/BLOG-42-add-comments
git branch -d feature/BLOG-42-add-comments
```

---

### `release-*` (Release Branches)

**Convention:** `release-<major>.<minor>` (e.g., `release-1.2`)

**Purpose:**
- Prepare new production version
- Testing before release
- Final fixes
- Release-specific bugfixes

**Source:** `develop`  
**Target:** `main` and back to `develop`

#### Procedure:

```bash
# 1. Create release branch from develop
git checkout develop
git pull origin develop
git checkout -b release-1.2

# 2. Update version numbers (if needed)
# In SimpleBlog: update version in project files
# .csproj, package.json, docs, etc.

# 3. Commit version bump
git commit -am "chore: bump version to 1.2.0"

# 4. Tag initial version (alpha)
git tag 1.2.0-alpha.1
git push origin release-1.2 --tags

# 5. Testing phase
# - Build and test on CI/CD
# - Bugfixes: commit to release branch
# - Tag new RC versions: 1.2.0-alpha.2, etc.

# 6. After release approval
git tag -a 1.2.0 -m "Release version 1.2.0"
git push origin --tags

# 7. Merge to main
git checkout main
git pull origin main
git merge --no-ff release-1.2
git push origin main

# 8. Merge back to develop
git checkout develop
git pull origin develop
git merge --no-ff release-1.2
git push origin develop

# 9. Delete release branch
git push origin --delete release-1.2
git branch -d release-1.2
```

---

### `hotfix-*` (Hotfix Branches)

**Convention:** `hotfix-<major>.<minor>.<patch>` (e.g., `hotfix-1.2.1`)

**Purpose:**
- Quick fixes for production bugs
- Immediately merges to `main` and `develop`
- Doesn't wait for release cycle

**Source:** `main`  
**Target:** `main` and `develop`

#### Procedure:

```bash
# 1. Create hotfix from main
git checkout main
git pull origin main
git checkout -b hotfix-1.2.1

# 2. Fix bug
git add .
git commit -m "fix: critical bug in comment validation

- Validate empty comments
- Prevent injection attack
- Add test case"

# 3. Update patch version
# .csproj, CHANGELOG.md, etc.
git commit -am "chore: bump version to 1.2.1"

# 4. Tag and push
git tag -a 1.2.1 -m "Hotfix for critical validation bug"
git push origin hotfix-1.2.1 --tags

# 5. Merge to main
git checkout main
git pull origin main
git merge --no-ff hotfix-1.2.1
git push origin main

# 6. Merge to develop (to include fix there too)
git checkout develop
git pull origin develop
git merge --no-ff hotfix-1.2.1
git push origin develop

# 7. Delete hotfix branch
git push origin --delete hotfix-1.2.1
git branch -d hotfix-1.2.1
```

---

### `bugfix/*` (Bugfix Branches - Optional)

**Convention:** `bugfix/<ticket-id>-<description>`

**Examples:**
- `bugfix/SHOP-89-cart-calculation`
- `bugfix/API-12-auth-timeout`

**When to use:**
- Bugfixes for new features in develop (not in production)
- Treat like feature branches
- Merge to develop (not main)

---

## 🏷️ Naming Conventions

### Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <subject>

<body>

<footer>
```

#### Types:

- `feat:` - New feature
- `fix:` - Bug fix
- `chore:` - Dependency updates, version bumps, etc.
- `docs:` - Documentation
- `style:` - Code formatting
- `refactor:` - Code restructuring without new features
- `perf:` - Performance improvements
- `test:` - Adding/changing tests
- `ci:` - CI/CD configuration

#### Examples:

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

---

### Branch Names

**Rules:**
- Lowercase only
- Hyphen separator: `-`
- Include ticket ID if available
- Descriptive suffix

**Incorrect:**
- `feature/add-comments` ❌ (no ID)
- `Feature/BLOG-42-Add-Comments` ❌ (uppercase)
- `feature_BLOG_42_add_comments` ❌ (underscore)

**Correct:**
- `feature/BLOG-42-add-comments` ✅
- `bugfix/SHOP-89-discount` ✅
- `hotfix-1.2.1` ✅

---

### Tag Names

Semantic Versioning (SemVer): `MAJOR.MINOR.PATCH[-PRERELEASE]`

```
1.0.0              # First release
1.0.1              # Patch (bugfix)
1.1.0              # Minor (new feature)
2.0.0              # Major (breaking change)

1.2.0-alpha.1      # Alpha pre-release
1.2.0-rc.1         # Release Candidate
```

---

## 📊 Workflow Diagrams

### Typical Development Workflow

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

---

### Release Cycle

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

## 💡 Practical Examples

### Example 1: Adding New Feature

**Scenario:** Add comment system to blog posts

```bash
# 1. Preparation
git clone https://github.com/yourorg/simpleblog.git
cd simpleblog
git checkout develop
git pull origin develop

# 2. Create feature branch
git checkout -b feature/BLOG-42-add-comments

# 3. Implement feature
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

# 5. Push to remote
git push -u origin feature/BLOG-42-add-comments

# 6. Create Pull Request on GitHub
#    - Describe changes
#    - Link ticket
#    - Request review

# 7. After approval (automatically by CI/CD)
#    - Merge to develop
#    - Feature branch deleted
```

---

### Example 2: Production Release

**Scenario:** Release v1.2.0

```bash
# 1. Prepare release
git checkout develop
git pull origin develop
git checkout -b release-1.2

# 2. Update version (if needed)
# SimpleBlog.Web/SimpleBlog.Web.csproj
# <Version>1.2.0</Version>

# 3. Update CHANGELOG
# docs/CHANGELOG.md
git add .
git commit -m "chore: prepare v1.2.0 release"

# 4. Tag alpha
git tag -a 1.2.0-alpha.1 -m "Alpha release for 1.2.0"
git push origin release-1.2 --tags

# 5. Testing in CI/CD
#    - Automated tests
#    - Build Docker images
#    - Deploy to staging

# 6. Hotfixes (if needed)
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

---

### Example 3: Production Hotfix

**Scenario:** Critical auth bug - v1.2.1

```bash
# 1. Create hotfix from main
git checkout main
git pull origin main
git checkout -b hotfix-1.2.1

# 2. Fix bug
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

## ✅ Best Practices

### DO

1. **Always pull before working**
   ```bash
   git checkout develop
   git pull origin develop
   ```

2. **Create feature branch from develop**
   ```bash
   git checkout -b feature/TICKET-ID-description
   ```

3. **Commit frequently with good messages**
   ```bash
   git commit -m "feat: add validation to order form"
   ```

4. **Push regularly**
   ```bash
   git push origin feature/TICKET-ID-description
   ```

5. **Create Pull Request for review**
   - Describe changes
   - Link issue/ticket
   - Request 1-2 reviewers

6. **Test before merge**
   ```bash
   dotnet build
   dotnet test
   ```

7. **Use merge commits (not squash)**
   ```bash
   git merge --no-ff feature/...
   ```

---

### DON'T

1. **Don't commit directly to main** ❌
   ```bash
   # Forbidden:
   git checkout main && git commit ...
   ```

2. **Don't commit directly to develop** (except small hotfixes) ❌
   ```bash
   # Forbidden:
   git checkout develop && git commit ...
   ```

3. **Don't mix multiple features in one PR** ❌
   ```bash
   # Forbidden:
   feature/BLOG-42-SHOP-15-AUTH-8-multiple-features
   ```

4. **Don't write unclear commit messages** ❌
   ```bash
   # Forbidden:
   git commit -m "fix stuff"
   git commit -m "wip"
   git commit -m "asdf"
   ```

5. **Don't force-push to shared branches** ❌
   ```bash
   # Forbidden:
   git push -f origin develop
   ```

6. **Don't ignore code review comments** ❌
   - Address all comments
   - Commit fixes to the same PR
   - Don't delete branch until merge

7. **Don't delete release/hotfix branches before merge** ❌

---

## 🔄 CI/CD Integration

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

---

### Branch Protection Rules

#### Main Branch:
- ✅ Require pull request reviews (≥1)
- ✅ Require status checks before merging
- ✅ Require branches up to date
- ✅ Restrict deletions
- ✅ Require signed commits (optional)

#### Develop Branch:
- ✅ Require pull request reviews (≥1)
- ✅ Require status checks
- ✅ Restrict deletions

#### Other Branches:
- ℹ️ No restrictions (developers manage themselves)

---

### Version Tagging in CI/CD

```bash
# Auto-tag on releases
git tag -a v1.2.0 -m "Release version 1.2.0"
git push origin v1.2.0

# Trigger deployment
# - Docker build & push
# - Deploy to Render
# - Run smoke tests
```

---

## 🛠️ Useful Commands

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

## 🔗 External Resources

- [Original GitFlow Model](https://nvie.com/posts/a-successful-git-branching-model/) by Vincent Driessen
- [GitVersion - GitFlow Documentation](https://gitversion.net/docs/learn/branching-strategies/gitflow/)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Semantic Versioning](https://semver.org/)
- [GitHub Flow](https://docs.github.com/en/get-started/quickstart/github-flow)

---

## 📚 Related Documents

- [Getting Started](./getting-started.md) - Initial setup guide
- [Database Guide](./database-guide.md) - PostgreSQL setup
- [Render Deployment](../deployment/render-guide.md) - Production deployment

---

## 💬 Contact & Questions

If you have questions about GitFlow in SimpleBlog project:

1. Read this document
2. Check [GitFlow Examples](https://gitversion.net/docs/learn/branching-strategies/gitflow/examples)
3. Ask in team chat (#development)
4. Open issue with `documentation` tag

---

## 📝 Changelog

| Date | Version | Changes |
|------|---------|---------|
| 2026-01-17 | 1.0.0 | Converted from legacy GITFLOW.md, translated to English, added proper metadata |
