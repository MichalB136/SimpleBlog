using Microsoft.EntityFrameworkCore;
using SimpleBlog.Common;
using SimpleBlog.Common.Logging;

namespace SimpleBlog.Blog.Services;

public sealed class EfAboutMeRepository(
    BlogDbContext context,
    IOperationLogger operationLogger) : IAboutMeRepository
{
    public async Task<AboutMe?> GetAsync()
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetAboutMe",
            async () =>
            {
                var entity = await context.AboutMe.FirstOrDefaultAsync();
                return entity is not null ? MapToModel(entity) : null;
            });
    }

    public async Task<AboutMe> UpdateAsync(UpdateAboutMeRequest request, string updatedBy)
    {
        return await operationLogger.LogRepositoryOperationAsync(
            "Update",
            nameof(AboutMeEntity),
            async () =>
            {
                var entity = await context.AboutMe.FirstOrDefaultAsync();
                
                if (entity is null)
                {
                    // Create new AboutMe entry
                    entity = new AboutMeEntity
                    {
                        Id = Guid.NewGuid(),
                        Content = request.Content,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        UpdatedBy = updatedBy
                    };
                    context.AboutMe.Add(entity);
                }
                else
                {
                    // Update existing entry
                    entity.Content = request.Content;
                    entity.UpdatedAt = DateTimeOffset.UtcNow;
                    entity.UpdatedBy = updatedBy;
                }

                await context.SaveChangesAsync();
                return MapToModel(entity);
            },
            new { UpdatedBy = updatedBy });
    }

    private static AboutMe MapToModel(AboutMeEntity entity) =>
        new(
            entity.Id,
            entity.Content,
            entity.UpdatedAt,
            entity.UpdatedBy);
}
