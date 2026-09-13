using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Common.Models;
using RecyclingApp.Application.Features.Admin.Analytics.DTOs;
using RecyclingApp.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Admin.Analytics.Queries.GetTopCustomers;

/// <summary>
/// CQRS Query to retrieve top spending customers / big spenders.
/// </summary>
public record GetTopCustomersQuery(int Count = 5) : IRequest<Result<List<TopCustomerDto>>>;

public class GetTopCustomersQueryHandler : IRequestHandler<GetTopCustomersQuery, Result<List<TopCustomerDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetTopCustomersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<TopCustomerDto>>> Handle(GetTopCustomersQuery request, CancellationToken cancellationToken)
    {
        var count = request.Count is < 1 or > 50 ? 5 : request.Count;

        // 1. Database-level aggregation of customer purchase statistics from Orders table
        var topCustomerAggregates = await _context.Orders
            .AsNoTracking()
            .Where(o => o.Status != OrderStatus.Cancelled)
            .GroupBy(o => o.CustomerId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalOrdersPlaced = g.Count(),
                TotalSpentAmount = g.Sum(o => o.TotalAmount)
            })
            .OrderByDescending(g => g.TotalSpentAmount)
            .Take(count)
            .ToListAsync(cancellationToken);

        // Fallback to legacy transactions if no orders exist yet
        if (topCustomerAggregates.Count == 0)
        {
            topCustomerAggregates = await _context.RecyclingTransactions
                .AsNoTracking()
                .Where(t => t.Status != "Cancelled")
                .GroupBy(t => t.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalOrdersPlaced = g.Count(),
                    TotalSpentAmount = g.Sum(t => t.Amount)
                })
                .OrderByDescending(g => g.TotalSpentAmount)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        if (topCustomerAggregates.Count == 0)
        {
            return Result<List<TopCustomerDto>>.Success(new List<TopCustomerDto>());
        }

        var userIds = topCustomerAggregates.Select(x => x.UserId).ToList();

        // 2. Fetch User identity and profile metadata
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        // 3. Project to TopCustomerDto
        var result = topCustomerAggregates.Select(item =>
        {
            if (users.TryGetValue(item.UserId, out var user))
            {
                return new TopCustomerDto(
                    user.Id,
                    !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : (user.UserName ?? "Customer"),
                    user.Email ?? string.Empty,
                    user.PhoneNumber ?? string.Empty,
                    item.TotalOrdersPlaced,
                    item.TotalSpentAmount
                );
            }

            return new TopCustomerDto(
                item.UserId,
                "Unknown Customer",
                string.Empty,
                string.Empty,
                item.TotalOrdersPlaced,
                item.TotalSpentAmount
            );
        }).ToList();

        return Result<List<TopCustomerDto>>.Success(result);
    }
}

public class GetTopCustomersQueryValidator : AbstractValidator<GetTopCustomersQuery>
{
    public GetTopCustomersQueryValidator()
    {
        RuleFor(x => x.Count)
            .InclusiveBetween(1, 50).WithMessage("Count must be between 1 and 50.");
    }
}
