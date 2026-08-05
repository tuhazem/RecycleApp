using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecyclingApp.Application.Features.Profile.Queries.GetProfile;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RecyclingApp.API.Controllers;

/// <summary>
/// Controller for Profile endpoints. Protected with Authorize attribute.
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

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        // Extract the NameIdentifier (UserId) claim from the JWT token
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Message = "User ID claim not found in JWT." });
        }

        var result = await _mediator.Send(new GetUserProfileQuery(userId));
        if (!result.Succeeded)
        {
            return NotFound(result);
        }

        return Ok(result.Profile);
    }
}
