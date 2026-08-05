using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using RecyclingApp.Domain.Entities;
using RecyclingApp.Domain.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Auth.Commands.Register;

public record RegisterUserCommand(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password,
    string Street,
    string City,
    string BuildingNumber) : IRequest<RegisterResult>;

public record RegisterResult(bool Succeeded, string? Message, string? UserId = null);

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterResult>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<RegisterResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new RegisterResult(false, "Email is already registered.");
        }

        Address address;
        try
        {
            address = new Address(request.Street, request.City, request.BuildingNumber);
        }
        catch (ArgumentException ex)
        {
            return new RegisterResult(false, $"Invalid address data: {ex.Message}");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FullName = request.FullName,
            Address = address
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return new RegisterResult(false, $"Registration failed: {errors}");
        }

        return new RegisterResult(true, "User registered successfully.", user.Id);
    }
}

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required.")
            .MaximumLength(100).WithMessage("Full Name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone Number is required.")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone Number must be a valid international phone format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.");

        RuleFor(x => x.BuildingNumber)
            .NotEmpty().WithMessage("Building Number is required.");
    }
}
