namespace RecyclingApp.Application.Features.Admin.Analytics.DTOs;

/// <summary>
/// DTO representing top customers / big spenders.
/// </summary>
public record TopCustomerDto(
    string CustomerId,
    string CustomerName,
    string Email,
    string Phone,
    int TotalOrdersPlaced,
    decimal TotalSpentAmount);
