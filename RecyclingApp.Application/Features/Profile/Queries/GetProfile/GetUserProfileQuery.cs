using MediatR;
using Microsoft.AspNetCore.Identity;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Common.Models;
using RecyclingApp.Application.Features.Profile.DTOs;
using RecyclingApp.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Profile.Queries.GetProfile;

/// <summary>
/// CQRS Query to retrieve a user profile by user identifier.
/// </summary>
public record GetUserProfileQuery(string UserId) : IRequest<Result<UserProfileDto>>;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICacheService _cacheService;

    public GetUserProfileQueryHandler(UserManager<ApplicationUser> userManager, ICacheService cacheService)
    {
        _userManager = userManager;
        _cacheService = cacheService;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return Result<UserProfileDto>.Failure("User identifier is required.");
        }

        var cacheKey = $"user-profile-{request.UserId}";

        // 1. Try to fetch from Redis distributed cache
        var cachedProfile = await _cacheService.GetAsync<UserProfileDto>(cacheKey, cancellationToken);
        if (cachedProfile != null)
        {
            return Result<UserProfileDto>.Success(cachedProfile);
        }

        // 2. Fetch from database if cache miss
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return Result<UserProfileDto>.Failure("User profile not found.");
        }

        var addressDisplay = user.Address != null
            ? $"{user.Address.BuildingNumber} {user.Address.Street}, {user.Address.City}".Trim()
            : string.Empty;

        var profileDto = new UserProfileDto(
            user.Id,
            user.UserName ?? user.Email ?? string.Empty,
            user.Email ?? string.Empty,
            user.PhoneNumber ?? string.Empty,
            addressDisplay
        );

        // 3. Cache the retrieved profile in Redis (expires in 15 minutes)
        await _cacheService.SetAsync(cacheKey, profileDto, TimeSpan.FromMinutes(15), cancellationToken);

        return Result<UserProfileDto>.Success(profileDto);
    }
}
