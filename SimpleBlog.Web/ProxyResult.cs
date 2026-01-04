namespace SimpleBlog.Web;

internal sealed class ProxyResult : IResult
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

    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = _statusCode;
        httpContext.Response.ContentType = _contentType;
        return httpContext.Response.WriteAsync(_body);
    }
}
