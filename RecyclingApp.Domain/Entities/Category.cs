using System;
using System.Collections.Generic;

namespace RecyclingApp.Domain.Entities;

/// <summary>
/// Domain model for Product Category.
/// </summary>
public class Category
{
    private readonly List<Product> _products = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public bool IsActive { get; private set; }
    
    public virtual IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    // EF Core parameterless constructor
    protected Category() { }

    public Category(string name, string description)
    {
        Id = Guid.NewGuid();
        Update(name, description);
        IsActive = true;
    }

    public void Update(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

        Name = name;
        Description = description;
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
