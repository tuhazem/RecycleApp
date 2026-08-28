using System;

namespace RecyclingApp.Application.Features.Admin.Analytics.DTOs;

/// <summary>
/// DTO representing a customer directory entry with aggregated purchase metrics.
/// </summary>
public record CustomerDirectoryDto(
    string CustomerId,
    string CustomerName,
    string Email,
    string Phone,
    string Address,
    decimal PointsBalance,
    int TotalOrdersCount,
    decimal TotalSpentAmount,
    DateTime? LastOrderDate);
