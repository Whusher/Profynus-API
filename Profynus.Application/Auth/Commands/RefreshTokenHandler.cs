namespace Profynus.Application.Auth.Commands;

using Microsoft.AspNetCore.Http;
using Profynus.Domain.Auth.Enums;
using Profynus.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Profynus.Application.Common.Exception;
using Profynus.Application.Common.RefreshToken;
using Profynus.Infrastructure.Persistence.Context;
using Profynus.Application.Common.TokenizationService;


// ── DTOs ─────────────────────────────────────────────────────────────────────

/// <summary>
/// For web clients, RefreshToken should be null — it is read from the
/// __Host-rt cookie automatically. Mobile and desktop clients must send
/// the refresh token in the request body.
/// </summary>
public record RefreshRequest(string? RefreshToken);

public record RefreshResponse(
    string AccessToken,
    DateTimeOffset AccessExpiresAt,
    string? RefreshToken);  // null when rotated token is delivered as cookie

// ── Handler ───────────────────────────────────────────────────────────────────

/// <summary>
/// Implements refresh token rotation: every successful refresh invalidates the
/// old token and issues a new pair. This limits the blast radius of a stolen
/// refresh token to one use.
/// </summary>
public class RefreshTokenHandler(
    MasterDbContext db,
    TokenService tokens,
    RefreshTokenDelivery tokenDelivery)
{
    public async Task<RefreshResponse> HandleAsync(
        RefreshRequest request,
        HttpContext http,
        CancellationToken ct = default)
    {
        // 1. Extract refresh token — cookie (web) or body (mobile/desktop)
        var rawToken = ExtractRefreshToken(request, http);
        if (string.IsNullOrWhiteSpace(rawToken))
            throw new UnauthorizedException("Refresh token is required.");

        var tokenHash = tokens.HashToken(rawToken);

        // 2. Load session with device
        var session = await db.Sessions
            .Include(s => s.Device)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == tokenHash, ct);

        if (session == null)
            throw new UnauthorizedException("Invalid refresh token.");

        // 3. Validate session state
        if (!session.IsActive)
            throw new UnauthorizedException("Session has been revoked.");

        if (session.RefreshExpiresAt < DateTimeOffset.UtcNow)
        {
            // Expired — revoke and force re-login
            session.IsActive      = false;
            session.RevokedAt     = DateTimeOffset.UtcNow;
            session.RevokeReason  = "refresh_token_expired";
            await db.SaveChangesAsync(ct);
            throw new UnauthorizedException("Refresh token has expired. Please log in again.");
        }

        if (!session.User.IsActive)
            throw new UnauthorizedException("Account is disabled.");

        // 4. Rotate: revoke old session, issue new token pair
        session.IsActive     = false;
        session.RevokedAt    = DateTimeOffset.UtcNow;
        session.RevokeReason = "refresh_token_rotated";

        var clientType = session.Device.ClientType;
        var newPair    = tokens.Generate(
            session.UserId, Guid.NewGuid(), session.DeviceId, clientType);

        var newSession = new Session()
        {
            UserId           = session.UserId,
            DeviceId         = session.DeviceId,
            AccessTokenHash  = tokens.HashToken(newPair.AccessToken),
            RefreshTokenHash = tokens.HashToken(newPair.RefreshToken),
            AccessExpiresAt  = newPair.AccessExpiresAt,
            RefreshExpiresAt = newPair.RefreshExpiresAt,
            IpAddress        = http.Connection.RemoteIpAddress?.ToString(),
        };
        db.Sessions.Add(newSession);

        // 5. Update device last-seen
        session.Device.LastSeenAt = DateTimeOffset.UtcNow;

        // 6. Audit
        db.AuthEvents.Add(new AuthEvent()
        {
            UserId    = session.UserId,
            SessionId = newSession.Id,
            DeviceId  = session.DeviceId,
            EventType = AuthEventType.TokenRefreshed,
            Status    = EventStatus.Success,
            IpAddress = newSession.IpAddress,
        });

        await db.SaveChangesAsync(ct);

        // 7. Deliver new refresh token
        var delivery = tokenDelivery.Deliver(
            http.Response,
            newPair.RefreshToken,
            newPair.RefreshExpiresAt,
            clientType);

        return new RefreshResponse(
            AccessToken:     newPair.AccessToken,
            AccessExpiresAt: newPair.AccessExpiresAt,
            RefreshToken:    delivery.TokenForBody);
    }

    // ── Token extraction ─────────────────────────────────────────────────

    private static string? ExtractRefreshToken(RefreshRequest request, HttpContext http)
    {
        // Prefer cookie (web path) — HttpOnly cookie is not accessible to JS
        if (http.Request.Cookies.TryGetValue("__Host-rt", out var cookieToken)
            && !string.IsNullOrWhiteSpace(cookieToken))
            return cookieToken;

        // Fallback: subdomain cookie name
        if (http.Request.Cookies.TryGetValue("profynus_rt", out var laxToken)
            && !string.IsNullOrWhiteSpace(laxToken))
            return laxToken;

        // Body: mobile / desktop / Safari fallback
        return request.RefreshToken;
    }
}