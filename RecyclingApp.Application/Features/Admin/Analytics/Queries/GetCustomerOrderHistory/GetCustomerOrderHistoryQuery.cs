using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Common.Models;
using RecyclingApp.Application.Features.Admin.Analytics.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Admin.Analytics.Queries.GetCustomerOrderHistory;

/// <summary>
/// CQRS Query to retrieve the complete purchase/transaction history for a specific customer.
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

        // 2. Fetch all orders/transactions for this customer
        var transactions = await _context.RecyclingTransactions
            .AsNoTracking()
            .Include(t => t.Product)
            .Where(t => t.UserId == request.CustomerId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);

        // 3. Map orders to DTO
        var orderItems = transactions.Select(t => new CustomerOrderItemDto(
            t.Id,
            t.ProductId,
            t.Product != null ? t.Product.Name : "Product",
            t.Product != null ? t.Product.SKU : "N/A",
            t.Quantity,
            t.Amount,
            t.TransactionDate,
            t.Status
        )).ToList();

        var totalSpent = transactions
            .Where(t => t.Status == "Completed")
            .Sum(t => t.Amount);

        var addressDisplay = user.Address != null
            ? $"{user.Address.BuildingNumber} {user.Address.Street}, {user.Address.City}".Trim()
            : string.Empty;

        var historyDto = new CustomerOrderHistoryDto(
            user.Id,
            !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : (user.UserName ?? "Customer"),
            user.Email ?? string.Empty,
            user.PhoneNumber ?? string.Empty,
            addressDisplay,
            totalSpent,
            transactions.Count,
            orderItems
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
