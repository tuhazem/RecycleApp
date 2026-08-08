using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Features.Products.DTOs;

namespace RecyclingApp.Application.Features.Products.Queries.GetLowStockProducts;

public record GetLowStockProductsQuery : IRequest<List<ProductDto>>;

public class GetLowStockProductsQueryHandler : IRequestHandler<GetLowStockProductsQuery, List<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLowStockProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> Handle(GetLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.StockQuantity <= p.LowStockThreshold && p.StockQuantity > 0)
            .OrderBy(p => p.StockQuantity)
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.Price.Amount,
                p.Price.Currency,
                p.StockQuantity,
                p.LowStockThreshold,
                p.SKU,
                p.CategoryId,
                p.Category.Name,
                p.IsActive))
            .ToListAsync(cancellationToken);
    }
}
