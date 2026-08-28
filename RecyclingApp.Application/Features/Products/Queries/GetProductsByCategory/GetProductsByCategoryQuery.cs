using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Common.Models;
using RecyclingApp.Application.Features.Products.DTOs;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Products.Queries.GetProductsByCategory;

/// <summary>
/// CQRS Query for category-based product filtering with pagination and AsNoTracking for high performance.
/// </summary>
public record GetProductsByCategoryQuery(
    Guid CategoryId,
    int PageIndex = 1,
    int PageSize = 10) : IRequest<Result<PagedResult<ProductDto>>>;

public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsByCategoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        // 1. Validate Category Existence
        var categoryExists = await _context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result<PagedResult<ProductDto>>.Failure($"Category with ID '{request.CategoryId}' was not found.");
        }

        // 2. Base Query with AsNoTracking() for high read performance
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CategoryId == request.CategoryId && p.IsActive);

        // 3. Total count calculation
        var totalCount = await query.CountAsync(cancellationToken);

        // 4. Bound check parameters
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        // 5. Paginated projection
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((pageIndex - 1) * pageSize)
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
                p.IsActive
            ))
            .ToListAsync(cancellationToken);

        // 6. Return standardized PagedResult
        var pagedResult = new PagedResult<ProductDto>(items, totalCount, pageIndex, pageSize);
        return Result<PagedResult<ProductDto>>.Success(pagedResult);
    }
}

public class GetProductsByCategoryQueryValidator : AbstractValidator<GetProductsByCategoryQuery>
{
    public GetProductsByCategoryQueryValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required and must not be empty.");

        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(1).WithMessage("PageIndex must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
