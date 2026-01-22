using SimpleBlog.Common;
using SimpleBlog.Common.Api.Extensions;
using SimpleBlog.ApiService;

namespace SimpleBlog.ApiService.Endpoints;

public static class TagEndpoints
{
    public static void MapTagEndpoints(this IEndpointRouteBuilder app)
    {
        var tags = app.MapGroup("/tags").WithTags("Tags");

        // GET /tags - Get all tags
        tags.MapGet("", async (ITagRepository repository) =>
        {
            var allTags = await repository.GetAllAsync();
            return Results.Ok(allTags);
        })
        .WithName("GetAllTags")
        .Produces<IReadOnlyList<Tag>>();

        // GET /tags/{id} - Get tag by ID
        tags.MapGet("{id:guid}", async (Guid id, ITagRepository repository) =>
        {
            var tag = await repository.GetByIdAsync(id);
            return tag is not null ? Results.Ok(tag) : Results.NotFound();
        })
        .WithName("GetTagById")
        .Produces<Tag>()
        .Produces(StatusCodes.Status404NotFound);

        // GET /tags/by-slug/{slug} - Get tag by slug
        tags.MapGet("by-slug/{slug}", async (string slug, ITagRepository repository) =>
        {
            var tag = await repository.GetBySlugAsync(slug);
            return tag is not null ? Results.Ok(tag) : Results.NotFound();
        })
        .WithName("GetTagBySlug")
        .Produces<Tag>()
        .Produces(StatusCodes.Status404NotFound);

        // GET /tags/{id}/posts - Get posts by tag ID
        tags.MapGet("{id:guid}/posts", async (Guid id, ITagRepository repository, IPostRepository postRepository) =>
        {
            var tag = await repository.GetByIdAsync(id);
            if (tag is null)
                return Results.NotFound();

            var posts = await postRepository.GetByTagAsync(id);
            return Results.Ok(new { tag, posts });
        })
        .WithName("GetPostsByTag")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST /tags - Create new tag
        tags.MapPost("", async (CreateTagRequest request, ITagRepository repository, HttpContext context) =>
        {
            if (!context.User.IsInRole(SeedDataConstants.AdminRole))
                return Results.Forbid();

            var tag = await repository.CreateAsync(request);
            return Results.CreatedAtRoute("GetTagById", new { id = tag.Id }, tag);
        })
        .WithName("CreateTag")
        .RequireAuthorization()
        .Produces<Tag>(StatusCodes.Status201Created);

        // PUT /tags/{id} - Update tag
        tags.MapPut("{id:guid}", async (Guid id, UpdateTagRequest request, ITagRepository repository, HttpContext context) =>
        {
            if (!context.User.IsInRole(SeedDataConstants.AdminRole))
                return Results.Forbid();

            var tag = await repository.UpdateAsync(id, request);
            return tag is not null ? Results.Ok(tag) : Results.NotFound();
        })
        .WithName("UpdateTag")
        .RequireAuthorization()
        .Produces<Tag>()
        .Produces(StatusCodes.Status404NotFound);

        // DELETE /tags/{id} - Delete tag
        tags.MapDelete("{id:guid}", async (Guid id, ITagRepository repository, HttpContext context) =>
        {
            if (!context.User.IsInRole(SeedDataConstants.AdminRole))
                return Results.Forbid();

            var success = await repository.DeleteAsync(id);
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteTag")
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}

