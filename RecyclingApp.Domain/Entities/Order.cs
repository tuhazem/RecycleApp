using RecyclingApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RecyclingApp.Domain.Entities;

/// <summary>
/// Domain model for an Order.
/// </summary>
public class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; }
    public string CustomerId { get; private set; } = default!;
    public string CustomerName { get; private set; } = default!;
    public string Phone { get; private set; } = default!;
    public string? Notes { get; private set; }
    public DateTime PickupTime { get; private set; }
    public OrderType OrderType { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public virtual ApplicationUser Customer { get; private set; } = default!;
    public virtual IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // EF Core parameterless constructor
    protected Order() { }

    public Order(
        string customerId,
        string customerName,
        string phone,
        string? notes,
        DateTime pickupTime,
        OrderType orderType = OrderType.Pickup)
    {
        Id = Guid.NewGuid();
        CustomerId = !string.IsNullOrWhiteSpace(customerId) ? customerId.Trim() : throw new ArgumentException("CustomerId is required.", nameof(customerId));
        CustomerName = !string.IsNullOrWhiteSpace(customerName) ? customerName.Trim() : throw new ArgumentException("CustomerName is required.", nameof(customerName));
        Phone = !string.IsNullOrWhiteSpace(phone) ? phone.Trim() : throw new ArgumentException("Phone is required.", nameof(phone));
        Notes = notes?.Trim();
        PickupTime = pickupTime;
        OrderType = orderType;
        Status = OrderStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        var item = new OrderItem(productId, productName, unitPrice, quantity);
        _items.Add(item);
        RecalculateTotal();
    }

    public void RecalculateTotal()
    {
        TotalAmount = _items.Sum(i => i.SubTotal);
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
    }
}
