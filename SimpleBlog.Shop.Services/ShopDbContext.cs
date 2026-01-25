using Microsoft.EntityFrameworkCore;

namespace SimpleBlog.Shop.Services;

public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
    {
    }

    public DbSet<ProductEntity> Products { get; set; } = null!;
    public DbSet<ProductColorEntity> ProductColors { get; set; } = null!;
    public DbSet<OrderEntity> Orders { get; set; } = null!;
    public DbSet<OrderItemEntity> OrderItems { get; set; } = null!;
    public DbSet<TagEntity> Tags { get; set; } = null!;
    public DbSet<ProductTagEntity> ProductTags { get; set; } = null!;
    public DbSet<ProductViewEntity> ProductViews { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Price).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Category).IsRequired();
            entity.Property(e => e.Stock).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).IsRequired();
            entity.Property(e => e.CustomerEmail).IsRequired();
            entity.Property(e => e.CustomerPhone).IsRequired();
            entity.Property(e => e.ShippingAddress).IsRequired();
            entity.Property(e => e.ShippingCity).IsRequired();
            entity.Property(e => e.ShippingPostalCode).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasMany(e => e.Items).WithOne(i => i.Order!).HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItemEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired();
            entity.Property(e => e.Price).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();
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

        modelBuilder.Entity<ProductTagEntity>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.TagId });
            entity.HasOne(e => e.Product).WithMany(p => p.ProductTags).HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Tag).WithMany(t => t.ProductTags).HasForeignKey(e => e.TagId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductColorEntity>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.Color });
            entity.Property(e => e.Color).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(2083);
            entity.HasOne(e => e.Product).WithMany(p => p.ProductColors).HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.ProductId });
        });

        modelBuilder.Entity<ProductViewEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.ViewedAt).IsRequired();
            entity.HasIndex(e => e.ProductId);
        });
    }
}
