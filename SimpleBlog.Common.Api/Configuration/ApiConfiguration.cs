namespace SimpleBlog.Common.Api.Configuration;

/// <summary>
/// Endpoint configuration loaded from appsettings.shared.json.
/// Provides strongly-typed access to API endpoint paths.
/// </summary>
public class EndpointConfiguration
{
    public string Login { get; set; } = "/login";
    public string Register { get; set; } = "/register";
    public string Refresh { get; set; } = "/refresh";
    public string Revoke { get; set; } = "/revoke";
    public PostsEndpoints Posts { get; set; } = new();
    public AboutMeEndpoints AboutMe { get; set; } = new();
    public ProductsEndpoints Products { get; set; } = new();
    public OrdersEndpoints Orders { get; set; } = new();
    public string Health { get; set; } = "/health";
    public string OpenApi { get; set; } = "/openapi/v1.json";
}

/// <summary>
/// Posts endpoint path configuration.
/// </summary>
public class PostsEndpoints
{
    public string Base { get; set; } = "/posts";
    public string GetAll { get; set; } = "";
    public string GetById { get; set; } = "/{id:guid}";
    public string Create { get; set; } = "";
    public string Update { get; set; } = "/{id:guid}";
    public string Delete { get; set; } = "/{id:guid}";
    public string GetComments { get; set; } = "/{id:guid}/comments";
    public string AddComment { get; set; } = "/{id:guid}/comments";
}

/// <summary>
/// AboutMe endpoint path configuration.
/// </summary>
public class AboutMeEndpoints
{
    public string Base { get; set; } = "/aboutme";
    public string Get { get; set; } = "";
    public string Update { get; set; } = "";
}

/// <summary>
/// Products endpoint path configuration.
/// </summary>
public class ProductsEndpoints
{
    public string Base { get; set; } = "/products";
    public string GetAll { get; set; } = "";
    public string GetById { get; set; } = "/{id:guid}";
    public string Create { get; set; } = "";
    public string Update { get; set; } = "/{id:guid}";
    public string Delete { get; set; } = "/{id:guid}";
}

/// <summary>
/// Orders endpoint path configuration.
/// </summary>
public class OrdersEndpoints
{
    public string Base { get; set; } = "/orders";
    public string GetAll { get; set; } = "";
    public string GetById { get; set; } = "/{id:guid}";
    public string Create { get; set; } = "";
}

/// <summary>
/// Authorization configuration loaded from appsettings.shared.json.
/// Provides settings for role-based access control and token expiration.
/// </summary>
public class AuthorizationConfiguration
{
    public bool RequireAdminForPostCreate { get; set; } = true;
    public bool RequireAdminForPostUpdate { get; set; } = true;
    public bool RequireAdminForPostDelete { get; set; } = true;
    public bool RequireAdminForProductCreate { get; set; } = true;
    public bool RequireAdminForProductUpdate { get; set; } = true;
    public bool RequireAdminForProductDelete { get; set; } = true;
    public bool RequireAdminForOrderView { get; set; } = true;
    public int TokenExpirationHours { get; set; } = 8;
    public int RefreshTokenExpirationDays { get; set; } = 30;
}
