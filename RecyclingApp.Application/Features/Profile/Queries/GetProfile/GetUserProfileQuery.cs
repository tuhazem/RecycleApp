using MediatR;
using Microsoft.AspNetCore.Identity;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Profile.Queries.GetProfile;

public record GetUserProfileQuery(string UserId) : IRequest<UserProfileResult>;

public record UserProfileResult(bool Succeeded, UserProfileDto? Profile = null, string? Message = null);

public record UserProfileDto(
    string Id,
    string FullName,
    string Email,
    string PhoneNumber,
    string Street,
    string City,
    string BuildingNumber,
    decimal PointsBalance);

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICacheService _cacheService;

    public GetUserProfileQueryHandler(UserManager<ApplicationUser> userManager, ICacheService cacheService)
    {
        _userManager = userManager;
        _cacheService = cacheService;
    }

    public async Task<UserProfileResult> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"user-profile-{request.UserId}";

        // 1. Try to fetch from Redis cache
        var cachedProfile = await _cacheService.GetAsync<UserProfileDto>(cacheKey, cancellationToken);
        if (cachedProfile != null)
        {
            return new UserProfileResult(true, Profile: cachedProfile);
        }

        // 2. Fetch from database if not cached
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return new UserProfileResult(false, Message: "User profile not found.");
        }

        var profileDto = new UserProfileDto(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            user.PhoneNumber ?? string.Empty,
            user.Address.Street,
            user.Address.City,
            user.Address.BuildingNumber,
            user.PointsBalance
        );

        // 3. Cache the retrieved profile in Redis (expires in 15 minutes)
        await _cacheService.SetAsync(cacheKey, profileDto, TimeSpan.FromMinutes(15), cancellationToken);

        return new UserProfileResult(true, Profile: profileDto);
    }
}
