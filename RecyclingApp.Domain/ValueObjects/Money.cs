using RecyclingApp.Domain.Common;
using System;
using Microsoft.EntityFrameworkCore;

namespace RecyclingApp.Domain.ValueObjects;

/// <summary>
/// Money Value Object representing a price with amount and currency.
/// </summary>
[Owned]
public record Money : ValueObject
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "EGP";

    // EF Core parameterless constructor
    private Money() { }

    public Money(decimal amount, string currency = "EGP")
    {
        if (amount < 0)
        {
            throw new ArgumentException("Money amount cannot be negative.", nameof(amount));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(currency, nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }
}
