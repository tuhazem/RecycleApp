using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecyclingApp.Domain.Entities;

namespace RecyclingApp.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(100);

        // Map Address Value Object as an Owned Type
        builder.OwnsOne(u => u.Address, a =>
        {
            a.Property(ad => ad.Street)
                .HasColumnName("Street")
                .IsRequired()
                .HasMaxLength(150);

            a.Property(ad => ad.City)
                .HasColumnName("City")
                .IsRequired()
                .HasMaxLength(100);

            a.Property(ad => ad.BuildingNumber)
                .HasColumnName("BuildingNumber")
                .IsRequired()
                .HasMaxLength(20);
        });
    }
}
