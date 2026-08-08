using FluentValidation;
using MediatR;
using RecyclingApp.Application.Common.Interfaces;

namespace RecyclingApp.Application.Features.Products.Commands.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<DeleteProductResult>;

public record DeleteProductResult(bool Succeeded, string? Message);

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, DeleteProductResult>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteProductResult> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { request.Id }, cancellationToken);
        if (product is null)
        {
            return new DeleteProductResult(false, "Product not found.");
        }

        product.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteProductResult(true, "Product deactivated successfully.");
    }
}

public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Product ID is required.");
    }
}
