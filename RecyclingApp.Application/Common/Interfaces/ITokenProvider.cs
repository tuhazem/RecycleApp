using RecyclingApp.Domain.Entities;
using System.Collections.Generic;

namespace RecyclingApp.Application.Common.Interfaces;

/// <summary>
/// Service interface for generating JSON Web Tokens (JWT) and Refresh Tokens.
/// </summary>
public interface ITokenProvider
{
    string GenerateJwtToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
}
