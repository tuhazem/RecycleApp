using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;

namespace RecyclingApp.Application.Features.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(Guid Id, string Name, string Description) : IRequest<UpdateCategoryResult>;

public record UpdateCategoryResult(bool Succeeded, string? Message);

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, UpdateCategoryResult>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateCategoryResult> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FindAsync(new object[] { request.Id }, cancellationToken);
        if (category is null)
        {
            return new UpdateCategoryResult(false, "Category not found.");
        }

        var duplicate = await _context.Categories
            .AnyAsync(c => c.Name == request.Name && c.Id != request.Id && c.IsActive, cancellationToken);

        if (duplicate)
        {
            return new UpdateCategoryResult(false, "A category with this name already exists.");
        }

        category.Update(request.Name, request.Description);
        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateCategoryResult(true, "Category updated successfully.");
    }
}

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Category ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Category description is required.")
            .MaximumLength(500).WithMessage("Category description must not exceed 500 characters.");
    }
}
