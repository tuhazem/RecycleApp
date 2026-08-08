using FluentValidation;
using MediatR;
using RecyclingApp.Application.Common.Interfaces;

namespace RecyclingApp.Application.Features.Products.Commands.AdjustStock;

public record AdjustProductStockCommand(Guid ProductId, int Delta) : IRequest<AdjustStockResult>;

public record AdjustStockResult(bool Succeeded, string? Message, int? NewStockQuantity = null);

public class AdjustProductStockCommandHandler : IRequestHandler<AdjustProductStockCommand, AdjustStockResult>
{
    private readonly IApplicationDbContext _context;

    public AdjustProductStockCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdjustStockResult> Handle(AdjustProductStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { request.ProductId }, cancellationToken);
        if (product is null)
        {
            return new AdjustStockResult(false, "Product not found.");
        }

        try
        {
            product.UpdateStock(request.Delta);
            await _context.SaveChangesAsync(cancellationToken);
            return new AdjustStockResult(true, "Product stock adjusted successfully.", product.StockQuantity);
        }
        catch (InvalidOperationException ex)
        {
            return new AdjustStockResult(false, ex.Message);
        }
    }
}

public class AdjustProductStockCommandValidator : AbstractValidator<AdjustProductStockCommand>
{
    public AdjustProductStockCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product ID is required.");
        RuleFor(x => x.Delta).NotEqual(0).WithMessage("Adjustment delta cannot be zero.");
    }
}
