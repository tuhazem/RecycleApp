using FluentValidation;
using System;

namespace RecyclingApp.Application.Features.Orders.Commands.CreatePickupOrder;

public class CreatePickupOrderCommandValidator : AbstractValidator<CreatePickupOrderCommand>
{
    public CreatePickupOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");

        RuleFor(x => x.Dto.PickupTime)
            .GreaterThan(DateTime.UtcNow).WithMessage("Pickup time must be in the future.");

        RuleFor(x => x.Dto.Items)
            .NotEmpty().WithMessage("At least one order item is required.");

        RuleForEach(x => x.Dto.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("ProductId is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });
    }
}
