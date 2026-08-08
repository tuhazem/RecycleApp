using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Domain.Entities;

namespace RecyclingApp.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(string Name, string Description) : IRequest<CreateCategoryResult>;

public record CreateCategoryResult(bool Succeeded, string? Message, Guid? CategoryId = null);

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResult>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateCategoryResult> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Categories
            .AnyAsync(c => c.Name == request.Name && c.IsActive, cancellationToken);

        if (exists)
        {
            return new CreateCategoryResult(false, "A category with this name already exists.");
        }

        var category = new Category(request.Name, request.Description);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateCategoryResult(true, "Category created successfully.", category.Id);
    }
}

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Category description is required.")
            .MaximumLength(500).WithMessage("Category description must not exceed 500 characters.");
    }
}
