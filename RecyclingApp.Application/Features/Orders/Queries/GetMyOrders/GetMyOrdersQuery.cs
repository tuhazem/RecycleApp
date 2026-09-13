using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Features.Orders.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Orders.Queries.GetMyOrders;

public record GetMyOrdersQuery(string CustomerId) : IRequest<List<PickupOrderResponseDto>>;

public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, List<PickupOrderResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMyOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PickupOrderResponseDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerId))
        {
            return new List<PickupOrderResponseDto>();
        }

        var orders = await _context.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .Where(o => o.CustomerId == request.CustomerId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return orders.Select(order => new PickupOrderResponseDto(
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
                i.SubTotal)).ToList()
        )).ToList();
    }
}
