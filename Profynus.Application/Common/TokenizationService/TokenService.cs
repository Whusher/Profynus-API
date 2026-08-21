// Third Party Libraries
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

// Local dependencies
using System.Text;
using Profynus.Domain.Auth.Enums;
using Profynus.Application.Common.TokenizationService.RecordTypes;

namespace Profynus.Application.Common.TokenizationService;
/// <summary>
/// Generates JWT access and opaque refresh tokens.
/// Refresh token lifetimes are differenciated by ClientType:
/// - Web   -> 7 days
/// - Desktop -> 30 days
/// - Mobile  -> 90 days
/// </summary>
/// <param name="config"></param>
public class TokenService (TokenConfig config)
{
    private static readonly TimeSpan AccessTtl = TimeSpan.FromMinutes(15);
    
    private static readonly Dictionary<ClientType, TimeSpan> RefreshTtl = new()
    {
        [ClientType.Web]     = TimeSpan.FromDays(7),
        [ClientType.Mobile]  = TimeSpan.FromDays(90),
        [ClientType.Desktop] = TimeSpan.FromDays(30),
    };
 
    public TokenPair Generate(Guid userId, Guid sessionId, Guid deviceId, ClientType clientType)
    {
        var now = DateTimeOffset.UtcNow;
        var accessExpiresAt  = now.Add(AccessTtl);
        var refreshExpiresAt = now.Add(RefreshTtl[clientType]);
 
        var accessToken  = BuildJwt(userId, sessionId, deviceId, clientType, accessExpiresAt);
        var refreshToken = BuildRefreshToken();
 
        return new TokenPair(accessToken, refreshToken, accessExpiresAt, refreshExpiresAt);
    }
 
    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
 
    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SecretKey));
        var handler = new JwtSecurityTokenHandler();
        try
        {
            return handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = config.Issuer,
                ValidateAudience = true,
                ValidAudience = config.Audience,
                ClockSkew = TimeSpan.Zero,
            }, out _);
        }
        catch { return null; }
    }
 
    // ── Helpers ───────────────────────────────────────────────────────────
 
    private string BuildJwt(Guid userId, Guid sessionId, Guid deviceId,
        ClientType clientType, DateTimeOffset expiresAt)
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SecretKey));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims  = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("sid", sessionId.ToString()),
            new Claim("did", deviceId.ToString()),
            new Claim("client_type", clientType.ToString().ToLower()),
        };
 
        var token = new JwtSecurityToken(
            issuer:   config.Issuer,
            audience: config.Audience,
            claims:   claims,
            notBefore: DateTime.UtcNow,
            expires:   expiresAt.UtcDateTime,
            signingCredentials: creds);
 
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
 
    /// <summary>
    /// Cryptographically secure 64-byte opaque token (URL-safe base64).
    /// </summary>
    /// <returns></returns>
    private static string BuildRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

}