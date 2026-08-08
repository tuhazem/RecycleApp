using FluentValidation;
using MediatR;
using RecyclingApp.Application.Common.Interfaces;

namespace RecyclingApp.Application.Features.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : IRequest<DeleteCategoryResult>;

public record DeleteCategoryResult(bool Succeeded, string? Message);

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, DeleteCategoryResult>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteCategoryResult> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FindAsync(new object[] { request.Id }, cancellationToken);
        if (category is null)
        {
            return new DeleteCategoryResult(false, "Category not found.");
        }

        category.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteCategoryResult(true, "Category deactivated successfully.");
    }
}

public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Category ID is required.");
    }
}
