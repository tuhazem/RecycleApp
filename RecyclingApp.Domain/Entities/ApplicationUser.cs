using Microsoft.AspNetCore.Identity;
using RecyclingApp.Domain.ValueObjects;

namespace RecyclingApp.Domain.Entities;

/// <summary>
/// Domain model for the application user, extending ASP.NET Core Identity's IdentityUser.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = default!;
    public Address Address { get; set; } = default!;
    public decimal PointsBalance { get; set; } = 0;
}
