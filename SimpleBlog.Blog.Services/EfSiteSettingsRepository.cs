using Microsoft.EntityFrameworkCore;
using SimpleBlog.Common.Interfaces;
using SimpleBlog.Common.Models;

namespace SimpleBlog.Blog.Services;

public sealed class EfSiteSettingsRepository : ISiteSettingsRepository
{
    private readonly BlogDbContext _context;

    public EfSiteSettingsRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<SiteSettings?> GetAsync(CancellationToken ct = default)
    {
        var entity = await _context.SiteSettings
            .OrderByDescending(s => s.UpdatedAt)
            .FirstOrDefaultAsync(ct);

        return entity is null 
            ? null 
            : new SiteSettings(entity.Id, entity.Theme, entity.LogoUrl, entity.ContactText, entity.UpdatedAt, entity.UpdatedBy);
    }

    public async Task<SiteSettings> UpdateAsync(string theme, string? contactText, string updatedBy, CancellationToken ct = default)
    {
        var existing = await _context.SiteSettings.FirstOrDefaultAsync(ct);
        
        if (existing is not null)
        {
            existing.Theme = theme;
            if (contactText is not null)
            {
                existing.ContactText = contactText;
            }
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            existing.UpdatedBy = updatedBy;
        }
        else
        {
            existing = new SiteSettingsEntity
            {
                Id = Guid.NewGuid(),
                Theme = theme,
                ContactText = contactText,
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = updatedBy
            };
            _context.SiteSettings.Add(existing);
        }

        await _context.SaveChangesAsync(ct);
        
        return new SiteSettings(existing.Id, existing.Theme, existing.LogoUrl, existing.ContactText, existing.UpdatedAt, existing.UpdatedBy);
    }

    public async Task<SiteSettings> UpdateLogoAsync(string? logoUrl, string updatedBy, CancellationToken ct = default)
    {
        var existing = await _context.SiteSettings.FirstOrDefaultAsync(ct);
        
        if (existing is not null)
        {
            existing.LogoUrl = logoUrl;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            existing.UpdatedBy = updatedBy;
        }
        else
        {
            existing = new SiteSettingsEntity
            {
                Id = Guid.NewGuid(),
                Theme = "light",
                LogoUrl = logoUrl,
                ContactText = null,
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = updatedBy
            };
            _context.SiteSettings.Add(existing);
        }

        await _context.SaveChangesAsync(ct);
        
        return new SiteSettings(existing.Id, existing.Theme, existing.LogoUrl, existing.ContactText, existing.UpdatedAt, existing.UpdatedBy);
    }
}
