using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddHttpClient("ApiService", client =>
{
    // "https+http" lets service discovery prefer HTTPS but fall back to HTTP during development.
    client.BaseAddress = new("https+http://apiservice");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

var api = app.MapGroup("/api");

api.MapPost("/login", async (LoginRequest request, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("ApiService");
    var response = await client.PostAsJsonAsync("/login", request);
    return await ToResult(response);
});

api.MapGet("/posts", async (IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("ApiService");
    var posts = await client.GetFromJsonAsync<List<Post>>("/posts");
    return posts is not null ? Results.Ok(posts) : Results.Problem("Unable to load posts.");
});

api.MapGet("/posts/{id:guid}", async (Guid id, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("ApiService");
    var post = await client.GetFromJsonAsync<Post>($"/posts/{id}");
    return post is not null ? Results.Ok(post) : Results.NotFound();
});

api.MapPost("/posts", async (CreatePostRequest request, IHttpClientFactory factory, HttpContext context) =>
{
    var client = factory.CreateClient("ApiService");
    
    // Forward auth token if present
    if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
    {
        client.DefaultRequestHeaders.Add("Authorization", authHeader.ToString());
    }
    
    var response = await client.PostAsJsonAsync("/posts", request);
    return await ToResult(response);
});

api.MapPut("/posts/{id:guid}", async (Guid id, UpdatePostRequest request, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("ApiService");
    var response = await client.PutAsJsonAsync($"/posts/{id}", request);
    return await ToResult(response);
});

api.MapDelete("/posts/{id:guid}", async (Guid id, IHttpClientFactory factory, HttpContext context) =>
{
    var client = factory.CreateClient("ApiService");
    
    // Forward auth token if present
    if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
    {
        client.DefaultRequestHeaders.Add("Authorization", authHeader.ToString());
    }
    
    var response = await client.DeleteAsync($"/posts/{id}");
    return Results.StatusCode((int)response.StatusCode);
});

api.MapGet("/posts/{id:guid}/comments", async (Guid id, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("ApiService");
    var comments = await client.GetFromJsonAsync<List<Comment>>($"/posts/{id}/comments");
    return comments is not null ? Results.Ok(comments) : Results.NotFound();
});

api.MapPost("/posts/{id:guid}/comments", async (Guid id, CreateCommentRequest request, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("ApiService");
    var response = await client.PostAsJsonAsync($"/posts/{id}/comments", request);
    return await ToResult(response);
});

app.MapFallbackToFile("index.html");

app.MapDefaultEndpoints();

app.Run();

static async Task<IResult> ToResult(HttpResponseMessage response)
{
    var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
    var body = await response.Content.ReadAsStringAsync();
    return new ProxyResult(body, contentType, (int)response.StatusCode);
}

record Post(Guid Id, string Title, string Content, string Author, DateTimeOffset CreatedAt, IReadOnlyList<Comment> Comments, string? ImageUrl = null);

record Comment(Guid Id, Guid PostId, string Author, string Content, DateTimeOffset CreatedAt);

record CreatePostRequest(string Title, string Content, string Author, string? ImageUrl = null);

record UpdatePostRequest(string Title, string Content, string Author, string? ImageUrl = null);

record CreateCommentRequest(string Content, string Author);

record LoginRequest(string Username, string Password);

sealed class ProxyResult : IResult
{
    private readonly string _body;
    private readonly string _contentType;
    private readonly int _statusCode;

    public ProxyResult(string body, string contentType, int statusCode)
    {
        _body = body;
        _contentType = contentType;
        _statusCode = statusCode;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = _statusCode;
        httpContext.Response.ContentType = _contentType;
        await httpContext.Response.WriteAsync(_body);
    }
}
