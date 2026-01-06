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

// Load shared endpoint configuration from hierarchy:
// 1. appsettings.shared.json (base)
// 2. appsettings.shared.{Environment}.json (environment override)
// 3. Environment variables (SimpleBlog_* prefix)
builder.Configuration.AddSharedConfiguration(builder.Environment.EnvironmentName);

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

// Add API configurations (endpoints and authorization) to DI container
builder.Services.AddApiConfigurations(builder.Configuration);

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

// EF Core - PostgreSQL (external docker-compose instance)
var connectionString = builder.Configuration.GetConnectionString("blogdb") 
    ?? throw new InvalidOperationException("Connection string 'blogdb' not found.");

// Log connection string for debugging (mask password)
var maskedConnectionString = connectionString.Contains("Password=")
    ? System.Text.RegularExpressions.Regex.Replace(connectionString, @"Password=[^;]*", "Password=***")
    : connectionString;
Console.WriteLine($"Using connection string: {maskedConnectionString}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register DbContext aliases for repositories
builder.Services.AddScoped<BlogDbContext>(sp => 
{
    var options = new DbContextOptionsBuilder<BlogDbContext>()
        .UseNpgsql(connectionString);
    
    return new BlogDbContext(options.Options);
});

builder.Services.AddScoped<ShopDbContext>(sp => 
{
    var options = new DbContextOptionsBuilder<ShopDbContext>()
        .UseNpgsql(connectionString);
    
    return new ShopDbContext(options.Options);
});

// Register repositories from service layers
builder.Services.AddScoped<IPostRepository, EfPostRepository>();
builder.Services.AddScoped<IProductRepository, EfProductRepository>();
builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

var app = builder.Build();

// Apply database migrations automatically
// PostgreSQL container must be running before application starts
// User must start with: docker-compose up -d
await MigrateDatabaseAsync(app);

// Seed database with dummy data if configured
var seedData = app.Configuration.GetValue<bool>("Database:SeedData");
if (seedData)
{
    await SeedDatabaseAsync(app);
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Get configurations from DI container
var endpointConfig = app.Services.GetRequiredService<EndpointConfiguration>();
var authConfig = app.Services.GetRequiredService<AuthorizationConfiguration>();

app.UseCors("AllowDevClients");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks(endpointConfig.Health);

// Login endpoint
app.MapPost(endpointConfig.Login, (LoginRequest request, IUserRepository userRepo, ILogger<Program> logger) =>
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
        Expires = DateTime.UtcNow.AddHours(authConfig.TokenExpirationHours),
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

var posts = app.MapGroup(endpointConfig.Posts.Base);

posts.MapGet(endpointConfig.Posts.GetAll, (IPostRepository repository) => Results.Ok(repository.GetAll()));

posts.MapGet(endpointConfig.Posts.GetById, (Guid id, IPostRepository repository) =>
{
    var post = repository.GetById(id);
    return post is not null ? Results.Ok(post) : Results.NotFound();
});

posts.MapPost(endpointConfig.Posts.Create, (CreatePostRequest request, IPostRepository repository, HttpContext context, ILogger<Program> logger) =>
{
    logger.LogInformation("POST {Endpoint} called by {UserName}", endpointConfig.Posts.Base, context.User.Identity?.Name);
    
    // Require admin role to create posts
    if (authConfig.RequireAdminForPostCreate && !context.User.IsInRole(SeedDataConstants.AdminUsername))
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
    return Results.Created($"{endpointConfig.Posts.Base}/{created.Id}", created);
}).RequireAuthorization();

posts.MapPut(endpointConfig.Posts.Update, (Guid id, UpdatePostRequest request, IPostRepository repository, ILogger<Program> logger) =>
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

posts.MapDelete(endpointConfig.Posts.Delete, (Guid id, IPostRepository repository, HttpContext context, ILogger<Program> logger) =>
{
    // Require admin role to delete posts
    if (authConfig.RequireAdminForPostDelete && !context.User.IsInRole(SeedDataConstants.AdminUsername))
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

posts.MapGet(endpointConfig.Posts.GetComments, (Guid id, IPostRepository repository) =>
{
    var comments = repository.GetComments(id);
    return comments is not null ? Results.Ok(comments) : Results.NotFound();
});

posts.MapPost(endpointConfig.Posts.AddComment, (Guid id, CreateCommentRequest request, IPostRepository repository, ILogger<Program> logger) =>
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
    return Results.Created($"{endpointConfig.Posts.Base}/{id}/comments/{created.Id}", created);
});

// Products endpoints
var products = app.MapGroup(endpointConfig.Products.Base);

products.MapGet(endpointConfig.Products.GetAll, (IProductRepository repository) => Results.Ok(repository.GetAll()));

products.MapGet(endpointConfig.Products.GetById, (Guid id, IProductRepository repository) =>
{
    var product = repository.GetById(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

products.MapPost(endpointConfig.Products.Create, (CreateProductRequest request, IProductRepository repository, HttpContext context, ILogger<Program> logger) =>
{
    if (authConfig.RequireAdminForProductCreate && !context.User.IsInRole(SeedDataConstants.AdminUsername))
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
    return Results.Created($"{endpointConfig.Products.Base}/{created.Id}", created);
}).RequireAuthorization();

products.MapPut(endpointConfig.Products.Update, (Guid id, UpdateProductRequest request, IProductRepository repository, HttpContext context, ILogger<Program> logger) =>
{
    if (authConfig.RequireAdminForProductUpdate && !context.User.IsInRole(SeedDataConstants.AdminUsername))
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

products.MapDelete(endpointConfig.Products.Delete, (Guid id, IProductRepository repository, HttpContext context, ILogger<Program> logger) =>
{
    if (authConfig.RequireAdminForProductDelete && !context.User.IsInRole(SeedDataConstants.AdminUsername))
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
var orders = app.MapGroup(endpointConfig.Orders.Base);

orders.MapGet(endpointConfig.Orders.GetAll, (IOrderRepository repository, HttpContext context, ILogger<Program> logger) =>
{
    if (authConfig.RequireAdminForOrderView && !context.User.IsInRole("Admin"))
    {
        logger.LogWarning("Unauthorized attempt to view all orders");
        return Results.Forbid();
    }
    
    return Results.Ok(repository.GetAll());
}).RequireAuthorization();

orders.MapGet(endpointConfig.Orders.GetById, (Guid id, IOrderRepository repository, HttpContext context, ILogger<Program> logger) =>
{
    if (authConfig.RequireAdminForOrderView && !context.User.IsInRole("Admin"))
    {
        logger.LogWarning("Unauthorized attempt to view order: {OrderId}", id);
        return Results.Forbid();
    }
    
    var order = repository.GetById(id);
    return order is not null ? Results.Ok(order) : Results.NotFound();
}).RequireAuthorization();

orders.MapPost(endpointConfig.Orders.Create, async (CreateOrderRequest request, IOrderRepository repository, IEmailService emailService, ILogger<Program> logger) =>
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
    
    return Results.Created($"{endpointConfig.Orders.Base}/{created.Id}", created);
});

app.MapDefaultEndpoints();

await app.RunAsync();

// Apply database migrations automatically
static async Task MigrateDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        logger.LogInformation("Applying database migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations completed successfully");
        
        // Ensure other DbContexts use the same database
        var blogDb = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
        await blogDb.Database.MigrateAsync();
        
        var shopDb = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
        await shopDb.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying database migrations");
        throw;
    }
}

// Seed database with dummy data
static async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var appDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var blogDb = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
        var shopDb = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
        
        await DatabaseSeeder.SeedAsync(appDb, blogDb, shopDb, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error seeding database");
        throw;
    }
}
