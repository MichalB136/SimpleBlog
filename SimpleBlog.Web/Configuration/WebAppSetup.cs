namespace SimpleBlog.Web.Configuration;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;
using System.Net.Http.Headers;
using SimpleBlog.Common.Models;

public static class WebAppSetup
{
    public static void ConfigureClientServing(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using var viteHttpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5173"),
                Timeout = TimeSpan.FromSeconds(30)
            };

            app.Use(async (context, next) =>
            {
                // Skip API and health check routes
                if (context.Request.Path.StartsWithSegments("/api") ||
                    context.Request.Path.StartsWithSegments("/health") ||
                    context.Request.Path.StartsWithSegments("/.well-known"))
                {
                    await next();
                    return;
                }

                try
                {
                    var requestPath = context.Request.Path.Value ?? "/";
                    var requestUrl = $"{requestPath}{context.Request.QueryString}";

                    using var proxyRequest = new HttpRequestMessage(
                        new HttpMethod(context.Request.Method),
                        requestUrl);

                    // Copy headers (excluding Host header)
                    foreach (var header in context.Request.Headers
                        .Where(h => !h.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)))
                    {
                        proxyRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }

                    // Copy body if present
                    if (context.Request.ContentLength > 0)
                    {
                        proxyRequest.Content = new StreamContent(context.Request.Body);
                        if (context.Request.ContentType != null)
                        {
                            proxyRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(context.Request.ContentType);
                        }
                    }

                    var response = await viteHttpClient.SendAsync(proxyRequest);

                    context.Response.StatusCode = (int)response.StatusCode;

                    foreach (var header in response.Content.Headers)
                    {
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                    }

                    foreach (var header in response.Headers)
                    {
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                    }

                    await response.Content.CopyToAsync(context.Response.Body);
                }
                catch
                {
                    // Fallback to next middleware on error
                    await next();
                }
            });
        }
        else
        {
            // Production: Serve static files from dist/
            var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
            var distPath = Path.Combine(wwwrootPath, "dist");

            // 1) Serve root static files (favicon, etc.)
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(wwwrootPath),
                RequestPath = ""
            });

            // 2) Serve built assets from /assets mapped to dist/assets
            var assetsPath = Path.Combine(distPath, "assets");
            if (Directory.Exists(assetsPath))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(assetsPath),
                    RequestPath = "/assets"
                });
            }

            // SPA fallback to Vite-built index
            app.MapFallbackToFile("dist/index.html");
        }
    }

    public static void MapApiEndpoints(WebApplication app)
    {
        var api = app.MapGroup("/api");

        // Authentication
        api.MapPost(EndpointPaths.Login,
            async (LoginRequest request, IHttpClientFactory factory, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyPostRequest(factory, EndpointPaths.Login, request, null, logger));

        api.MapPost(EndpointPaths.Register,
            async (RegisterRequest request, IHttpClientFactory factory, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyPostRequest(factory, EndpointPaths.Register, request, null, logger));

        // Posts
        api.MapGet(EndpointPaths.Posts,
            async (IHttpClientFactory factory, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyGetRequest(factory, EndpointPaths.Posts, logger));

        api.MapGet("/posts/{id:guid}",
            async (Guid id, IHttpClientFactory factory, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyGetRequest(factory, $"/posts/{id}", logger));

        api.MapPost(EndpointPaths.Posts,
            async (IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
            {
                if (context.Request.HasFormContentType)
                {
                    return await ApiProxyHelper.ProxyFormDataRequest(factory, EndpointPaths.Posts, context, logger);
                }
                else
                {
                    var request = await context.Request.ReadFromJsonAsync<CreatePostRequest>();
                    return await ApiProxyHelper.ProxyPostRequest(factory, EndpointPaths.Posts, request!, context, logger);
                }
            });

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

        api.MapPut("/posts/{id:guid}/pin",
            async (Guid id, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyPutRequestWithoutBody(factory, $"/posts/{id}/pin", context, logger));

        api.MapPut("/posts/{id:guid}/unpin",
            async (Guid id, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyPutRequestWithoutBody(factory, $"/posts/{id}/unpin", context, logger));

        api.MapPost("/posts/{id:guid}/images",
            async (Guid id, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyFormDataRequest(factory, $"/posts/{id}/images", context, logger));

        api.MapDelete("/posts/{id:guid}/images",
            async (Guid id, string imageUrl, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyDeleteRequest(factory, $"/posts/{id}/images?imageUrl={Uri.EscapeDataString(imageUrl)}", context, logger));

        api.MapPut("/posts/{id:guid}/tags",
            async (Guid id, AssignTagsRequest request, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyPutRequest(factory, $"/posts/{id}/tags", request, context, logger));

        // Tags
        api.MapGet(EndpointPaths.Tags,
            async (IHttpClientFactory factory, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyGetRequest(factory, EndpointPaths.Tags, logger));

        api.MapGet("/tags/{id:guid}",
            async (Guid id, IHttpClientFactory factory, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyGetRequest(factory, $"/tags/{id}", logger));

        api.MapGet("/tags/by-slug/{slug}",
            async (string slug, IHttpClientFactory factory, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyGetRequest(factory, $"/tags/by-slug/{slug}", logger));

        api.MapPost(EndpointPaths.Tags,
            async (CreateTagRequest request, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyPostRequest(factory, EndpointPaths.Tags, request, context, logger));

        api.MapPut("/tags/{id:guid}",
            async (Guid id, UpdateTagRequest request, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyPutRequest(factory, $"/tags/{id}", request, context, logger));

        api.MapDelete("/tags/{id:guid}",
            async (Guid id, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyDeleteRequest(factory, $"/tags/{id}", context, logger));

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

        api.MapPut("/products/{id:guid}/tags",
            async (Guid id, AssignTagsRequest request, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyPutRequest(factory, $"/products/{id}/tags", request, context, logger));

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

        // AboutMe
        api.MapGet(EndpointPaths.AboutMe,
            async (IHttpClientFactory factory, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyGetRequest(factory, EndpointPaths.AboutMe, logger));

        api.MapPut(EndpointPaths.AboutMe,
            async (UpdateAboutMeRequest request, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyPutRequest(factory, EndpointPaths.AboutMe, request, context, logger));

        // Site Settings
        api.MapGet(EndpointPaths.SiteSettings,
            async (IHttpClientFactory factory, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyGetRequest(factory, EndpointPaths.SiteSettings, logger));

        api.MapPut(EndpointPaths.SiteSettings,
            async (UpdateSiteSettingsRequest request, IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyPutRequest(factory, EndpointPaths.SiteSettings, request, context, logger));

        api.MapGet($"{EndpointPaths.SiteSettings}/themes",
            async (IHttpClientFactory factory, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyGetRequest(factory, $"{EndpointPaths.SiteSettings}/themes", logger));

        api.MapPost(EndpointPaths.SiteSettingsLogo,
            async (IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyFormDataRequest(factory, EndpointPaths.SiteSettingsLogo, context, logger));

        api.MapDelete(EndpointPaths.SiteSettingsLogo,
            async (IHttpClientFactory factory, HttpContext context, ILogger<Program> logger) =>
                await ApiProxyHelper.ProxyDeleteRequest(factory, EndpointPaths.SiteSettingsLogo, context, logger));
    }
}
