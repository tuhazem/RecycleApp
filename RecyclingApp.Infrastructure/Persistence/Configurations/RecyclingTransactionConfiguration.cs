using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecyclingApp.Domain.Entities;

namespace RecyclingApp.Infrastructure.Persistence.Configurations;

public class RecyclingTransactionConfiguration : IEntityTypeConfiguration<RecyclingTransaction>
{
    public void Configure(EntityTypeBuilder<RecyclingTransaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.UserId)
            .IsRequired()
            .HasMaxLength(450); // matches Identity string key length
        builder.Property(t => t.Quantity)
            .IsRequired();
        builder.Property(t => t.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
        builder.Property(t => t.TransactionDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
        // Relationships
        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.Product)
            .WithMany()
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
