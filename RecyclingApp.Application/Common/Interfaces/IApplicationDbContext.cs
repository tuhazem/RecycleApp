using Microsoft.EntityFrameworkCore;
using RecyclingApp.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<RecyclingTransaction> RecyclingTransactions { get; }
    DbSet<ApplicationUser> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
