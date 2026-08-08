using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Features.Products.DTOs;

namespace RecyclingApp.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
