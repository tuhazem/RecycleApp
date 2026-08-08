namespace RecyclingApp.Application.Features.Products.DTOs;

/// <summary>
/// Data Transfer Object for Product.
/// </summary>
public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal PriceAmount,
    string PriceCurrency,
    int StockQuantity,
    int LowStockThreshold,
    string SKU,
    Guid CategoryId,
    string CategoryName,
    bool IsActive);

/// <summary>
/// Paginated result wrapper.
/// </summary>
public record PaginatedResult<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);
