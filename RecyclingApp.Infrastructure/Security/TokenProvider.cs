using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RecyclingApp.Infrastructure.Security;

/// <summary>
/// Implementation of ITokenProvider for generating JSON Web Tokens.
/// </summary>
public class TokenProvider : ITokenProvider
{
    private readonly IConfiguration _configuration;

    public TokenProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var secretKey = _configuration["Jwt:Secret"] ?? "DefaultSuperSecretKeyForDevelopmentOnly1234567890!";
        var issuer = _configuration["Jwt:Issuer"] ?? "RecyclingApp";
        var audience = _configuration["Jwt:Audience"] ?? "RecyclingAppUsers";
        var expiryMinutes = double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "120");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.FullName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
