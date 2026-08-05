using RecyclingApp.Domain.Common;

namespace RecyclingApp.Domain.ValueObjects;

/// <summary>
/// Address Value Object representing a physical address.
/// </summary>
public record Address : ValueObject
{
    public string Street { get; init; } = default!;
    public string City { get; init; } = default!;
    public string BuildingNumber { get; init; } = default!;

    // EF Core requires a parameterless constructor for owned types/entities
    private Address() { }

    public Address(string street, string city, string buildingNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(street, nameof(street));
        ArgumentException.ThrowIfNullOrWhiteSpace(city, nameof(city));
        ArgumentException.ThrowIfNullOrWhiteSpace(buildingNumber, nameof(buildingNumber));

        Street = street;
        City = city;
        BuildingNumber = buildingNumber;
    }
}
