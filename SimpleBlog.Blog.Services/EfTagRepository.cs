using SimpleBlog.Common;
using SimpleBlog.Common.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleBlog.Blog.Services;

public sealed partial class EfTagRepository(
    BlogDbContext context,
    IOperationLogger operationLogger) : ITagRepository
{
    public async Task<IReadOnlyList<Tag>> GetAllAsync()
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetAllTags",
            async () =>
            {
                var entities = await context.Tags
                    .OrderBy(t => t.Name)
                    .ToListAsync();
                return entities.Select(MapToModel).ToList();
            },
            new { });
    }

    public async Task<Tag?> GetByIdAsync(Guid id)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetTagById",
            async () =>
            {
                var entity = await context.Tags.FirstOrDefaultAsync(t => t.Id == id);
                return entity is not null ? MapToModel(entity) : null;
            },
            new { TagId = id });
    }

    public async Task<Tag?> GetBySlugAsync(string slug)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetTagBySlug",
            async () =>
            {
                var entity = await context.Tags.FirstOrDefaultAsync(t => t.Slug == slug);
                return entity is not null ? MapToModel(entity) : null;
            },
            new { Slug = slug });
    }

    public async Task<Tag> CreateAsync(CreateTagRequest request)
    {
        return await operationLogger.LogRepositoryOperationAsync(
            "Create",
            "Tag",
            async () =>
            {
                var slug = GenerateSlug(request.Name);
                
                var entity = new TagEntity
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Slug = slug,
                    Color = request.Color,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                context.Tags.Add(entity);
                await context.SaveChangesAsync();
                return MapToModel(entity);
            },
            new { request.Name, request.Color });
    }

    public async Task<Tag?> UpdateAsync(Guid id, UpdateTagRequest request)
    {
        return await operationLogger.LogRepositoryOperationAsync(
            "Update",
            "Tag",
            async () =>
            {
                var entity = await context.Tags.FirstOrDefaultAsync(t => t.Id == id);
                if (entity is null)
                    return null;

                if (request.Name is not null)
                {
                    entity.Name = request.Name;
                    entity.Slug = GenerateSlug(request.Name);
                }
                
                if (request.Color is not null)
                    entity.Color = request.Color;

                await context.SaveChangesAsync();
                return MapToModel(entity);
            },
            new { TagId = id, request.Name, request.Color });
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await operationLogger.LogRepositoryOperationAsync(
            "Delete",
            "Tag",
            async () =>
            {
                var entity = await context.Tags.FirstOrDefaultAsync(t => t.Id == id);
                if (entity is null)
                    return false;

                context.Tags.Remove(entity);
                await context.SaveChangesAsync();
                return true;
            },
            new { TagId = id });
    }

    private static Tag MapToModel(TagEntity entity) =>
        new(
            entity.Id,
            entity.Name,
            entity.Slug,
            entity.Color,
            entity.CreatedAt
        );

    private static string GenerateSlug(string name)
    {
        // Convert to lowercase
        var slug = name.ToLowerInvariant();

        // Remove diacritics (accents)
        slug = RemoveDiacritics(slug);

        // Replace spaces and special characters with hyphens
        slug = SlugRegex().Replace(slug, "-");

        // Remove multiple consecutive hyphens
        slug = MultipleHyphensRegex().Replace(slug, "-");

        // Trim hyphens from start and end
        slug = slug.Trim('-');

        return slug;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex SlugRegex();

    [GeneratedRegex(@"[\s-]+")]
    private static partial Regex MultipleHyphensRegex();
}
