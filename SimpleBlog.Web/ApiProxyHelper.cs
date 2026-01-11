namespace SimpleBlog.Web;

internal static class ApiProxyHelper
{
    public static async Task<IResult> ProxyGetRequest(
        IHttpClientFactory factory,
        string path,
        ILogger logger)
    {
        try
        {
            var client = factory.CreateClient(ApiConstants.ClientName);
            var response = await client.GetAsync(path);
            return await ToResult(response, logger, $"GET {path}");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error fetching from API: {Path}", path);
            return Results.Problem(ApiConstants.ErrorUnableToConnect);
        }
    }

    public static async Task<IResult> ProxyPostRequest<T>(
        IHttpClientFactory factory,
        string path,
        T request,
        HttpContext? context,
        ILogger logger)
    {
        try
        {
            var client = factory.CreateClient(ApiConstants.ClientName);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = JsonContent.Create(request)
            };

            ForwardAuthorizationHeader(context, httpRequest);

            var response = await client.SendAsync(httpRequest);
            return await ToResult(response, logger, $"POST {path}");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error posting to API: {Path}", path);
            return Results.Problem(ApiConstants.ErrorUnableToConnect);
        }
    }

    public static async Task<IResult> ProxyPutRequest<T>(
        IHttpClientFactory factory,
        string path,
        T request,
        HttpContext? context,
        ILogger logger)
    {
        try
        {
            var client = factory.CreateClient(ApiConstants.ClientName);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, path)
            {
                Content = JsonContent.Create(request)
            };

            ForwardAuthorizationHeader(context, httpRequest);

            var response = await client.SendAsync(httpRequest);
            return await ToResult(response, logger, $"PUT {path}");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error putting to API: {Path}", path);
            return Results.Problem(ApiConstants.ErrorUnableToConnect);
        }
    }

    public static async Task<IResult> ProxyPutRequestWithoutBody(
        IHttpClientFactory factory,
        string path,
        HttpContext? context,
        ILogger logger)
    {
        try
        {
            var client = factory.CreateClient(ApiConstants.ClientName);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, path);

            ForwardAuthorizationHeader(context, httpRequest);

            var response = await client.SendAsync(httpRequest);
            return await ToResult(response, logger, $"PUT {path}");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error putting to API: {Path}", path);
            return Results.Problem(ApiConstants.ErrorUnableToConnect);
        }
    }

    public static async Task<IResult> ProxyDeleteRequest(
        IHttpClientFactory factory,
        string path,
        HttpContext? context,
        ILogger logger)
    {
        try
        {
            var client = factory.CreateClient(ApiConstants.ClientName);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, path);

            ForwardAuthorizationHeader(context, httpRequest);

            var response = await client.SendAsync(httpRequest);
            return await ToResult(response, logger, $"DELETE {path}");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error deleting from API: {Path}", path);
            return Results.Problem(ApiConstants.ErrorUnableToConnect);
        }
    }

    public static async Task<IResult> ProxyGetWithAuthRequest(
        IHttpClientFactory factory,
        string path,
        HttpContext context,
        ILogger logger)
    {
        try
        {
            var client = factory.CreateClient(ApiConstants.ClientName);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

            ForwardAuthorizationHeader(context, httpRequest);

            var response = await client.SendAsync(httpRequest);
            return await ToResult(response, logger, $"GET {path}");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error fetching from API: {Path}", path);
            return Results.Problem(ApiConstants.ErrorUnableToConnect);
        }
    }

    private static void ForwardAuthorizationHeader(HttpContext? context, HttpRequestMessage request)
    {
        if (context is not null && 
            context.Request.Headers.TryGetValue(ApiConstants.AuthorizationHeader, out var authHeader) && 
            !string.IsNullOrEmpty(authHeader))
        {
            request.Headers.Add(ApiConstants.AuthorizationHeader, authHeader[0]!);
        }
    }

    private static async Task<IResult> ToResult(HttpResponseMessage response, ILogger logger, string endpoint)
    {
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("API endpoint {Endpoint} returned {StatusCode}", endpoint, response.StatusCode);
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
}
