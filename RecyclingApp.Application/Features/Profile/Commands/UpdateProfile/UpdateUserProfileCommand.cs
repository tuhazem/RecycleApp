using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Common.Models;
using RecyclingApp.Application.Features.Profile.DTOs;
using RecyclingApp.Domain.Entities;
using RecyclingApp.Domain.ValueObjects;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Profile.Commands.UpdateProfile;

/// <summary>
/// CQRS Command to update the authenticated user profile.
/// </summary>
public record UpdateUserProfileCommand(
    string UserId,
    string Username,
    string Email,
    string Phone,
    string Address) : IRequest<Result<UpdateProfileResponse>>;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result<UpdateProfileResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICacheService _cacheService;

    public UpdateUserProfileCommandHandler(
        UserManager<ApplicationUser> userManager,
        ICacheService cacheService)
    {
        _userManager = userManager;
        _cacheService = cacheService;
    }

    public async Task<Result<UpdateProfileResponse>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch user by Id
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return Result<UpdateProfileResponse>.Failure("User profile not found.");
        }

        // 2. Check for Email conflicts if email is changing
        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var userWithEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithEmail != null && userWithEmail.Id != user.Id)
            {
                return Result<UpdateProfileResponse>.Failure("Email address is already in use by another account.");
            }
            user.Email = request.Email;
        }

        // 3. Check for Username conflicts if username is changing
        if (!string.Equals(user.UserName, request.Username, StringComparison.OrdinalIgnoreCase))
        {
            var userWithName = await _userManager.FindByNameAsync(request.Username);
            if (userWithName != null && userWithName.Id != user.Id)
            {
                return Result<UpdateProfileResponse>.Failure("Username is already taken by another account.");
            }
            user.UserName = request.Username;
        }

        // 4. Update phone number
        user.PhoneNumber = request.Phone;

        // 5. Update Address value object
        user.Address = ParseAddress(request.Address);

        // 6. Save changes
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = updateResult.Errors.Select(e => e.Description);
            return Result<UpdateProfileResponse>.Failure("Failed to update profile.", errors);
        }

        // 7. Evict user profile cache from Redis
        var cacheKey = $"user-profile-{user.Id}";
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);

        // 8. Build response DTO
        var addressDisplay = user.Address != null
            ? $"{user.Address.BuildingNumber} {user.Address.Street}, {user.Address.City}".Trim()
            : request.Address;

        var profileDto = new UserProfileDto(
            user.Id,
            user.UserName ?? user.Email ?? string.Empty,
            user.Email ?? string.Empty,
            user.PhoneNumber ?? string.Empty,
            addressDisplay
        );

        var response = new UpdateProfileResponse("Profile updated successfully", profileDto);
        return Result<UpdateProfileResponse>.Success(response, "Profile updated successfully");
    }

    private static Address ParseAddress(string addressString)
    {
        if (string.IsNullOrWhiteSpace(addressString))
        {
            return new Address("General", "City", "1");
        }

        var parts = addressString.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            >= 3 => new Address(parts[0], parts[1], parts[2]),
            2 => new Address(parts[0], parts[1], "1"),
            _ => new Address(addressString.Trim(), "City", "1")
        };
    }
}

/// <summary>
/// FluentValidation rules for UpdateUserProfileCommand.
/// </summary>
public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Phone number must be a valid format containing 10 to 15 digits.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(200).WithMessage("Address must not exceed 200 characters.");
    }
}
