using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecyclingApp.Domain.Entities;

namespace RecyclingApp.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);
        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(1000);
        builder.Property(p => p.SKU)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(p => p.StockQuantity)
            .IsRequired();
        builder.Property(p => p.LowStockThreshold)
            .IsRequired();
        builder.Property(p => p.IsActive)
            .IsRequired();
        // Configure owned Money type for Price
        builder.OwnsOne(p => p.Price, money =>
        {
            money.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasColumnName("PriceAmount");
            money.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("PriceCurrency");
        });
        // Unique index on SKU
        builder.HasIndex(p => p.SKU).IsUnique();
        // Relationship with Category
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
