using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Domain.Entities;
using RecyclingApp.Domain.ValueObjects;

namespace RecyclingApp.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal PriceAmount,
    string PriceCurrency,
    int StockQuantity,
    int LowStockThreshold,
    string SKU,
    Guid CategoryId) : IRequest<CreateProductResult>;

public record CreateProductResult(bool Succeeded, string? Message, Guid? ProductId = null);

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CreateProductResult>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateProductResult> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId && c.IsActive, cancellationToken);

        if (!categoryExists)
        {
            return new CreateProductResult(false, "Category not found or inactive.");
        }

        var skuExists = await _context.Products
            .AnyAsync(p => p.SKU == request.SKU.ToUpperInvariant(), cancellationToken);

        if (skuExists)
        {
            return new CreateProductResult(false, $"A product with SKU '{request.SKU}' already exists.");
        }

        var currency = string.IsNullOrWhiteSpace(request.PriceCurrency) ? "EGP" : request.PriceCurrency;
        var price = new Money(request.PriceAmount, currency);

        var product = new Product(
            request.Name,
            request.Description,
            price,
            request.StockQuantity,
            request.LowStockThreshold,
            request.SKU,
            request.CategoryId);

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateProductResult(true, "Product created successfully.", product.Id);
    }
}

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
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

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

        RuleFor(x => x.LowStockThreshold)
            .GreaterThanOrEqualTo(0).WithMessage("Low stock threshold cannot be negative.");
    }
}
