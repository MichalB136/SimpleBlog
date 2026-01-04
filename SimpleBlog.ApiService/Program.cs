using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using SimpleBlog.ApiService;
using SimpleBlog.ApiService.Data;
using SimpleBlog.Blog.Services;
using SimpleBlog.Shop.Services;
using SimpleBlog.Email.Services;
using SimpleBlog.Common;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Configure Kestrel to bind to Render's PORT environment variable if present
if (Environment.GetEnvironmentVariable("PORT") is string port)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddLogging();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    // Configure CORS based on environment
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
        ?? new[] { "http://localhost:5080", "http://localhost:7166", "https://localhost:7166" };
    
    options.AddPolicy("AllowDevClients", policy =>
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// JWT Authentication - Load from configuration
var jwtConfig = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtConfig["Key"] ?? throw new InvalidOperationException("JWT:Key is not configured");
var jwtIssuer = jwtConfig["Issuer"] ?? "SimpleBlog";
var jwtAudience = jwtConfig["Audience"] ?? "SimpleBlog";
var key = Encoding.UTF8.GetBytes(jwtKey);

Console.WriteLine($"JWT Config - Key length: {jwtKey.Length}, Issuer: {jwtIssuer}, Audience: {jwtAudience}");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("JWT Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var claims = context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}");
            logger.LogInformation("JWT Token validated successfully. Claims: {Claims}", string.Join(", ", claims ?? Array.Empty<string>()));
            return Task.CompletedTask;
        }
    };
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// EF Core - PostgreSQL (both local Aspire and Render deployment)
builder.AddNpgsqlDbContext<ApplicationDbContext>("blogdb");

// Register DbContext aliases for repositories
builder.Services.AddScoped<BlogDbContext>(sp => 
{
    var appDb = sp.GetRequiredService<ApplicationDbContext>();
    var connStr = appDb.Database.GetConnectionString();
    var options = new DbContextOptionsBuilder<BlogDbContext>()
        .UseNpgsql(connStr);
    
    return new BlogDbContext(options.Options);
});

builder.Services.AddScoped<ShopDbContext>(sp => 
{
    var appDb = sp.GetRequiredService<ApplicationDbContext>();
    var connStr = appDb.Database.GetConnectionString();
    var options = new DbContextOptionsBuilder<ShopDbContext>()
        .UseNpgsql(connStr);
    
    return new ShopDbContext(options.Options);
});

// Register repositories from service layers
builder.Services.AddScoped<IPostRepository, EfPostRepository>();
builder.Services.AddScoped<IProductRepository, EfProductRepository>();
builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

var app = builder.Build();

// Ensure database is created and apply migrations, then seed initial data if empty.
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // Ensure database exists (creates if missing, does nothing if exists)
        await db.Database.EnsureCreatedAsync();

        // Seed initial data if database is empty
        if (!await db.Posts.AnyAsync())
        {
            var now = DateTimeOffset.UtcNow;
            
            // Post 1 - z obrazem
            var p1 = new PostEntity 
            { 
                Id = Guid.NewGuid(), 
                Title = "Witaj w SimpleBlog!", 
                Content = "Twój nowy blog oparty na .NET Aspire i React jest gotowy! 🚀\n\nSimpleBlog to nowoczesna platforma blogowa, która łączy w sobie moc backendu .NET z dynamicznym frontendem React. Ciesz się szybkim prototypowaniem i skalowalnością.", 
                Author = SeedDataConstants.SystemUsername, 
                CreatedAt = now.AddDays(-7),
                ImageUrl = null
            };
            
            // Post 2 - bez obrazu
            var p2 = new PostEntity 
            { 
                Id = Guid.NewGuid(), 
                Title = "Przewodnik po funkcjach", 
                Content = "SimpleBlog oferuje wiele funkcji:\n\n✨ Tworzenie i edycja postów\n💬 System komentarzy\n🖼️ Wsparcie dla obrazów\n🌓 Tryb jasny i ciemny\n🔐 Uwierzytelnianie JWT\n📱 Responsywny design\n\nWszystko to w jednej, lekkiej aplikacji!", 
                Author = SeedDataConstants.AdminUsername, 
                CreatedAt = now.AddDays(-5)
            };
            
            // Post 3 - z obrazem
            var p3 = new PostEntity 
            { 
                Id = Guid.NewGuid(), 
                Title = "Technologie pod maską", 
                Content = "SimpleBlog wykorzystuje najnowsze technologie:\n\n🔹 Backend: .NET 9.0 z Aspire\n🔹 Frontend: React 18.3 + Bootstrap 5\n🔹 Baza danych: SQLite z Entity Framework Core\n🔹 Autoryzacja: JWT Bearer tokens\n🔹 API: Minimal APIs\n\nWszystko zoptymalizowane pod kątem wydajności i łatwości rozwoju.", 
                Author = "Tech Team", 
                CreatedAt = now.AddDays(-4),
                ImageUrl = null
            };
            
            // Post 4 - z obrazem
            var p4 = new PostEntity 
            { 
                Id = Guid.NewGuid(), 
                Title = "Krajobrazy programowania", 
                Content = "W świecie developmentu każdy dzień przynosi nowe wyzwania i możliwości. Od debugowania zagadkowych błędów po moment eureki, gdy kod wreszcie działa - to podróż pełna emocji.\n\nProgramowanie to nie tylko kod, to sztuka rozwiązywania problemów i tworzenia czegoś z niczego.", 
                Author = "CodePoet", 
                CreatedAt = now.AddDays(-3),
                ImageUrl = null
            };
            
            // Post 5 - bez obrazu
            var p5 = new PostEntity 
            { 
                Id = Guid.NewGuid(), 
                Title = "Tips & Tricks dla deweloperów", 
                Content = "💡 Killer tips dla każdego developera:\n\n1. Pisz testy jednostkowe - uratują Cię przed bugami\n2. Używaj kontroli wersji - Git to Twój przyjaciel\n3. Code review to nie krytyka, to nauka\n4. Dokumentuj swój kod - przyszłe 'ty' będzie wdzięczne\n5. Rób przerwy - wypalenie to prawdziwe zagrożenie\n6. Ucz się nowych technologii, ale nie gon za każdym trendem\n\nPamiętaj: kod pisze się raz, czyta wiele razy!", 
                Author = "DevMentor", 
                CreatedAt = now.AddDays(-2)
            };
            
            // Post 6 - z obrazem
            var p6 = new PostEntity 
            { 
                Id = Guid.NewGuid(), 
                Title = "Architektura mikrousług w praktyce", 
                Content = "Mikrousługi to nie srebrna kula, ale potężne narzędzie w odpowiednich rękach.\n\n.NET Aspire ułatwia orkiestrację usług, zapewniając:\n- Service discovery\n- Health checks\n- Distributed tracing\n- Centralized configuration\n\nTo zmienia zasady gry w budowaniu skalowalnych aplikacji!", 
                Author = "Architect", 
                CreatedAt = now.AddDays(-1),
                ImageUrl = null
            };
            
            // Post 7 - bez obrazu
            var p7 = new PostEntity 
            { 
                Id = Guid.NewGuid(), 
                Title = "Community matters", 
                Content = "Społeczność open source to serce innowacji technologicznych. Dzielenie się wiedzą, współpraca nad projektami i wzajemna pomoc - to fundament, na którym zbudowano internet.\n\nDołącz do społeczności, zadawaj pytania, dziel się swoją wiedzą. Każdy ekspert był kiedyś początkującym.", 
                Author = "OpenSourceFan", 
                CreatedAt = now.AddHours(-12)
            };
            
            db.Posts.AddRange(p1, p2, p3, p4, p5, p6, p7);
            await db.SaveChangesAsync();
            
            // Dodaj przykładowe komentarze
            var c1 = new CommentEntity 
            { 
                Id = Guid.NewGuid(), 
                PostId = p1.Id, 
                Author = "Jan Kowalski", 
                Content = "Świetny początek! Nie mogę się doczekać, aby zobaczyć więcej.", 
                CreatedAt = now.AddDays(-6) 
            };
            
            var c2 = new CommentEntity 
            { 
                Id = Guid.NewGuid(), 
                PostId = p1.Id, 
                Author = "Anna", 
                Content = "Design wygląda super! Dark mode działa rewelacyjnie 🌙", 
                CreatedAt = now.AddDays(-5).AddHours(-2) 
            };
            
            var c3 = new CommentEntity 
            { 
                Id = Guid.NewGuid(), 
                PostId = p3.Id, 
                Author = "DevExpert", 
                Content = "Aspire to game changer! Używam go w produkcji i jestem bardzo zadowolony.", 
                CreatedAt = now.AddDays(-3) 
            };
            
            var c4 = new CommentEntity 
            { 
                Id = Guid.NewGuid(), 
                PostId = p5.Id, 
                Author = "Junior Dev", 
                Content = "Dzięki za tipy! Punkt o przerwach szczególnie trafiony 😅", 
                CreatedAt = now.AddDays(-1) 
            };
            
            db.Comments.AddRange(c1, c2, c3, c4);
            await db.SaveChangesAsync();
        }

        // Seed products if empty
        if (!await db.Products.AnyAsync())
        {
            var now = DateTimeOffset.UtcNow;
            
            var seedProducts = new[]
            {
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Koszulka SimpleBlog",
                    Description = "Premium koszulka bawełniana z logo SimpleBlog. Dostępna w różnych rozmiarach.",
                    Price = 79.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryClothing,
                    Stock = 50,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Kubek programisty",
                    Description = "Kubek ceramiczny z motywacyjnym cytatem. Idealny do porannej kawy podczas kodowania.",
                    Price = 39.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryAccessories,
                    Stock = 100,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Notatnik developerski",
                    Description = "Notatnik w linie z twardą okładką. Idealny do szkicowania architektury i robienia notatek.",
                    Price = 29.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryOffice,
                    Stock = 75,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Naklejki kodu",
                    Description = "Zestaw 20 naklejek z motywami programistycznymi. Ozdobnymi laptop lub inne gadżety!",
                    Price = 19.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryAccessories,
                    Stock = 150,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Bluza z kapturem",
                    Description = "Ciepła bluza z logo SimpleBlog. Idealna na długie noce kodowania.",
                    Price = 149.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryClothing,
                    Stock = 30,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Mata pod mysz",
                    Description = "Duża mata pod mysz z logo SimpleBlog. Antypoślizgowa powierzchnia.",
                    Price = 49.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryOffice,
                    Stock = 60,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Klawiatura mechaniczna RGB",
                    Description = "Profesjonalna klawiatura mechaniczna z podświetleniem RGB. Przełączniki Cherry MX Blue.",
                    Price = 399.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryElectronics,
                    Stock = 25,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Mysz gamingowa",
                    Description = "Mysz optyczna 16000 DPI z programowalnymi przyciskami. Ergonomiczny design.",
                    Price = 199.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryElectronics,
                    Stock = 40,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Plecak na laptopa",
                    Description = "Wodoodporny plecak z kieszenią na laptopa do 17 cali. Wiele przegródek organizacyjnych.",
                    Price = 179.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryAccessories,
                    Stock = 45,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Słuchawki bezprzewodowe",
                    Description = "Słuchawki Bluetooth z aktywną redukcją szumów. Do 30h odtwarzania.",
                    Price = 299.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryElectronics,
                    Stock = 35,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Stojak pod laptopa",
                    Description = "Aluminiowy stojak ergonomiczny. Regulowana wysokość, doskonała wentylacja.",
                    Price = 89.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryOffice,
                    Stock = 55,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Lampka LED na USB",
                    Description = "Elastyczna lampka LED zasilana przez USB. Idealna do pracy wieczorem.",
                    Price = 34.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryOffice,
                    Stock = 80,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Koszulka 'Hello World'",
                    Description = "Kultowa koszulka z napisem Hello World. Must-have dla każdego programisty!",
                    Price = 69.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryClothing,
                    Stock = 70,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Podkładka chłodząca",
                    Description = "Aktywna podkładka chłodząca pod laptopa z 4 wentylatorami. 2x USB.",
                    Price = 129.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryElectronics,
                    Stock = 28,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Bidon termiczny",
                    Description = "Bidon stalowy 500ml. Utrzymuje temperaturę przez 12h. Logo SimpleBlog.",
                    Price = 59.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryAccessories,
                    Stock = 90,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Poduszka pod nadgarstek",
                    Description = "Memory foam poduszka pod nadgarstek. Redukuje zmęczenie podczas długiej pracy.",
                    Price = 44.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryOffice,
                    Stock = 65,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Powerbank 20000mAh",
                    Description = "Mocny powerbank z szybkim ładowaniem USB-C i Qi wireless charging.",
                    Price = 159.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryElectronics,
                    Stock = 42,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Czapka SimpleBlog",
                    Description = "Bawełniana czapka z daszkiem. Haftowane logo SimpleBlog. Regulowany rozmiar.",
                    Price = 54.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryClothing,
                    Stock = 48,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Hub USB-C 7w1",
                    Description = "Uniwersalny hub USB-C: 3x USB 3.0, HDMI 4K, SD/microSD, USB-C PD 100W.",
                    Price = 189.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryElectronics,
                    Stock = 32,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Skarpety programisty",
                    Description = "Kolorowe skarpety z motywami kodu. Zestaw 3 pary. 80% bawełna.",
                    Price = 39.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryClothing,
                    Stock = 120,
                    CreatedAt = now
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Kabel USB-C premium",
                    Description = "Wzmocniony kabel USB-C 2m. Szybkie ładowanie 100W i transfer danych 40Gbps.",
                    Price = 49.99m,
                    ImageUrl = null,
                    Category = SeedDataConstants.CategoryElectronics,
                    Stock = 95,
                    CreatedAt = now
                }
            };
            
            db.Products.AddRange(seedProducts);
            await db.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing database: {ex}");
        throw;
    }
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowDevClients");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

// Login endpoint
app.MapPost("/login", (LoginRequest request, IUserRepository userRepo, ILogger<Program> logger) =>
{
    // Validate input
    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
    {
        logger.LogWarning("Login attempt with empty credentials");
        return Results.BadRequest(new { error = "Username and password are required" });
    }

    var user = userRepo.ValidateUser(request.Username, request.Password);
    if (user == null)
    {
        logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
        return Results.Unauthorized();
    }

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        }),
        Expires = DateTime.UtcNow.AddHours(8),
        Issuer = jwtIssuer,
        Audience = jwtAudience,
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    
    logger.LogInformation("Generating JWT token with Issuer: {Issuer}, Audience: {Audience}, Key length: {KeyLength}", jwtIssuer, jwtAudience, key.Length);
    
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    logger.LogInformation("Successful login for user: {Username}, Token length: {TokenLength}", user.Username, tokenString.Length);
    return Results.Ok(new { token = tokenString, username = user.Username, role = user.Role });
});

var posts = app.MapGroup("/posts");

posts.MapGet(string.Empty, (IPostRepository repository) => Results.Ok(repository.GetAll()));

posts.MapGet("/{id:guid}", (Guid id, IPostRepository repository) =>
{
    var post = repository.GetById(id);
    return post is not null ? Results.Ok(post) : Results.NotFound();
});

posts.MapPost(string.Empty, (CreatePostRequest request, IPostRepository repository, HttpContext context, ILogger<Program> logger) =>
{
    logger.LogInformation("POST /posts called by {UserName}", context.User.Identity?.Name);
    
    // Require admin role to create posts
    if (!context.User.IsInRole(SeedDataConstants.AdminUsername))
    {
        logger.LogWarning("User {UserName} attempted to create post without Admin role", context.User.Identity?.Name);
        return Results.Forbid();
    }

    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["title"] = ["Title is required."] });
    }

    if (string.IsNullOrWhiteSpace(request.Content))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["content"] = ["Content is required."] });
    }

    var created = repository.Create(request);
    return Results.Created($"/posts/{created.Id}", created);
}).RequireAuthorization();

posts.MapPut("/{id:guid}", (Guid id, UpdatePostRequest request, IPostRepository repository, ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(request.Title) && string.IsNullOrWhiteSpace(request.Content))
    {
        return Results.BadRequest(new { error = "At least title or content must be provided" });
    }

    var updated = repository.Update(id, request);
    if (updated is null)
    {
        logger.LogWarning("Update attempt for non-existent post: {PostId}", id);
        return Results.NotFound();
    }
    
    logger.LogInformation("Post updated: {PostId}", id);
    return Results.Ok(updated);
}).RequireAuthorization();

posts.MapDelete("/{id:guid}", (Guid id, IPostRepository repository, HttpContext context, ILogger<Program> logger) =>
{
    // Require admin role to delete posts
    if (!context.User.IsInRole(SeedDataConstants.AdminUsername))
    {
        logger.LogWarning("Unauthorized delete attempt for post: {PostId}", id);
        return Results.Forbid();
    }
    
    var deleted = repository.Delete(id);
    if (!deleted)
    {
        logger.LogWarning("Delete attempt for non-existent post: {PostId}", id);
        return Results.NotFound();
    }
    
    logger.LogInformation("Post deleted: {PostId}", id);
    return Results.NoContent();
}).RequireAuthorization();

posts.MapGet("/{id:guid}/comments", (Guid id, IPostRepository repository) =>
{
    var comments = repository.GetComments(id);
    return comments is not null ? Results.Ok(comments) : Results.NotFound();
});

posts.MapPost("/{id:guid}/comments", (Guid id, CreateCommentRequest request, IPostRepository repository, ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(request.Content))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["content"] = ["Content is required."] });
    }

    if (string.IsNullOrWhiteSpace(request.Author))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["author"] = ["Author is required."] });
    }

    var created = repository.AddComment(id, request);
    if (created is null)
    {
        logger.LogWarning("Comment creation attempt for non-existent post: {PostId}", id);
        return Results.NotFound();
    }
    
    logger.LogInformation("Comment added to post: {PostId}", id);
    return Results.Created($"/posts/{id}/comments/{created.Id}", created);
});

// Products endpoints
var products = app.MapGroup("/products");

products.MapGet(string.Empty, (IProductRepository repository) => Results.Ok(repository.GetAll()));

products.MapGet("/{id:guid}", (Guid id, IProductRepository repository) =>
{
    var product = repository.GetById(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

products.MapPost(string.Empty, (CreateProductRequest request, IProductRepository repository, HttpContext context, ILogger<Program> logger) =>
{
    if (!context.User.IsInRole(SeedDataConstants.AdminUsername))
    {
        logger.LogWarning("User {UserName} attempted to create product without Admin role", context.User.Identity?.Name);
        return Results.Forbid();
    }

    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["name"] = ["Name is required."] });
    }

    var created = repository.Create(request);
    logger.LogInformation("Product created: {ProductId}", created.Id);
    return Results.Created($"/products/{created.Id}", created);
}).RequireAuthorization();

products.MapPut("/{id:guid}", (Guid id, UpdateProductRequest request, IProductRepository repository, HttpContext context, ILogger<Program> logger) =>
{
    if (!context.User.IsInRole(SeedDataConstants.AdminUsername))
    {
        logger.LogWarning("Unauthorized update attempt for product: {ProductId}", id);
        return Results.Forbid();
    }

    var updated = repository.Update(id, request);
    if (updated is null)
    {
        logger.LogWarning("Update attempt for non-existent product: {ProductId}", id);
        return Results.NotFound();
    }
    
    logger.LogInformation("Product updated: {ProductId}", id);
    return Results.Ok(updated);
}).RequireAuthorization();

products.MapDelete("/{id:guid}", (Guid id, IProductRepository repository, HttpContext context, ILogger<Program> logger) =>
{
    if (!context.User.IsInRole(SeedDataConstants.AdminUsername))
    {
        logger.LogWarning("Unauthorized delete attempt for product: {ProductId}", id);
        return Results.Forbid();
    }
    
    var deleted = repository.Delete(id);
    if (!deleted)
    {
        logger.LogWarning("Delete attempt for non-existent product: {ProductId}", id);
        return Results.NotFound();
    }
    
    logger.LogInformation("Product deleted: {ProductId}", id);
    return Results.NoContent();
}).RequireAuthorization();

// Orders endpoints
var orders = app.MapGroup("/orders");

orders.MapGet(string.Empty, (IOrderRepository repository, HttpContext context, ILogger<Program> logger) =>
{
    if (!context.User.IsInRole("Admin"))
    {
        logger.LogWarning("Unauthorized attempt to view all orders");
        return Results.Forbid();
    }
    
    return Results.Ok(repository.GetAll());
}).RequireAuthorization();

orders.MapGet("/{id:guid}", (Guid id, IOrderRepository repository, HttpContext context, ILogger<Program> logger) =>
{
    if (!context.User.IsInRole("Admin"))
    {
        logger.LogWarning("Unauthorized attempt to view order: {OrderId}", id);
        return Results.Forbid();
    }
    
    var order = repository.GetById(id);
    return order is not null ? Results.Ok(order) : Results.NotFound();
}).RequireAuthorization();

orders.MapPost(string.Empty, async (CreateOrderRequest request, IOrderRepository repository, IEmailService emailService, ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(request.CustomerName))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["customerName"] = ["Customer name is required."] });
    }

    if (string.IsNullOrWhiteSpace(request.CustomerEmail))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["customerEmail"] = ["Customer email is required."] });
    }

    if (request.Items is null || request.Items.Count == 0)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["items"] = ["At least one item is required."] });
    }

    var created = repository.Create(request);
    logger.LogInformation("Order created: {OrderId}, Total: {Total}", created.Id, created.TotalAmount);
    
    // Send email notification
    await emailService.SendOrderConfirmationAsync(request.CustomerEmail, request.CustomerName, created);
    logger.LogInformation("Order confirmation email sent to: {Email}", request.CustomerEmail);
    
    return Results.Created($"/orders/{created.Id}", created);
});

app.MapDefaultEndpoints();

await app.RunAsync();
