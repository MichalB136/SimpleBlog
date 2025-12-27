using Microsoft.EntityFrameworkCore;

namespace SimpleBlog.Shop.Services;

public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
    {
    }

    public DbSet<ProductEntity> Products { get; set; } = null!;
    public DbSet<OrderEntity> Orders { get; set; } = null!;
    public DbSet<OrderItemEntity> OrderItems { get; set; } = null!;

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
    }
}
