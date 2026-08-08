namespace RecyclingApp.Application.Features.Categories.DTOs;

/// <summary>
/// Data Transfer Object for Category.
/// </summary>
public record CategoryDto(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    int ProductCount);
