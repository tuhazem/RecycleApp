using System;
using System.Collections.Generic;

namespace RecyclingApp.Application.Features.Orders.DTOs;

public record CreatePickupOrderItemDto(
    Guid ProductId,
    int Quantity);

public record CreatePickupOrderDto(
    string? CustomerId,
    string? CustomerName,
    string? Phone,
    string? Notes,
    DateTime PickupTime,
    List<CreatePickupOrderItemDto> Items);

public record OrderItemResponseDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal SubTotal);

public record PickupOrderResponseDto(
    Guid Id,
    string CustomerId,
    string CustomerName,
    string Phone,
    string? Notes,
    DateTime PickupTime,
    string OrderType,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAtUtc,
    List<OrderItemResponseDto> Items);
