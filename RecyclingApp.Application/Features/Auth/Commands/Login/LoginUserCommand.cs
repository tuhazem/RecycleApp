using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Auth.Commands.Login;

public record LoginUserCommand(string EmailOrPhone, string Password) : IRequest<LoginResult>;

public record LoginResult(
    bool Succeeded,
    string? Token = null,
    string? RefreshToken = null,
    string? Message = null);

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenProvider _tokenProvider;
    private readonly ICacheService _cacheService;

    public LoginUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenProvider tokenProvider,
        ICacheService cacheService)
    {
        _userManager = userManager;
        _tokenProvider = tokenProvider;
        _cacheService = cacheService;
    }

    public async Task<LoginResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Find user by email or phone number
        ApplicationUser? user = null;
        if (request.EmailOrPhone.Contains('@'))
        {
            user = await _userManager.FindByEmailAsync(request.EmailOrPhone);
        }
        else
        {
            // Use EF Core extension to query phone number since UserManager doesn't have FindByPhoneNumber
            user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.EmailOrPhone, cancellationToken);
        }

        if (user == null)
        {
            return new LoginResult(false, Message: "Invalid email/phone or password.");
        }

        // 2. Validate password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return new LoginResult(false, Message: "Invalid email/phone or password.");
        }

        // 3. Generate JWT Access Token and Refresh Token
        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenProvider.GenerateJwtToken(user, roles);
        var refreshToken = _tokenProvider.GenerateRefreshToken();

        // 4. Store Refresh Token in Cache with 7-day expiration
        var refreshTokenKey = $"refresh-token-{refreshToken}";
        await _cacheService.SetAsync(refreshTokenKey, user.Id, TimeSpan.FromDays(7), cancellationToken);

        return new LoginResult(
            Succeeded: true,
            Token: token,
            RefreshToken: refreshToken,
            Message: "Login successful"
        );
    }
}

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.EmailOrPhone)
            .NotEmpty().WithMessage("Email or Phone Number is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
