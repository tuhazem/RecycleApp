using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Features.Categories.DTOs;

namespace RecyclingApp.Application.Features.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto?>;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCategoryByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.IsActive,
                c.Products.Count))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
