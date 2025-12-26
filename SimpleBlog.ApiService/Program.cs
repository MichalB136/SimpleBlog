using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using SimpleBlog.ApiService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    // Broad CORS for dev and playground usage; tighten for production.
    options.AddPolicy("AllowDevClients", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// JWT Authentication
var jwtKey = "SimpleBlog_Secret_Key_For_Dev_1234567890"; // In production, use configuration/secrets
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// EF Core - SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=simpleblog.db"));

builder.Services.AddScoped<IPostRepository, EfPostRepository>();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

var app = builder.Build();

// Ensure database is created and apply migrations, then seed initial data if empty.
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();

        if (!db.Posts.Any())
        {
            var now = DateTimeOffset.UtcNow;
            var p1 = new PostEntity { Id = Guid.NewGuid(), Title = "Pierwszy wpis", Content = "Witaj w SimpleBlog!", Author = "System", CreatedAt = now };
            var p2 = new PostEntity { Id = Guid.NewGuid(), Title = "Drugi wpis", Content = "Edytuj lub dodaj nowe posty, aby zobaczyć React w akcji.", Author = "System", CreatedAt = now.AddMinutes(-30) };
            db.Posts.AddRange(p1, p2);
            db.SaveChanges();
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

// Login endpoint
app.MapPost("/login", (LoginRequest request, IUserRepository userRepo) =>
{
    var user = userRepo.ValidateUser(request.Username, request.Password);
    if (user == null)
    {
        return Results.Unauthorized();
    }

    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes("SimpleBlog_Secret_Key_For_Dev_1234567890");
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        }),
        Expires = DateTime.UtcNow.AddHours(8),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    return Results.Ok(new { token = tokenString, username = user.Username, role = user.Role });
});

var posts = app.MapGroup("/posts");

posts.MapGet(string.Empty, (IPostRepository repository) => Results.Ok(repository.GetAll()));

posts.MapGet("/{id:guid}", (Guid id, IPostRepository repository) =>
{
    var post = repository.GetById(id);
    return post is not null ? Results.Ok(post) : Results.NotFound();
});

posts.MapPost(string.Empty, (CreatePostRequest request, IPostRepository repository, HttpContext context) =>
{
    // Require admin role to create posts
    if (!context.User.IsInRole("Admin"))
    {
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

posts.MapPut("/{id:guid}", (Guid id, UpdatePostRequest request, IPostRepository repository) =>
{
    var updated = repository.Update(id, request);
    return updated is not null ? Results.Ok(updated) : Results.NotFound();
});

posts.MapDelete("/{id:guid}", (Guid id, IPostRepository repository, HttpContext context) =>
{
    // Require admin role to delete posts
    if (!context.User.IsInRole("Admin"))
    {
        return Results.Forbid();
    }
    
    return repository.Delete(id) ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization();

posts.MapGet("/{id:guid}/comments", (Guid id, IPostRepository repository) =>
{
    var comments = repository.GetComments(id);
    return comments is not null ? Results.Ok(comments) : Results.NotFound();
});

posts.MapPost("/{id:guid}/comments", (Guid id, CreateCommentRequest request, IPostRepository repository) =>
{
    if (string.IsNullOrWhiteSpace(request.Content))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["content"] = ["Content is required."] });
    }

    var created = repository.AddComment(id, request);
    return created is not null ? Results.Created($"/posts/{id}/comments/{created.Id}", created) : Results.NotFound();
});

app.MapDefaultEndpoints();

app.Run();

// Authentication-related types
record LoginRequest(string Username, string Password);

record User(string Username, string Role);

interface IUserRepository
{
    User? ValidateUser(string username, string password);
}

sealed class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<string, (string Password, string Role)> _users = new()
    {
        ["admin"] = ("admin123", "Admin"),
        ["user"] = ("user123", "User")
    };

    public User? ValidateUser(string username, string password)
    {
        if (_users.TryGetValue(username, out var userInfo) && userInfo.Password == password)
        {
            return new User(username, userInfo.Role);
        }
        return null;
    }
}
