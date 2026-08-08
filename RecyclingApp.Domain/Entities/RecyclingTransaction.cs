using System;

namespace RecyclingApp.Domain.Entities;

/// <summary>
/// Domain model for a Recycling/Product Transaction in the system.
/// </summary>
public class RecyclingTransaction
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = default!;
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime TransactionDate { get; private set; }

    public virtual ApplicationUser User { get; private set; } = default!;
    public virtual Product Product { get; private set; } = default!;

    // EF Core parameterless constructor
    protected RecyclingTransaction() { }

    public RecyclingTransaction(string userId, Guid productId, int quantity, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID is required.", nameof(userId));
        }

        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product ID is required.", nameof(productId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        if (amount < 0)
        {
            throw new ArgumentException("Transaction amount cannot be negative.", nameof(amount));
        }

        Id = Guid.NewGuid();
        UserId = userId;
        ProductId = productId;
        Quantity = quantity;
        Amount = amount;
        TransactionDate = DateTime.UtcNow;
    }
}
