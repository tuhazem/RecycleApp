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
    List<CustomerOrderItemDto> Orders);

/// <summary>
/// Individual line item / transaction entry in a customer's order history.
/// </summary>
public record CustomerOrderItemDto(
    Guid OrderId,
    Guid ProductId,
    string ProductName,
    string SKU,
    int Quantity,
    decimal Amount,
    DateTime OrderDate,
    string Status);
