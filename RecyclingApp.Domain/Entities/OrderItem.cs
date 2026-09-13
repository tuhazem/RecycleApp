using RecyclingApp.Domain.Enums;
using System;
using System.Collections.Generic;

namespace RecyclingApp.Domain.Entities;

/// <summary>
/// Domain model for an item within an Order.
/// </summary>
public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal SubTotal => UnitPrice * Quantity;

    public virtual Order Order { get; private set; } = default!;

    // EF Core parameterless constructor
    protected OrderItem() { }

    public OrderItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        Id = Guid.NewGuid();
        ProductId = productId != Guid.Empty ? productId : throw new ArgumentException("ProductId is required.", nameof(productId));
        ProductName = !string.IsNullOrWhiteSpace(productName) ? productName : throw new ArgumentException("ProductName is required.", nameof(productName));
        UnitPrice = unitPrice >= 0 ? unitPrice : throw new ArgumentException("UnitPrice cannot be negative.", nameof(unitPrice));
        Quantity = quantity > 0 ? quantity : throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
    }
}
