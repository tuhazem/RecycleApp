using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace RecyclingApp.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<ForgotPasswordResult>;

public record ForgotPasswordResult(bool Succeeded, string? Message = null);

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(UserManager<ApplicationUser> userManager, IEmailSender emailSender, ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<ForgotPasswordResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // For security, return success to prevent email enumeration
            return new ForgotPasswordResult(true, Message: "If the email is registered, a password reset link has been sent.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        // In a real application, you would link to a frontend UI reset page with query parameters
        var resetLink = $"https://recyclingapp.com/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
        _logger.LogInformation("Password reset token generated for user {Email}. Reset Link: {ResetLink}", user.Email, resetLink);
        
        var emailBody = $@"
            <h3>Reset Your Password</h3>
            <p>Hello {user.FullName},</p>
            <p>You requested a password reset. Click the link below to set a new password:</p>
            <p><a href='{resetLink}'>{resetLink}</a></p>
            <p>If you did not request this, please ignore this email.</p>";

        await _emailSender.SendEmailAsync(user.Email!, "Reset Your Password - Recycling App", emailBody);

        return new ForgotPasswordResult(true, Message: "Password reset link has been sent to your email.");
    }
}

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}
