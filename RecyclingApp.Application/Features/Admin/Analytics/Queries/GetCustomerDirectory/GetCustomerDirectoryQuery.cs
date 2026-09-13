using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Common.Models;
using RecyclingApp.Application.Features.Admin.Analytics.DTOs;
using RecyclingApp.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Admin.Analytics.Queries.GetCustomerDirectory;

/// <summary>
/// CQRS Query for paginated customer directory listing with search and spend analytics.
/// </summary>
public record GetCustomerDirectoryQuery(
    int PageIndex = 1,
    int PageSize = 10,
    string? SearchTerm = null) : IRequest<Result<PagedResult<CustomerDirectoryDto>>>;

public class GetCustomerDirectoryQueryHandler : IRequestHandler<GetCustomerDirectoryQuery, Result<PagedResult<CustomerDirectoryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerDirectoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<CustomerDirectoryDto>>> Handle(GetCustomerDirectoryQuery request, CancellationToken cancellationToken)
    {
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

        // 1. Filter customers by search term
        var usersQuery = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            usersQuery = usersQuery.Where(u =>
                (u.FullName != null && u.FullName.ToLower().Contains(term)) ||
                (u.Email != null && u.Email.ToLower().Contains(term)) ||
                (u.UserName != null && u.UserName.ToLower().Contains(term)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(term))
            );
        }

        var totalCount = await usersQuery.CountAsync(cancellationToken);

        // 2. Fetch paginated users
        var users = await usersQuery
            .OrderBy(u => u.FullName)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (users.Count == 0)
        {
            var emptyResult = new PagedResult<CustomerDirectoryDto>(Array.Empty<CustomerDirectoryDto>(), totalCount, pageIndex, pageSize);
            return Result<PagedResult<CustomerDirectoryDto>>.Success(emptyResult);
        }

        var userIds = users.Select(u => u.Id).ToList();

        // 3. Batch aggregate purchase statistics for the current page users from Orders table
        var userStats = await _context.Orders
            .AsNoTracking()
            .Where(o => userIds.Contains(o.CustomerId))
            .GroupBy(o => o.CustomerId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalOrdersCount = g.Count(),
                TotalSpentAmount = g.Where(o => o.Status != OrderStatus.Cancelled).Sum(o => (decimal?)o.TotalAmount) ?? 0m,
                LastOrderDate = g.Max(o => (DateTime?)o.CreatedAtUtc)
            })
            .ToDictionaryAsync(s => s.UserId, cancellationToken);

        // 4. Project into CustomerDirectoryDto
        var items = users.Select(u =>
        {
            userStats.TryGetValue(u.Id, out var stats);

            var addressDisplay = u.Address != null
                ? $"{u.Address.BuildingNumber} {u.Address.Street}, {u.Address.City}".Trim()
                : string.Empty;

            return new CustomerDirectoryDto(
                u.Id,
                !string.IsNullOrWhiteSpace(u.FullName) ? u.FullName : (u.UserName ?? "Customer"),
                u.Email ?? string.Empty,
                u.PhoneNumber ?? string.Empty,
                addressDisplay,
                u.PointsBalance,
                stats?.TotalOrdersCount ?? 0,
                stats?.TotalSpentAmount ?? 0m,
                stats?.LastOrderDate
            );
        }).ToList();

        var pagedResult = new PagedResult<CustomerDirectoryDto>(items, totalCount, pageIndex, pageSize);
        return Result<PagedResult<CustomerDirectoryDto>>.Success(pagedResult);
    }
}

public class GetCustomerDirectoryQueryValidator : AbstractValidator<GetCustomerDirectoryQuery>
{
    public GetCustomerDirectoryQueryValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(1).WithMessage("PageIndex must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
