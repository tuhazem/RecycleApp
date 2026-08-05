using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Domain.Entities;
using System.Reflection;

namespace RecyclingApp.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core Database Context extending IdentityDbContext for user authentication.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Must call base.OnModelCreating to configure Identity tables/keys
        base.OnModelCreating(builder);

        // Apply all IEntityTypeConfiguration configurations in the Infrastructure assembly
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
