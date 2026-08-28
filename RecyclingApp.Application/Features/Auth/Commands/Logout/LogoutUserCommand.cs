using FluentValidation;
using MediatR;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Common.Models;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Auth.Commands.Logout;

/// <summary>
/// CQRS Command to log out the authenticated user, revoking tokens and clearing active sessions.
/// </summary>
public record LogoutUserCommand(
    string UserId,
    string? AccessToken = null,
    string? RefreshToken = null) : IRequest<Result<LogoutResponse>>;

public record LogoutResponse(string Message);

public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, Result<LogoutResponse>>
{
    private readonly ICacheService _cacheService;

    public LogoutUserCommandHandler(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<Result<LogoutResponse>> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Blacklist the active access token in Redis cache (with 2-hour TTL to match token lifetime)
        if (!string.IsNullOrWhiteSpace(request.AccessToken))
        {
            var tokenHash = ComputeSha256Hash(request.AccessToken);
            var blacklistKey = $"blacklisted-token-{tokenHash}";
            await _cacheService.SetAsync(blacklistKey, "revoked", TimeSpan.FromHours(2), cancellationToken);
        }

        // 2. Revoke Refresh Token if provided
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var refreshTokenKey = $"refresh-token-{request.RefreshToken}";
            await _cacheService.RemoveAsync(refreshTokenKey, cancellationToken);
        }

        // 3. Invalidate cached user profile & user session
        var userProfileCacheKey = $"user-profile-{request.UserId}";
        await _cacheService.RemoveAsync(userProfileCacheKey, cancellationToken);

        var userSessionKey = $"user-session-{request.UserId}";
        await _cacheService.RemoveAsync(userSessionKey, cancellationToken);

        return Result<LogoutResponse>.Success(
            new LogoutResponse("Logged out successfully"),
            "Logged out successfully"
        );
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes);
    }
}

public class LogoutUserCommandValidator : AbstractValidator<LogoutUserCommand>
{
    public LogoutUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required for logout.");
    }
}
