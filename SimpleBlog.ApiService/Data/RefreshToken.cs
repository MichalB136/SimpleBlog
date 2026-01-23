using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleBlog.ApiService.Data;

public sealed class RefreshToken
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public DateTime ExpiresUtc { get; set; }

    [Required]
    public DateTime CreatedUtc { get; set; }

    public DateTime? RevokedUtc { get; set; }

    public bool IsActive => RevokedUtc is null && ExpiresUtc > DateTime.UtcNow;
}
