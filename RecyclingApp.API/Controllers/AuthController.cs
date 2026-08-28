using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RecyclingApp.Application.Common.Models;
using RecyclingApp.Application.Features.Auth.Commands.ForgotPassword;
using RecyclingApp.Application.Features.Auth.Commands.Login;
using RecyclingApp.Application.Features.Auth.Commands.Logout;
using RecyclingApp.Application.Features.Auth.Commands.Register;
using RecyclingApp.Application.Features.Auth.Commands.ResetPassword;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RecyclingApp.API.Controllers;

/// <summary>
/// Controller for Authentication endpoints: Register, Login, Logout, Forgot Password, and Reset Password.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return Unauthorized(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Logs out the authenticated user by invalidating the active JWT token in Redis and clearing session caches.
    /// No request body is required — user identity and access token are extracted directly from the Bearer header.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(LogoutResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] LogoutRequestDto? request = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(Result.Failure("User ID claim not found in JWT token."));
        }

        var authHeader = Request.Headers["Authorization"].ToString();
        var accessToken = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader["Bearer ".Length..].Trim()
            : null;

        var command = new LogoutUserCommand(userId, accessToken, request?.RefreshToken);
        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(new LogoutResponseDto(result.Data?.Message ?? "Logged out successfully"));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}

public record LogoutRequestDto(string? RefreshToken = null);
public record LogoutResponseDto(string Message);
