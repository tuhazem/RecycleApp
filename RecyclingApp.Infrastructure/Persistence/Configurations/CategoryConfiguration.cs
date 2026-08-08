using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecyclingApp.Domain.Entities;

namespace RecyclingApp.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(500);
        builder.Property(c => c.IsActive)
            .IsRequired();
        // Configure relationship with Products (one-to-many)
        builder.HasMany(c => c.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
