using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Common.Models;
using RecyclingApp.Application.Features.Admin.Analytics.DTOs;
using RecyclingApp.Domain.Enums;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Admin.Analytics.Queries.GetCustomerOrderHistory;

/// <summary>
/// CQRS Query to retrieve the complete purchase/order history for a specific customer.
/// </summary>
public record GetCustomerOrderHistoryQuery(string CustomerId) : IRequest<Result<CustomerOrderHistoryDto>>;

public class GetCustomerOrderHistoryQueryHandler : IRequestHandler<GetCustomerOrderHistoryQuery, Result<CustomerOrderHistoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerOrderHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CustomerOrderHistoryDto>> Handle(GetCustomerOrderHistoryQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerId))
        {
            return Result<CustomerOrderHistoryDto>.Failure("Customer ID is required.");
        }

        // 1. Fetch Customer details
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.CustomerId, cancellationToken);

        if (user == null)
        {
            return Result<CustomerOrderHistoryDto>.Failure($"Customer with ID '{request.CustomerId}' was not found.");
        }

        // 2. Fetch all orders for this customer (including items)
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.CustomerId == request.CustomerId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        // 3. Map orders to DTO with explicit OrderId
        var orderSummaries = orders.Select(o => new CustomerOrderSummaryDto(
            OrderId: o.Id,
            OrderType: o.OrderType.ToString(),
            Status: o.Status.ToString(),
            TotalAmount: o.TotalAmount,
            OrderDate: o.CreatedAtUtc,
            PickupTime: o.PickupTime,
            Notes: o.Notes,
            Items: o.Items.Select(i => new CustomerOrderItemDto(
                OrderId: o.Id,
                ProductId: i.ProductId,
                ProductName: i.ProductName,
                UnitPrice: i.UnitPrice,
                Quantity: i.Quantity,
                SubTotal: i.SubTotal
            )).ToList()
        )).ToList();

        var totalSpent = orders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .Sum(o => o.TotalAmount);

        var addressDisplay = user.Address != null
            ? $"{user.Address.BuildingNumber} {user.Address.Street}, {user.Address.City}".Trim()
            : string.Empty;

        var historyDto = new CustomerOrderHistoryDto(
            CustomerId: user.Id,
            CustomerName: !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : (user.UserName ?? "Customer"),
            Email: user.Email ?? string.Empty,
            Phone: user.PhoneNumber ?? string.Empty,
            Address: addressDisplay,
            TotalSpent: totalSpent,
            TotalOrders: orders.Count,
            Orders: orderSummaries
        );

        return Result<CustomerOrderHistoryDto>.Success(historyDto);
    }
}

public class GetCustomerOrderHistoryQueryValidator : AbstractValidator<GetCustomerOrderHistoryQuery>
{
    public GetCustomerOrderHistoryQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");
    }
}
