using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Domain.Entities;
using System.Reflection;

namespace RecyclingApp.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core Database Context extending IdentityDbContext for user authentication.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for domain entities
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<RecyclingTransaction> RecyclingTransactions { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }



    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Must call base.OnModelCreating to configure Identity tables/keys
        base.OnModelCreating(builder);

        // Apply all IEntityTypeConfiguration configurations in the Infrastructure assembly
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
