using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Features.Orders.DTOs;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid Id) : IRequest<PickupOrderResponseDto?>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, PickupOrderResponseDto?>
{
    private readonly IApplicationDbContext _context;

    public GetOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PickupOrderResponseDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order == null)
            return null;

        return new PickupOrderResponseDto(
            order.Id,
            order.CustomerId,
            order.CustomerName,
            order.Phone,
            order.Notes,
            order.PickupTime,
            order.OrderType.ToString(),
            order.Status.ToString(),
            order.TotalAmount,
            order.CreatedAtUtc,
            order.Items.Select(i => new OrderItemResponseDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.UnitPrice,
                i.Quantity,
                i.SubTotal)).ToList());
    }
}
