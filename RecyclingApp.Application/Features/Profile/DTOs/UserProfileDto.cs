namespace RecyclingApp.Application.Features.Profile.DTOs;

/// <summary>
/// Data Transfer Object representing the user profile.
/// </summary>
public record UserProfileDto(
    string Id,
    string Username,
    string Email,
    string Phone,
    string Address);

/// <summary>
/// Response model returned upon successfully updating a user profile.
/// </summary>
public record UpdateProfileResponse(
    string Message,
    UserProfileDto User);
