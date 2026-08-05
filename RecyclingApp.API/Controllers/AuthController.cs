using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecyclingApp.Application.Features.Auth.Commands.ForgotPassword;
using RecyclingApp.Application.Features.Auth.Commands.Login;
using RecyclingApp.Application.Features.Auth.Commands.Register;
using RecyclingApp.Application.Features.Auth.Commands.ResetPassword;
using System.Threading.Tasks;

namespace RecyclingApp.API.Controllers;

/// <summary>
/// Controller for Authentication endpoints: Register, Login, Forgot Password, and Reset Password.
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
