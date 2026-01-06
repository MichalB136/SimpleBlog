# SimpleBlog.Tests

Unit and integration tests for the SimpleBlog application using **xUnit** testing framework with **Entity Framework Core InMemory** database for isolation.

## 📋 Overview

This module contains comprehensive tests covering:
- **Validation** - Input validation for all API models
- **User Management** - Authentication and user repository operations
- **Blog Module** - Post and comment operations with EF Core
- **Shop Module** - Product and order management with EF Core

**Framework:** xUnit 2.9.2  
**Database Testing:** EF Core InMemory (isolation per test)  
**Code Coverage:** Coverlet 6.0.2  

## 🗂️ Test Structure

### [ValidationTests.cs](ValidationTests.cs)
Tests for request model validation across all domains.

**Test Cases:**
- `CreatePostRequest` - Title/content validation
- `CreateProductRequest` - Price/stock validation
- `CreateOrderRequest` - Email/shipping validation
- `LoginRequest` - Credentials validation

**Pattern:** Each test verifies invalid states are properly caught at API boundaries.

```csharp
[Theory]
[InlineData("")]
[InlineData(null)]
[InlineData("   ")]
public void CreatePostRequest_InvalidTitle_ShouldBeValidated(string? title)
{
    var request = new CreatePostRequest(title!, "Content", "Author", null);
    Assert.True(string.IsNullOrWhiteSpace(request.Title));
}
```

---

### [UserRepositoryTests.cs](UserRepositoryTests.cs)
Tests for user authentication and repository operations.

**Test Cases:**
- `ValidateUser_CorrectCredentials_ReturnsUser` - Successful authentication
- `ValidateUser_IncorrectPassword_ReturnsNull` - Failed password validation
- `ValidateUser_NonExistentUser_ReturnsNull` - Unknown user handling
- Role-based access (`Admin`, `User`)
- Edge cases (empty username, case sensitivity)

**Test Data:**
```csharp
["admin"] = ("admin123", "Admin")
["user"] = ("user123", "User")
["testuser"] = ("testpass", "User")
```

---

### [BlogRepositoryTests.cs](BlogRepositoryTests.cs)
Tests for post and comment management using EF Core InMemory.

**Test Cases - Posts:**
- `GetAll_ReturnsAllPosts_OrderedByCreatedAtDescending` - Ordering
- `GetById_PostExists_ReturnsPost` - Retrieve single post
- `GetById_PostDoesNotExist_ReturnsNull` - Not found handling
- `Create_ValidRequest_CreatesPost` - Create operations
- `Update_PostExists_UpdatesSuccessfully` - Update operations
- `Delete_PostExists_DeletesSuccessfully` - Delete operations

**Test Cases - Comments:**
- `AddComment_ValidComment_ReturnsComment` - Add to existing post
- `GetComments_PostHasComments_ReturnsAll` - Retrieve comments
- `DeleteComment_Exists_DeletesSuccessfully` - Remove comment

**Database Isolation:**
```csharp
private static BlogDbContext CreateInMemoryContext()
{
    var options = new DbContextOptionsBuilder<BlogDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
    return new BlogDbContext(options);
}
```
Each test gets a unique in-memory database instance.

---

### [ShopRepositoryTests.cs](ShopRepositoryTests.cs)
Tests for product and order operations with EF Core InMemory.

**Test Cases - Products:**
- `GetAll_Products_ReturnsAllOrderedByCreatedAt` - Product listing
- `Create_Product_ValidData_CreatesProduct` - Product creation
- `Update_Product_Exists_UpdatesSuccessfully` - Product updates
- `Delete_Product_Exists_DeletesSuccessfully` - Product deletion
- `GetByCategory_ReturnsProductsByCategory` - Filtering

**Test Cases - Orders:**
- `Create_Order_ValidRequest_CreatesOrder` - Order creation
- `GetById_OrderExists_ReturnsOrder` - Retrieve order
- `GetAll_ReturnsAllOrders_OrderedByCreatedAtDescending` - Order listing

---

## 🚀 Running Tests

### Run all tests:
```powershell
dotnet test SimpleBlog.Tests
```

### Run specific test class:
```powershell
dotnet test SimpleBlog.Tests --filter "FullyQualifiedName~ValidationTests"
```

### Run with coverage:
```powershell
dotnet test SimpleBlog.Tests --collect:"XPlat Code Coverage"
```

### Run in Watch Mode (development):
```powershell
dotnet watch test SimpleBlog.Tests
```

### Run specific test method:
```powershell
dotnet test SimpleBlog.Tests --filter "FullyQualifiedName~ValidateUser_CorrectCredentials_ReturnsUser"
```

---

## 🏗️ Test Patterns Used

### Arrange-Act-Assert (AAA)
```csharp
[Fact]
public void FeatureName_Scenario_ExpectedResult()
{
    // Arrange - Set up test data
    var repository = new TestRepository();
    
    // Act - Execute the operation
    var result = repository.DoSomething();
    
    // Assert - Verify the outcome
    Assert.NotNull(result);
    Assert.Equal("expected", result.Value);
}
```

### Theory Testing (Parameterized)
```csharp
[Theory]
[InlineData("")]
[InlineData(null)]
[InlineData("   ")]
public void Test_InvalidInput_ShouldFail(string input)
{
    Assert.True(string.IsNullOrWhiteSpace(input));
}
```

### Database Isolation
Each test using EF Core gets its own in-memory database via `Guid.NewGuid().ToString()` to prevent cross-test contamination.

---

## 📦 Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| xunit | 2.9.2 | Test framework |
| Microsoft.NET.Test.Sdk | 17.12.0 | Test discovery & execution |
| Microsoft.EntityFrameworkCore.InMemory | 9.0.10 | In-memory database for testing |
| Moq | 4.20.72 | Mocking framework (optional) |
| FluentAssertions | 8.8.0 | Assertion extensions (optional) |
| coverlet.collector | 6.0.2 | Code coverage reporting |
| xunit.runner.visualstudio | 2.8.2 | Visual Studio integration |

---

## 📊 Coverage Goals

Current test coverage targets:
- **Validation** - 100% (all request models)
- **Repositories** - 90%+ (CRUD operations)
- **User Authentication** - 100%
- **Business Logic** - 85%+

Run coverage reports:
```powershell
dotnet test SimpleBlog.Tests /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

---

## 🔧 Adding New Tests

### 1. Create new test class:
```csharp
namespace SimpleBlog.Tests;

public sealed class MyFeatureTests
{
    [Fact]
    public void MyTest_Scenario_ExpectedResult()
    {
        // Arrange
        
        // Act
        
        // Assert
    }
}
```

### 2. Use existing patterns:
- InMemory database for repository tests (see BlogRepositoryTests)
- Theory attributes for multiple input scenarios
- Clear naming: `Method_Scenario_ExpectedResult`

### 3. Leverage GlobalUsings:
```csharp
// GlobalUsings.cs automatically imports:
global using SimpleBlog.Common.Models;
global using SimpleBlog.Common.Interfaces;
global using SimpleBlog.Common.Exceptions;
global using SimpleBlog.Common.Utilities;
```

---

## ⚠️ Important Notes

### Security Tests
Password validation tests use constant-time comparison (`ConstantTimeEquals`) to prevent timing attacks. See [InMemoryUserRepository.cs](../SimpleBlog.ApiService/InMemoryUserRepository.cs).

### Email Validation
Email validation tests expect proper `MailAddress` format validation, not just `@` symbol presence.

### Database Tests
All EF Core tests use InMemory database:
- ✅ **Fast execution** - No I/O operations
- ✅ **Isolated** - Each test gets unique database instance
- ⚠️ **Limitations** - Some EF Core features not fully supported

### Test Data
- Admin credentials (dev): `admin` / `admin123`
- User credentials (dev): `user` / `user123`
- See [SeedDataConstants.cs](../SimpleBlog.ApiService/Constants.cs) for more

---

## 🔗 Related Documentation

- [Common API Module](../docs/COMMON_API_MODULE.md) - Configuration and validation
- [Architecture Documentation](../docs/README.md) - Overall system design
- [Endpoint Configuration](../docs/ENDPOINT_CONFIGURATION.md) - API configuration

---

## 📝 Continuous Integration

Tests run automatically on:
- **Pull Requests** - All tests must pass
- **Merge to main** - Full coverage verification
- **Release builds** - No warnings allowed

See `.github/workflows/` for CI configuration.

---

## ❓ Troubleshooting

### Tests timeout in InMemory database
Likely infinite loop in test logic. Verify query conditions.

### Cross-test contamination
Ensure using `using (var context = CreateInMemoryContext())` pattern for proper disposal.

### Mock object not working
Check that dependencies are properly registered in DI container.

### Coverage not generating
Ensure `coverlet.collector` package reference exists and run: 
```powershell
dotnet test /p:CollectCoverage=true
```

---

**Last Updated:** January 2026  
**Maintainer:** SimpleBlog Development Team
