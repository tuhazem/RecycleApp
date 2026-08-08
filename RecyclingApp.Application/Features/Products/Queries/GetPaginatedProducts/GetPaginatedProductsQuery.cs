using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Features.Products.DTOs;
using System.Linq;

namespace RecyclingApp.Application.Features.Products.Queries.GetPaginatedProducts;

public record GetPaginatedProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDescending = false,
    Guid? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? IsActive = null) : IRequest<PaginatedResult<ProductDto>>;

public class GetPaginatedProductsQueryHandler : IRequestHandler<GetPaginatedProductsQuery, PaginatedResult<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPaginatedProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<ProductDto>> Handle(GetPaginatedProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products.AsNoTracking().Include(p => p.Category).AsQueryable();

        // 1. Filtering
        if (request.CategoryId.HasValue && request.CategoryId != Guid.Empty)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price.Amount >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price.Amount <= request.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search) || 
                                     p.Description.ToLower().Contains(search) || 
                                     p.SKU.ToLower().Contains(search));
        }

        // 2. Sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => request.SortDescending ? query.OrderByDescending(p => p.Price.Amount) : query.OrderBy(p => p.Price.Amount),
            "stock" => request.SortDescending ? query.OrderByDescending(p => p.StockQuantity) : query.OrderBy(p => p.StockQuantity),
            "sku" => request.SortDescending ? query.OrderByDescending(p => p.SKU) : query.OrderBy(p => p.SKU),
            _ => query.OrderBy(p => p.Name) // default sort
        };

        // 3. Pagination count
        var totalCount = await query.CountAsync(cancellationToken);

        // 4. Page parameters bound check
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
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

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResult<ProductDto>(items, totalCount, pageNumber, pageSize, totalPages);
    }
}
