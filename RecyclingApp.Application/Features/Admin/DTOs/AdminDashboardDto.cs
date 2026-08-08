namespace RecyclingApp.Application.Features.Admin.DTOs;

public record TopRecycledItemDto(
    Guid ProductId,
    string ProductName,
    int TotalQuantity,
    decimal TotalAmount);

public record AdminDashboardStatsDto(
    int TotalUsers,
    int TotalActiveProducts,
    int TotalActiveCategories,
    int OutOfStockCount,
    int LowStockCount,
    decimal TotalRevenue,
    int TotalTransactions,
    List<TopRecycledItemDto> TopRecycledItems);
