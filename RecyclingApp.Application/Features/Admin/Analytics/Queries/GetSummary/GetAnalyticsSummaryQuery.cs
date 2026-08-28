using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Common.Models;
using RecyclingApp.Application.Features.Admin.Analytics.DTOs;
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
        // 1. Transaction / Order Aggregations via DB server
        var transactionsQuery = _context.RecyclingTransactions.AsNoTracking();

        var totalOrders = await transactionsQuery.CountAsync(cancellationToken);
        
        var totalRevenue = await transactionsQuery
            .Where(t => t.Status == "Completed")
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

        var completedOrders = await transactionsQuery.CountAsync(t => t.Status == "Completed", cancellationToken);
        var pendingOrders = await transactionsQuery.CountAsync(t => t.Status == "Pending", cancellationToken);
        var cancelledOrders = await transactionsQuery.CountAsync(t => t.Status == "Cancelled", cancellationToken);

        // 2. Customer Aggregations
        var totalCustomers = await _context.Users.AsNoTracking().CountAsync(cancellationToken);

        // 3. Product Inventory Aggregations
        var activeProductsQuery = _context.Products.AsNoTracking().Where(p => p.IsActive);

        var inStockCount = await activeProductsQuery.CountAsync(p => p.StockQuantity > 0, cancellationToken);
        var outOfStockCount = await activeProductsQuery.CountAsync(p => p.StockQuantity == 0, cancellationToken);
        var lowStockCount = await activeProductsQuery.CountAsync(p => p.StockQuantity <= p.LowStockThreshold && p.StockQuantity > 0, cancellationToken);

        var summary = new SummaryMetricsDto(
            totalRevenue,
            totalOrders,
            new OrderStatusBreakdownDto(completedOrders, pendingOrders, cancelledOrders),
            totalCustomers,
            inStockCount,
            outOfStockCount,
            lowStockCount
        );

        return Result<SummaryMetricsDto>.Success(summary);
    }
}
