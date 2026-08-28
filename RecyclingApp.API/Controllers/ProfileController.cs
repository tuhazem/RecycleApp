using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecyclingApp.Application.Common.Models;
using RecyclingApp.Application.Features.Profile.Commands.UpdateProfile;
using RecyclingApp.Application.Features.Profile.DTOs;
using RecyclingApp.Application.Features.Profile.Queries.GetProfile;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RecyclingApp.API.Controllers;

/// <summary>
/// Controller for authenticated User Profile Management (Get, Update).
/// </summary>
[Authorize]
[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets the profile of the currently authenticated user.
    /// Accessible via GET /api/profile and GET /api/users/me.
    /// </summary>
    [HttpGet]
    [HttpGet("/api/users/me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(Result.Failure("User ID claim not found in JWT token."));
        }

        var result = await _mediator.Send(new GetUserProfileQuery(userId));
        if (!result.Succeeded)
        {
            return NotFound(result);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Updates the profile of the currently authenticated user.
    /// Strictly guarantees users can only modify their own profile.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(UpdateProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(Result.Failure("User ID claim not found in JWT token."));
        }

        var command = new UpdateUserProfileCommand(
            userId,
            request.Username,
            request.Email,
            request.Phone,
            request.Address
        );

        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }
}

/// <summary>
/// Request contract for updating a user profile.
/// </summary>
public record UpdateProfileDto(
    string Username,
    string Email,
    string Phone,
    string Address);
