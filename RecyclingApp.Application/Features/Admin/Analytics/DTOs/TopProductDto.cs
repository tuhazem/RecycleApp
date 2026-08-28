using System;

namespace RecyclingApp.Application.Features.Admin.Analytics.DTOs;

/// <summary>
/// DTO representing top-selling product metrics.
/// </summary>
public record TopProductDto(
    Guid ProductId,
    string ProductName,
    string SKU,
    int TotalQuantitySold,
    decimal TotalRevenueGenerated,
    int CurrentStock,
    string StockStatus);
