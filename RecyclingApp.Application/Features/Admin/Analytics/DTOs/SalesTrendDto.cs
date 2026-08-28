using System;
using System.Collections.Generic;

namespace RecyclingApp.Application.Features.Admin.Analytics.DTOs;

/// <summary>
/// Sales performance trend analytics container.
/// </summary>
public record SalesTrendDto(
    string Period,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalRevenue,
    int TotalOrders,
    List<SalesTrendPointDto> DataPoints);

/// <summary>
/// Single time-series data point for dashboard analytical charts.
/// </summary>
public record SalesTrendPointDto(
    string PeriodLabel,
    DateTime Date,
    int OrderCount,
    decimal TotalRevenue,
    int TotalItemsSold);
