namespace SimpleBlog.Common.Models;

/// <summary>
/// Global site settings that can be customized by administrators
/// </summary>
public sealed record SiteSettings(
    Guid Id,
    string Theme,
    string? LogoUrl,
    string? ContactText,
    DateTimeOffset UpdatedAt,
    string UpdatedBy
);

/// <summary>
/// Request model for updating site settings
/// </summary>
public sealed record UpdateSiteSettingsRequest(
    string Theme,
    string? ContactText
);

/// <summary>
/// Available theme definitions
/// </summary>
public static class ThemeNames
{
    public const string Light = "light";
    public const string Dark = "dark";
    public const string Ocean = "ocean";
    public const string Forest = "forest";
    public const string Sunset = "sunset";
    public const string Purple = "purple";
    public const string Marjan = "marjan";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Light,
        Dark,
        Ocean,
        Forest,
        Sunset,
        Purple,
        Marjan
    };
}
