using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Features.Admin.DTOs;
using RecyclingApp.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        // 3. Revenue / Orders
        var totalOrders = await _context.Orders.AsNoTracking().CountAsync(cancellationToken);
        var totalRevenue = await _context.Orders.AsNoTracking()
            .Where(o => o.Status != OrderStatus.Cancelled)
            .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0m;

        // Fallback to legacy if no orders
        if (totalOrders == 0)
        {
            totalOrders = await _context.RecyclingTransactions.AsNoTracking().CountAsync(cancellationToken);
            totalRevenue = await _context.RecyclingTransactions.AsNoTracking()
                .Where(t => t.Status == "Completed")
                .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;
        }

        // 4. Top 5 Recycled/Ordered Items (grouped by ProductId from OrderItems)
        var topItemsGrouped = await _context.OrderItems.AsNoTracking()
            .Where(i => i.Order.Status != OrderStatus.Cancelled)
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalQuantity = g.Sum(i => i.Quantity),
                TotalAmount = g.Sum(i => i.UnitPrice * i.Quantity)
            })
            .OrderByDescending(g => g.TotalQuantity)
            .Take(5)
            .ToListAsync(cancellationToken);

        // Fallback to legacy transactions if empty
        if (topItemsGrouped.Count == 0)
        {
            topItemsGrouped = await _context.RecyclingTransactions.AsNoTracking()
                .Where(t => t.Status != "Cancelled")
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
        }

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
            totalOrders,
            topRecycledItems);
    }
}
