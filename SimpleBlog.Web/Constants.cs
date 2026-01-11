namespace SimpleBlog.Web;

internal static class ApiConstants
{
    public const string ClientName = "ApiService";
    public const string ErrorUnableToConnect = "Unable to connect to API service";
    public const string AuthorizationHeader = "Authorization";
}

internal static class EndpointPaths
{
    // Authentication
    public const string Login = "/login";
    public const string Register = "/register";
    
    // Posts
    public const string Posts = "/posts";
    public const string PostsById = "/posts/{0}";
    public const string PostComments = "/posts/{0}/comments";
    
    // Products
    public const string Products = "/products";
    public const string ProductsById = "/products/{0}";
    
    // Orders
    public const string Orders = "/orders";
    public const string OrdersById = "/orders/{0}";
}
