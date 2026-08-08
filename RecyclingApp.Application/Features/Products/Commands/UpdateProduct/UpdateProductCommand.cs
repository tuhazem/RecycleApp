using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Domain.ValueObjects;

namespace RecyclingApp.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal PriceAmount,
    string PriceCurrency,
    int LowStockThreshold,
    string SKU,
    Guid CategoryId) : IRequest<UpdateProductResult>;

public record UpdateProductResult(bool Succeeded, string? Message);

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, UpdateProductResult>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateProductResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { request.Id }, cancellationToken);
        if (product is null)
        {
            return new UpdateProductResult(false, "Product not found.");
        }

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId && c.IsActive, cancellationToken);

        if (!categoryExists)
        {
            return new UpdateProductResult(false, "Category not found or inactive.");
        }

        var skuDuplicate = await _context.Products
            .AnyAsync(p => p.SKU == request.SKU.ToUpperInvariant() && p.Id != request.Id, cancellationToken);

        if (skuDuplicate)
        {
            return new UpdateProductResult(false, $"A product with SKU '{request.SKU}' already exists.");
        }

        var currency = string.IsNullOrWhiteSpace(request.PriceCurrency) ? "EGP" : request.PriceCurrency;
        var price = new Money(request.PriceAmount, currency);

        product.Update(request.Name, request.Description, price, request.LowStockThreshold, request.SKU, request.CategoryId);
        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateProductResult(true, "Product updated successfully.");
    }
}

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(150).WithMessage("Product name must not exceed 150 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Product description is required.")
            .MaximumLength(1000).WithMessage("Product description must not exceed 1000 characters.");

        RuleFor(x => x.PriceAmount)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleFor(x => x.LowStockThreshold)
            .GreaterThanOrEqualTo(0).WithMessage("Low stock threshold cannot be negative.");
    }
}
