using System.Net.Http.Json;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddHttpClient("ApiService", client =>
{
    // "https+http" lets service discovery prefer HTTPS but fall back to HTTP during development.
    client.BaseAddress = new("https+http://apiservice");
});

// Health Checks
builder.Services.AddHealthChecks();

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

api.MapPost("/login", async (LoginRequest request, IHttpClientFactory factory, ILogger<Program> logger) =>
{
    var client = factory.CreateClient("ApiService");
    
    try
    {
        var response = await client.PostAsJsonAsync("/login", request);
        return await ToResult(response, logger, "POST /login");
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error during login - API connection failed");
        return Results.Problem("Unable to connect to API service");
    }
});

api.MapGet("/posts", async (IHttpClientFactory factory, ILogger<Program> logger) =>
{
    try
    {
        var client = factory.CreateClient("ApiService");
        var response = await client.GetAsync("/posts");
        
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("API returned {StatusCode} for GET /posts", response.StatusCode);
            return Results.StatusCode((int)response.StatusCode);
        }
        
        var posts = await response.Content.ReadFromJsonAsync<List<Post>>();
        return posts is not null ? Results.Ok(posts) : Results.Problem("Unable to load posts.");
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error fetching posts from API");
        return Results.Problem("Unable to connect to API service");
    }
});

api.MapGet("/posts/{id:guid}", async (Guid id, IHttpClientFactory factory, ILogger<Program> logger) =>
{
    try
    {
        var client = factory.CreateClient("ApiService");
        var response = await client.GetAsync($"/posts/{id}");
        
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Results.NotFound();
            
            logger.LogWarning("API returned {StatusCode} for GET /posts/{PostId}", response.StatusCode, id);
            return Results.StatusCode((int)response.StatusCode);
        }
        
        var post = await response.Content.ReadFromJsonAsync<Post>();
        return post is not null ? Results.Ok(post) : Results.NotFound();
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error fetching post {PostId} from API", id);
        return Results.Problem("Unable to connect to API service");
    }
});

api.MapPost("/posts", async (CreatePostRequest request, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
{
    var client = factory.CreateClient("ApiService");
    
    try
    {
        logger.LogInformation("Creating post with title: {Title}", request.Title);
        
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/posts")
        {
            Content = JsonContent.Create(request)
        };
        
        // Forward auth token if present
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader) && !string.IsNullOrEmpty(authHeader))
        {
            var tokenValue = authHeader[0]!;
            httpRequest.Headers.Add("Authorization", tokenValue);
            logger.LogInformation("Forwarding Authorization header: {AuthHeader}", tokenValue.Substring(0, Math.Min(30, tokenValue.Length)) + "...");
        }
        else
        {
            logger.LogWarning("No Authorization header present in POST /posts request");
        }
        
        var response = await client.SendAsync(httpRequest);
        logger.LogInformation("API responded with status: {StatusCode}", response.StatusCode);
        
        return await ToResult(response, logger, "POST /posts");
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error creating post - API connection failed");
        return Results.Problem("Unable to connect to API service");
    }
});

api.MapPut("/posts/{id:guid}", async (Guid id, UpdatePostRequest request, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
{
    var client = factory.CreateClient("ApiService");
    
    var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"/posts/{id}")
    {
        Content = JsonContent.Create(request)
    };
    
    // Forward auth token if present
    if (context.Request.Headers.TryGetValue("Authorization", out var authHeader) && !string.IsNullOrEmpty(authHeader))
    {
        httpRequest.Headers.Add("Authorization", authHeader[0]!);
    }
    
    try
    {
        var response = await client.SendAsync(httpRequest);
        return await ToResult(response, logger, $"PUT /posts/{id}");
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error updating post {PostId} - API connection failed", id);
        return Results.Problem("Unable to connect to API service");
    }
});

api.MapDelete("/posts/{id:guid}", async (Guid id, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
{
    var client = factory.CreateClient("ApiService");
    
    var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/posts/{id}");
    
    // Forward auth token if present
    if (context.Request.Headers.TryGetValue("Authorization", out var authHeader) && !string.IsNullOrEmpty(authHeader))
    {
        httpRequest.Headers.Add("Authorization", authHeader[0]!);
    }
    
    try
    {
        var response = await client.SendAsync(httpRequest);
        return await ToResult(response, logger, $"DELETE /posts/{id}");
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error deleting post {PostId} - API connection failed", id);
        return Results.Problem("Unable to connect to API service");
    }
});

api.MapGet("/posts/{id:guid}/comments", async (Guid id, IHttpClientFactory factory, ILogger<Program> logger) =>
{
    try
    {
        var client = factory.CreateClient("ApiService");
        var response = await client.GetAsync($"/posts/{id}/comments");
        
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Results.NotFound();
            
            logger.LogWarning("API returned {StatusCode} for GET /posts/{PostId}/comments", response.StatusCode, id);
            return Results.StatusCode((int)response.StatusCode);
        }
        
        var comments = await response.Content.ReadFromJsonAsync<List<Comment>>();
        return comments is not null ? Results.Ok(comments) : Results.NotFound();
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error fetching comments for post {PostId} from API", id);
        return Results.Problem("Unable to connect to API service");
    }
});

api.MapPost("/posts/{id:guid}/comments", async (Guid id, CreateCommentRequest request, IHttpClientFactory factory, ILogger<Program> logger) =>
{
    var client = factory.CreateClient("ApiService");
    
    try
    {
        var response = await client.PostAsJsonAsync($"/posts/{id}/comments", request);
        return await ToResult(response, logger, $"POST /posts/{id}/comments");
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error creating comment for post {PostId} - API connection failed", id);
        return Results.Problem("Unable to connect to API service");
    }
});

// Products endpoints
api.MapGet("/products", async (IHttpClientFactory factory, ILogger<Program> logger) =>
{
    try
    {
        var client = factory.CreateClient("ApiService");
        var response = await client.GetAsync("/products");
        
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("API returned {StatusCode} for GET /products", response.StatusCode);
            return Results.StatusCode((int)response.StatusCode);
        }
        
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        return products is not null ? Results.Ok(products) : Results.Problem("Unable to load products.");
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error fetching products from API");
        return Results.Problem("Unable to connect to API service");
    }
});

api.MapGet("/products/{id:guid}", async (Guid id, IHttpClientFactory factory, ILogger<Program> logger) =>
{
    try
    {
        var client = factory.CreateClient("ApiService");
        var response = await client.GetAsync($"/products/{id}");
        
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Results.NotFound();
            
            logger.LogWarning("API returned {StatusCode} for GET /products/{ProductId}", response.StatusCode, id);
            return Results.StatusCode((int)response.StatusCode);
        }
        
        var product = await response.Content.ReadFromJsonAsync<Product>();
        return product is not null ? Results.Ok(product) : Results.NotFound();
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error fetching product {ProductId} from API", id);
        return Results.Problem("Unable to connect to API service");
    }
});

api.MapPost("/products", async (CreateProductRequest request, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
{
    var client = factory.CreateClient("ApiService");
    
    try
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/products")
        {
            Content = JsonContent.Create(request)
        };
        
        // Forward auth token if present
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader) && !string.IsNullOrEmpty(authHeader))
        {
            httpRequest.Headers.Add("Authorization", authHeader[0]!);
        }
        
        var response = await client.SendAsync(httpRequest);
        return await ToResult(response, logger, "POST /products");
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error creating product - API connection failed");
        return Results.Problem("Unable to connect to API service");
    }
});

api.MapPut("/products/{id:guid}", async (Guid id, UpdateProductRequest request, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
{
    var client = factory.CreateClient("ApiService");
    
    var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"/products/{id}")
    {
        Content = JsonContent.Create(request)
    };
    
    // Forward auth token if present
    if (context.Request.Headers.TryGetValue("Authorization", out var authHeader) && !string.IsNullOrEmpty(authHeader))
    {
        httpRequest.Headers.Add("Authorization", authHeader[0]!);
    }
    
    try
    {
        var response = await client.SendAsync(httpRequest);
        return await ToResult(response, logger, $"PUT /products/{id}");
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error updating product {ProductId} - API connection failed", id);
        return Results.Problem("Unable to connect to API service");
    }
});

api.MapDelete("/products/{id:guid}", async (Guid id, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
{
    var client = factory.CreateClient("ApiService");
    
    var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/products/{id}");
    
    // Forward auth token if present
    if (context.Request.Headers.TryGetValue("Authorization", out var authHeader) && !string.IsNullOrEmpty(authHeader))
    {
        httpRequest.Headers.Add("Authorization", authHeader[0]!);
    }
    
    try
    {
        var response = await client.SendAsync(httpRequest);
        return await ToResult(response, logger, $"DELETE /products/{id}");
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error deleting product {ProductId} - API connection failed", id);
        return Results.Problem("Unable to connect to API service");
    }
});

// Orders endpoints
api.MapGet("/orders", async (IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
{
    var client = factory.CreateClient("ApiService");
    
    var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/orders");
    
    // Forward auth token if present
    if (context.Request.Headers.TryGetValue("Authorization", out var authHeader) && !string.IsNullOrEmpty(authHeader))
    {
        httpRequest.Headers.Add("Authorization", authHeader[0]!);
    }
    
    try
    {
        var response = await client.SendAsync(httpRequest);
        return await ToResult(response, logger, "GET /orders");
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error fetching orders - API connection failed");
        return Results.Problem("Unable to connect to API service");
    }
});

api.MapGet("/orders/{id:guid}", async (Guid id, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
{
    var client = factory.CreateClient("ApiService");
    
    var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/orders/{id}");
    
    // Forward auth token if present
    if (context.Request.Headers.TryGetValue("Authorization", out var authHeader) && !string.IsNullOrEmpty(authHeader))
    {
        httpRequest.Headers.Add("Authorization", authHeader[0]!);
    }
    
    try
    {
        var response = await client.SendAsync(httpRequest);
        return await ToResult(response, logger, $"GET /orders/{id}");
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error fetching order {OrderId} - API connection failed", id);
        return Results.Problem("Unable to connect to API service");
    }
});

api.MapPost("/orders", async (CreateOrderRequest request, IHttpClientFactory factory, ILogger<Program> logger) =>
{
    var client = factory.CreateClient("ApiService");
    
    try
    {
        var response = await client.PostAsJsonAsync("/orders", request);
        return await ToResult(response, logger, "POST /orders");
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error creating order - API connection failed");
        return Results.Problem("Unable to connect to API service");
    }
});

app.MapFallbackToFile("index.html");

app.MapHealthChecks("/health");

app.MapDefaultEndpoints();

app.Run();

static async Task<IResult> ToResult(HttpResponseMessage response, ILogger logger, string endpoint)
{
    var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
    var body = await response.Content.ReadAsStringAsync();
    
    if (!response.IsSuccessStatusCode)
    {
        logger.LogWarning("API endpoint {Endpoint} returned {StatusCode}: {Body}", endpoint, response.StatusCode, body);
    }
    else
    {
        logger.LogInformation("API endpoint {Endpoint} returned {StatusCode}", endpoint, response.StatusCode);
    }
    
    // For empty body responses (like 201 Created with no content), return empty JSON object
    if (string.IsNullOrWhiteSpace(body) && response.IsSuccessStatusCode)
    {
        body = "{}";
    }
    
    return new ProxyResult(body, contentType, (int)response.StatusCode);
}

record Post(Guid Id, string Title, string Content, string Author, DateTimeOffset CreatedAt, IReadOnlyList<Comment> Comments, string? ImageUrl = null);

record Comment(Guid Id, Guid PostId, string Author, string Content, DateTimeOffset CreatedAt);

record CreatePostRequest(string Title, string Content, string Author, string? ImageUrl = null);

record UpdatePostRequest(string Title, string Content, string Author, string? ImageUrl = null);

record CreateCommentRequest(string Content, string Author);

record LoginRequest(string Username, string Password);

record Product(Guid Id, string Name, string Description, decimal Price, string? ImageUrl, string Category, int Stock, DateTimeOffset CreatedAt);

record CreateProductRequest(string Name, string Description, decimal Price, string? ImageUrl, string Category, int Stock);

record UpdateProductRequest(string Name, string Description, decimal Price, string? ImageUrl, string Category, int Stock);

record Order(Guid Id, string CustomerName, string CustomerEmail, string CustomerPhone, string ShippingAddress, string ShippingCity, string ShippingPostalCode, decimal TotalAmount, DateTimeOffset CreatedAt, IReadOnlyList<OrderItem> Items);

record OrderItem(Guid Id, string ProductName, decimal Price, int Quantity);

record CreateOrderRequest(string CustomerName, string CustomerEmail, string CustomerPhone, string ShippingAddress, string ShippingCity, string ShippingPostalCode, List<CreateOrderItemRequest> Items);

record CreateOrderItemRequest(Guid ProductId, string ProductName, decimal Price, int Quantity);

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
