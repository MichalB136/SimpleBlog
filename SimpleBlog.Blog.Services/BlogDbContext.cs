using Microsoft.EntityFrameworkCore;

namespace SimpleBlog.Blog.Services;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {
    }

    public DbSet<PostEntity> Posts { get; set; } = null!;
    public DbSet<CommentEntity> Comments { get; set; } = null!;
    public DbSet<AboutMeEntity> AboutMe { get; set; } = null!;
    public DbSet<SiteSettingsEntity> SiteSettings { get; set; } = null!;
    public DbSet<TagEntity> Tags { get; set; } = null!;
    public DbSet<PostTagEntity> PostTags { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PostEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Author).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasMany(e => e.Comments).WithOne(c => c.Post!).HasForeignKey(c => c.PostId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CommentEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Author).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<AboutMeEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.UpdatedBy).IsRequired();
        });

        modelBuilder.Entity<SiteSettingsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Theme).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.UpdatedBy).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<TagEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(7);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<PostTagEntity>(entity =>
        {
            entity.HasKey(e => new { e.PostId, e.TagId });
            entity.HasOne(e => e.Post).WithMany(p => p.PostTags).HasForeignKey(e => e.PostId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Tag).WithMany(t => t.PostTags).HasForeignKey(e => e.TagId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
