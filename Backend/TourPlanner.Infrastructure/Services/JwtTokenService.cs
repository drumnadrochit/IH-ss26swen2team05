using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TourPlanner.Application.Contracts.Security;
using TourPlanner.Application.Contracts.Time;
using TourPlanner.Infrastructure.Options;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Infrastructure.Services;

public sealed class JwtTokenService(IOptions<JwtOptions> options, IClock clock) : ITokenService
{
    public Task<TokenPair> GenerateTokenPairAsync(User user, CancellationToken cancellationToken = default)
    {
        var jwtOptions = options.Value;
        if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
        {
            throw new InvalidOperationException("Jwt:SigningKey is not configured.");
        }

        var accessTokenExpiresAt = clock.UtcNow.AddMinutes(jwtOptions.AccessTokenMinutes);
        var refreshTokenExpiresAt = clock.UtcNow.AddDays(jwtOptions.RefreshTokenDays);
        Console.WriteLine($"Generating JWT token for user {user.UserName} with expiration at {accessTokenExpiresAt.UtcDateTime} (UTC).");
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            notBefore: clock.UtcNow.UtcDateTime,
            expires: accessTokenExpiresAt.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = tokenHandler.WriteToken(token);
        var refreshToken = GenerateRefreshToken();
        var pair = new TokenPair(accessToken, refreshTokenExpiresAt, refreshToken, accessTokenExpiresAt);
        return Task.FromResult(pair);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(48);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}

