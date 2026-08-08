using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RecyclingApp.Application.Features.Auth.Commands.Login;

public record LoginUserCommand(string EmailOrPhone, string Password) : IRequest<LoginResult>;

public record LoginResult(bool Succeeded, string? Token = null, string? Message = null);

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenProvider _tokenProvider;

    public LoginUserCommandHandler(UserManager<ApplicationUser> userManager, ITokenProvider tokenProvider)
    {
        _userManager = userManager;
        _tokenProvider = tokenProvider;
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

        // 3. Generate JWT Token
        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenProvider.GenerateJwtToken(user, roles);
        return new LoginResult(true, Token: token);
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
