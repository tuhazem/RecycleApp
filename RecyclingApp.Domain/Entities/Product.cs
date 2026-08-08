using RecyclingApp.Domain.ValueObjects;
using System;

namespace RecyclingApp.Domain.Entities;

/// <summary>
/// Domain model for Product.
/// </summary>
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public Money Price { get; private set; } = default!;
    public int StockQuantity { get; private set; }
    public int LowStockThreshold { get; private set; }
    public string SKU { get; private set; } = default!;
    public Guid CategoryId { get; private set; }
    public bool IsActive { get; private set; }

    public virtual Category Category { get; private set; } = default!;

    // EF Core parameterless constructor
    protected Product() { }

    public Product(
        string name,
        string description,
        Money price,
        int stockQuantity,
        int lowStockThreshold,
        string sku,
        Guid categoryId)
    {
        Id = Guid.NewGuid();
        StockQuantity = stockQuantity >= 0 ? stockQuantity : throw new ArgumentException("Initial stock quantity cannot be negative.", nameof(stockQuantity));
        
        Update(name, description, price, lowStockThreshold, sku, categoryId);
        IsActive = true;
    }

    public void Update(
        string name,
        string description,
        Money price,
        int lowStockThreshold,
        string sku,
        Guid categoryId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));
        ArgumentNullException.ThrowIfNull(price, nameof(price));
        ArgumentException.ThrowIfNullOrWhiteSpace(sku, nameof(sku));

        if (lowStockThreshold < 0)
        {
            throw new ArgumentException("Low stock threshold cannot be negative.", nameof(lowStockThreshold));
        }

        if (categoryId == Guid.Empty)
        {
            throw new ArgumentException("Category Id must be valid.", nameof(categoryId));
        }

        Name = name;
        Description = description;
        Price = price;
        LowStockThreshold = lowStockThreshold;
        SKU = sku.ToUpperInvariant();
        CategoryId = categoryId;
    }

    public void UpdateStock(int delta)
    {
        var newStock = StockQuantity + delta;
        if (newStock < 0)
        {
            throw new InvalidOperationException($"Insufficient stock. Current stock is {StockQuantity}, adjustment delta of {delta} is invalid.");
        }
        StockQuantity = newStock;
    }

    public void SetLowStockThreshold(int threshold)
    {
        if (threshold < 0)
        {
            throw new ArgumentException("Low stock threshold cannot be negative.", nameof(threshold));
        }
        LowStockThreshold = threshold;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
