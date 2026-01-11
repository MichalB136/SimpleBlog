using SimpleBlog.Web;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure Kestrel to bind to Render's PORT environment variable if present
if (Environment.GetEnvironmentVariable("PORT") is string port)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Support for both Aspire service discovery (dev) and external API URL (production/Render)
var apiBaseUrl = builder.Configuration["Api:BaseUrl"] 
    ?? Environment.GetEnvironmentVariable("API_BASE_URL")
    ?? "https+http://apiservice";

builder.Services.AddHttpClient(ApiConstants.ClientName, client =>
{
    client.BaseAddress = new(apiBaseUrl);
});

builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

var api = app.MapGroup("/api");

// Authentication
api.MapPost(EndpointPaths.Login, 
    async (LoginRequest request, IHttpClientFactory factory, ILogger<Program> logger) =>
        await ApiProxyHelper.ProxyPostRequest(factory, EndpointPaths.Login, request, null, logger));

api.MapPost(EndpointPaths.Register, 
    async (SimpleBlog.Common.Models.RegisterRequest request, IHttpClientFactory factory, ILogger<Program> logger) =>
        await ApiProxyHelper.ProxyPostRequest(factory, EndpointPaths.Register, request, null, logger));

// Posts
api.MapGet(EndpointPaths.Posts, 
    async (IHttpClientFactory factory, ILogger<Program> logger) =>
        await ApiProxyHelper.ProxyGetRequest(factory, EndpointPaths.Posts, logger));

api.MapGet("/posts/{id:guid}", 
    async (Guid id, IHttpClientFactory factory, ILogger<Program> logger) =>
        await ApiProxyHelper.ProxyGetRequest(factory, $"/posts/{id}", logger));

api.MapPost(EndpointPaths.Posts, 
    async (CreatePostRequest request, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
        await ApiProxyHelper.ProxyPostRequest(factory, EndpointPaths.Posts, request, context, logger));

api.MapPut("/posts/{id:guid}", 
    async (Guid id, UpdatePostRequest request, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
        await ApiProxyHelper.ProxyPutRequest(factory, $"/posts/{id}", request, context, logger));

api.MapDelete("/posts/{id:guid}", 
    async (Guid id, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
        await ApiProxyHelper.ProxyDeleteRequest(factory, $"/posts/{id}", context, logger));

api.MapGet("/posts/{id:guid}/comments", 
    async (Guid id, IHttpClientFactory factory, ILogger<Program> logger) =>
        await ApiProxyHelper.ProxyGetRequest(factory, $"/posts/{id}/comments", logger));

api.MapPost("/posts/{id:guid}/comments", 
    async (Guid id, CreateCommentRequest request, IHttpClientFactory factory, ILogger<Program> logger) =>
        await ApiProxyHelper.ProxyPostRequest(factory, $"/posts/{id}/comments", request, null, logger));

// Products
api.MapGet(EndpointPaths.Products, 
    async (IHttpClientFactory factory, ILogger<Program> logger) =>
        await ApiProxyHelper.ProxyGetRequest(factory, EndpointPaths.Products, logger));

api.MapGet("/products/{id:guid}", 
    async (Guid id, IHttpClientFactory factory, ILogger<Program> logger) =>
        await ApiProxyHelper.ProxyGetRequest(factory, $"/products/{id}", logger));

api.MapPost(EndpointPaths.Products, 
    async (CreateProductRequest request, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
        await ApiProxyHelper.ProxyPostRequest(factory, EndpointPaths.Products, request, context, logger));

api.MapPut("/products/{id:guid}", 
    async (Guid id, UpdateProductRequest request, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
        await ApiProxyHelper.ProxyPutRequest(factory, $"/products/{id}", request, context, logger));

api.MapDelete("/products/{id:guid}", 
    async (Guid id, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
        await ApiProxyHelper.ProxyDeleteRequest(factory, $"/products/{id}", context, logger));

// Orders
api.MapGet(EndpointPaths.Orders, 
    async (IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
        await ApiProxyHelper.ProxyGetWithAuthRequest(factory, EndpointPaths.Orders, context, logger));

api.MapGet("/orders/{id:guid}", 
    async (Guid id, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
        await ApiProxyHelper.ProxyGetWithAuthRequest(factory, $"/orders/{id}", context, logger));

api.MapPost(EndpointPaths.Orders, 
    async (CreateOrderRequest request, IHttpClientFactory factory, ILogger<Program> logger) =>
        await ApiProxyHelper.ProxyPostRequest(factory, EndpointPaths.Orders, request, null, logger));

app.MapFallbackToFile("index.html");
app.MapHealthChecks("/health");
app.MapDefaultEndpoints();

await app.RunAsync();
