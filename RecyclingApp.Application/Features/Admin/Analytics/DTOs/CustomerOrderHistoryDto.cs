using System;
using System.Collections.Generic;

namespace RecyclingApp.Application.Features.Admin.Analytics.DTOs;

/// <summary>
/// Full purchase and order history for a single customer.
/// </summary>
public record CustomerOrderHistoryDto(
    string CustomerId,
    string CustomerName,
    string Email,
    string Phone,
    string Address,
    decimal TotalSpent,
    int TotalOrders,
    List<CustomerOrderSummaryDto> Orders);

/// <summary>
/// Detailed summary of an individual order placed by the customer.
/// </summary>
public record CustomerOrderSummaryDto(
    Guid OrderId,
    string OrderType,
    string Status,
    decimal TotalAmount,
    DateTime OrderDate,
    DateTime? PickupTime,
    string? Notes,
    List<CustomerOrderItemDto> Items);

/// <summary>
/// Individual line item entry within an order.
/// </summary>
public record CustomerOrderItemDto(
    Guid OrderId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal SubTotal);
