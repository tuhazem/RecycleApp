using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Features.Admin.DTOs;

namespace RecyclingApp.Application.Features.Admin.Queries.GetDashboardStats;

public record GetAdminDashboardStatsQuery : IRequest<AdminDashboardStatsDto>;

public class GetAdminDashboardStatsQueryHandler : IRequestHandler<GetAdminDashboardStatsQuery, AdminDashboardStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetAdminDashboardStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardStatsDto> Handle(GetAdminDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        // 1. Basic Counts
        var totalUsers = await _context.Users.AsNoTracking().CountAsync(cancellationToken);
        var totalActiveProducts = await _context.Products.AsNoTracking().CountAsync(p => p.IsActive, cancellationToken);
        var totalActiveCategories = await _context.Categories.AsNoTracking().CountAsync(c => c.IsActive, cancellationToken);

        // 2. Stock Counts
        var outOfStockCount = await _context.Products.AsNoTracking().CountAsync(p => p.IsActive && p.StockQuantity == 0, cancellationToken);
        var lowStockCount = await _context.Products.AsNoTracking().CountAsync(p => p.IsActive && p.StockQuantity <= p.LowStockThreshold && p.StockQuantity > 0, cancellationToken);

        // 3. Revenue / Transactions
        var totalTransactions = await _context.RecyclingTransactions.AsNoTracking().CountAsync(cancellationToken);
        var totalRevenue = await _context.RecyclingTransactions.AsNoTracking().AnyAsync(cancellationToken)
            ? await _context.RecyclingTransactions.AsNoTracking().SumAsync(t => t.Amount, cancellationToken)
            : 0m;

        // 4. Top 5 Recycled Items (grouped by ProductId, Sum Quantity and Amount)
        var topItemsGrouped = await _context.RecyclingTransactions.AsNoTracking()
            .GroupBy(t => t.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalQuantity = g.Sum(t => t.Quantity),
                TotalAmount = g.Sum(t => t.Amount)
            })
            .OrderByDescending(g => g.TotalQuantity)
            .Take(5)
            .ToListAsync(cancellationToken);

        // Resolve product names
        var productIds = topItemsGrouped.Select(x => x.ProductId).ToList();
        var products = await _context.Products.AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

        var topRecycledItems = topItemsGrouped.Select(item => new TopRecycledItemDto(
            item.ProductId,
            products.TryGetValue(item.ProductId, out var name) ? name : "Unknown Product",
            item.TotalQuantity,
            item.TotalAmount
        )).ToList();

        return new AdminDashboardStatsDto(
            totalUsers,
            totalActiveProducts,
            totalActiveCategories,
            outOfStockCount,
            lowStockCount,
            totalRevenue,
            totalTransactions,
            topRecycledItems);
    }
}
