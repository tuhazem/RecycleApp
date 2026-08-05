using RecyclingApp.Domain.Entities;

namespace RecyclingApp.Application.Common.Interfaces;

/// <summary>
/// Service interface for generating JSON Web Tokens (JWT).
/// </summary>
public interface ITokenProvider
{
    string GenerateJwtToken(ApplicationUser user);
}
