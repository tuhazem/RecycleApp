using System.Collections.Generic;

namespace RecyclingApp.Application.Features.Admin.Analytics.DTOs;

/// <summary>
/// High-level executive dashboard summary metrics.
/// </summary>
public record SummaryMetricsDto(
    decimal TotalRevenue,
    int TotalOrders,
    OrderStatusBreakdownDto OrdersByStatus,
    int TotalCustomers,
    int ActiveProductsInStock,
    int ActiveProductsOutOfStock,
    int LowStockProductsCount);

/// <summary>
/// Breakdown of orders by fulfillment/processing status.
/// </summary>
public record OrderStatusBreakdownDto(
    int Pending,
    int Confirmed,
    int ReadyForPickup,
    int Completed,
    int Cancelled);
