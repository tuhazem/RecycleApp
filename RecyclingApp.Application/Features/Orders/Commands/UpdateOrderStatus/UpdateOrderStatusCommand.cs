using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Orders.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus NewStatus) : IRequest<UpdateOrderStatusResult>;

public record UpdateOrderStatusResult(bool Succeeded, string Message, string? NewStatusName = null);

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid order status value.");
    }
}

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>
{
    private readonly IApplicationDbContext _context;

    public UpdateOrderStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateOrderStatusResult> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            return new UpdateOrderStatusResult(false, $"Order with ID '{request.OrderId}' was not found.");
        }

        order.UpdateStatus(request.NewStatus);
        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateOrderStatusResult(true, $"Order status updated to '{request.NewStatus}'.", request.NewStatus.ToString());
    }
}
