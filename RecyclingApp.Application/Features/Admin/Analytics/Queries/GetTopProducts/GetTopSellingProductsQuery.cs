using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Common.Models;
using RecyclingApp.Application.Features.Admin.Analytics.DTOs;
using RecyclingApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Admin.Analytics.Queries.GetTopProducts;

/// <summary>
/// CQRS Query to retrieve the top N best-selling products by quantity sold.
/// </summary>
public record GetTopSellingProductsQuery(int Count = 5) : IRequest<Result<List<TopProductDto>>>;

public class GetTopSellingProductsQueryHandler : IRequestHandler<GetTopSellingProductsQuery, Result<List<TopProductDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetTopSellingProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<TopProductDto>>> Handle(GetTopSellingProductsQuery request, CancellationToken cancellationToken)
    {
        var count = request.Count is < 1 or > 50 ? 5 : request.Count;

        // 1. Group order items on the database server to calculate quantity and revenue per product
        var topProductAggregates = await _context.OrderItems
            .AsNoTracking()
            .Where(i => i.Order.Status != OrderStatus.Cancelled)
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalQuantitySold = g.Sum(i => i.Quantity),
                TotalRevenueGenerated = g.Sum(i => i.UnitPrice * i.Quantity)
            })
            .OrderByDescending(g => g.TotalQuantitySold)
            .Take(count)
            .ToListAsync(cancellationToken);

        // Fallback to legacy transactions if no orders exist yet
        if (topProductAggregates.Count == 0)
        {
            topProductAggregates = await _context.RecyclingTransactions
                .AsNoTracking()
                .Where(t => t.Status != "Cancelled")
                .GroupBy(t => t.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantitySold = g.Sum(t => t.Quantity),
                    TotalRevenueGenerated = g.Sum(t => t.Amount)
                })
                .OrderByDescending(g => g.TotalQuantitySold)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        if (topProductAggregates.Count == 0)
        {
            return Result<List<TopProductDto>>.Success(new List<TopProductDto>());
        }

        var productIds = topProductAggregates.Select(x => x.ProductId).ToList();

        // 2. Fetch product metadata
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        // 3. Project to TopProductDto with accurate stock statuses
        var result = topProductAggregates.Select(item =>
        {
            if (products.TryGetValue(item.ProductId, out var product))
            {
                var stockStatus = product.StockQuantity switch
                {
                    0 => "OutOfStock",
                    var qty when qty <= product.LowStockThreshold => "LowStock",
                    _ => "InStock"
                };

                return new TopProductDto(
                    product.Id,
                    product.Name,
                    product.SKU,
                    item.TotalQuantitySold,
                    item.TotalRevenueGenerated,
                    product.StockQuantity,
                    stockStatus
                );
            }

            return new TopProductDto(
                item.ProductId,
                "Archived Product",
                "N/A",
                item.TotalQuantitySold,
                item.TotalRevenueGenerated,
                0,
                "OutOfStock"
            );
        }).ToList();

        return Result<List<TopProductDto>>.Success(result);
    }
}

public class GetTopSellingProductsQueryValidator : AbstractValidator<GetTopSellingProductsQuery>
{
    public GetTopSellingProductsQueryValidator()
    {
        RuleFor(x => x.Count)
            .InclusiveBetween(1, 50).WithMessage("Count must be between 1 and 50.");
    }
}
