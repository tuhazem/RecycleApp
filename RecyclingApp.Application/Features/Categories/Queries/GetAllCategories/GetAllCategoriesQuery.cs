using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Features.Categories.DTOs;

namespace RecyclingApp.Application.Features.Categories.Queries.GetAllCategories;

public record GetAllCategoriesQuery(bool OnlyActive = true) : IRequest<List<CategoryDto>>;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<CategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Categories.AsNoTracking();

        if (request.OnlyActive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.IsActive,
                c.Products.Count))
            .ToListAsync(cancellationToken);
    }
}
