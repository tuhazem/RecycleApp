using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Common.Models;
using RecyclingApp.Application.Features.Admin.Analytics.DTOs;
using RecyclingApp.Domain.Enums;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Admin.Analytics.Queries.GetSummary;

/// <summary>
/// CQRS Query for high-level executive dashboard summary metrics.
/// </summary>
public record GetAnalyticsSummaryQuery : IRequest<Result<SummaryMetricsDto>>;

public class GetAnalyticsSummaryQueryHandler : IRequestHandler<GetAnalyticsSummaryQuery, Result<SummaryMetricsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAnalyticsSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SummaryMetricsDto>> Handle(GetAnalyticsSummaryQuery request, CancellationToken cancellationToken)
    {
        // 1. Order Aggregations from Orders table
        var ordersQuery = _context.Orders.AsNoTracking();

        var ordersCount = await ordersQuery.CountAsync(cancellationToken);

        var pendingOrders = await ordersQuery.CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);
        var confirmedOrders = await ordersQuery.CountAsync(o => o.Status == OrderStatus.Confirmed, cancellationToken);
        var readyForPickupOrders = await ordersQuery.CountAsync(o => o.Status == OrderStatus.ReadyForPickup, cancellationToken);
        var completedOrders = await ordersQuery.CountAsync(o => o.Status == OrderStatus.Completed, cancellationToken);
        var cancelledOrders = await ordersQuery.CountAsync(o => o.Status == OrderStatus.Cancelled, cancellationToken);

        var ordersRevenue = await ordersQuery
            .Where(o => o.Status != OrderStatus.Cancelled)
            .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0m;

        // Legacy transactions support (if any exist in database)
        var legacyTransactions = _context.RecyclingTransactions.AsNoTracking();
        var legacyCompleted = await legacyTransactions.CountAsync(t => t.Status == "Completed", cancellationToken);
        var legacyRevenue = await legacyTransactions
            .Where(t => t.Status == "Completed")
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

        var totalOrders = ordersCount + (ordersCount == 0 ? legacyCompleted : 0);
        var totalRevenue = ordersRevenue + (ordersCount == 0 ? legacyRevenue : 0);
        var finalCompleted = completedOrders + (ordersCount == 0 ? legacyCompleted : 0);

        // 2. Customer Aggregations
        var totalCustomers = await _context.Users.AsNoTracking().CountAsync(cancellationToken);

        // 3. Product Inventory Aggregations
        var activeProductsQuery = _context.Products.AsNoTracking().Where(p => p.IsActive);

        var inStockCount = await activeProductsQuery.CountAsync(p => p.StockQuantity > 0, cancellationToken);
        var outOfStockCount = await activeProductsQuery.CountAsync(p => p.StockQuantity == 0, cancellationToken);
        var lowStockCount = await activeProductsQuery.CountAsync(p => p.StockQuantity <= p.LowStockThreshold && p.StockQuantity > 0, cancellationToken);

        var summary = new SummaryMetricsDto(
            TotalRevenue: totalRevenue,
            TotalOrders: totalOrders,
            OrdersByStatus: new OrderStatusBreakdownDto(
                Pending: pendingOrders,
                Confirmed: confirmedOrders,
                ReadyForPickup: readyForPickupOrders,
                Completed: finalCompleted,
                Cancelled: cancelledOrders
            ),
            TotalCustomers: totalCustomers,
            ActiveProductsInStock: inStockCount,
            ActiveProductsOutOfStock: outOfStockCount,
            LowStockProductsCount: lowStockCount
        );

        return Result<SummaryMetricsDto>.Success(summary);
    }
}
