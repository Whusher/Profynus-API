namespace Profynus.Application.Auth.Commands;

using Microsoft.AspNetCore.Http;
using Profynus.Domain.Auth.Enums;
using Profynus.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Profynus.Application.Common.Helpers;
using Profynus.Application.Common.Exception;
using Profynus.Application.Common.RefreshToken;
using Profynus.Infrastructure.Persistence.Context;
using Profynus.Application.Common.EncryptionService;
using Profynus.Application.Common.TokenizationService;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record LoginRequest(
    string Email,
    string Password);

public record LoginResponse(
    Guid   UserId,
    string AccessToken,
    DateTimeOffset AccessExpiresAt,
    string? RefreshToken,           // null when delivered as cookie
    bool RequiresMfaChallenge,
    // User feedback
    string? Message = null, // Message to include additional information of the error if something went wrong
    bool Success = true // Operation result
);

// ── Handler ───────────────────────────────────────────────────────────────────

public class LoginHandler(
    MasterDbContext db,
    PasswordService passwords,
    TokenService tokens,
    DeviceResolver deviceResolver,
    RefreshTokenDelivery tokenDelivery)
{
    // Lockout policy: lock after 5 failures for 15 minutes.
    private const int  MaxFailures   = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public async Task<LoginResponse> HandleAsync(
        LoginRequest request,
        HttpContext http,
        CancellationToken ct = default)
    {
        var ip        = http.Connection.RemoteIpAddress?.ToString();
        var emailNorm = request.Email.Trim().ToLowerInvariant();

        // 1. Load user + credential in one query
        var user = await db.Users
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u => u.Email == emailNorm, ct);

        // 2. Unknown user — record attempt, return generic error (prevent enumeration)
        if (user == null)
        {
            await RecordFailedAttemptAnonymousAsync(emailNorm, ip, ct);
            throw new UnauthorizedException("Invalid credentials.");
        }

        // 3. Account checks
        if (!user.IsActive)
            throw new UnauthorizedException("Your account is disabled, Please verify your email.");

        if (user.IsLocked && user.LockedUntil > DateTimeOffset.UtcNow)
        {
            await AppendAuditAsync(user.Id, null, null, AuthEventType.LoginFailed,
                EventStatus.Failure, ip, "Account is locked.", ct);
            throw new UnauthorizedException(
                $"Account locked until {user.LockedUntil:u}. Try again later.");
        }

        // Auto-unlock if lockout window has passed
        if (user.IsLocked && user.LockedUntil <= DateTimeOffset.UtcNow)
        {
            user.IsLocked           = false;
            user.FailedLoginCount   = 0;
            await AppendAuditAsync(user.Id, null, null,
                AuthEventType.AccountUnlocked, EventStatus.Success, ip, null, ct);
        }

        // 4. Verify password
        var credential = user.Credential
            ?? throw new UnauthorizedException("Invalid credentials.");

        if (!passwords.Verify(request.Password, credential.PasswordHash))
        {
            user.FailedLoginCount++;

            if (user.FailedLoginCount >= MaxFailures)
            {
                user.IsLocked    = true;
                user.LockedUntil = DateTimeOffset.UtcNow.Add(LockoutDuration);
                await AppendAuditAsync(user.Id, null, null,
                    AuthEventType.AccountLocked, EventStatus.Failure, ip,
                    $"Locked after {MaxFailures} failed attempts.", ct);
            }

            await AppendAuditAsync(user.Id, null, null,
                AuthEventType.LoginFailed, EventStatus.Failure, ip,
                $"Bad password (attempt {user.FailedLoginCount}).", ct);

            await db.SaveChangesAsync(ct);
            throw new UnauthorizedException("Invalid credentials.");
        }

        // 5. Reset failure counter on success
        user.FailedLoginCount = 0;
        user.IsLocked         = false;
        user.LockedUntil      = null;

        // 6. Upsert device (find existing by UA hash, or create new)
        var deviceCtx = deviceResolver.Resolve(http.Request);
        var device    = await UpsertDeviceAsync(user.Id, deviceCtx, ct);

        // 7. Check MFA requirement
        //    True if user has a verified primary MFA config AND device is not trusted.
        var requiresMfa = await db.MfaConfigs
            .AnyAsync(m => m.UserId == user.Id && m.IsPrimary && m.IsVerified, ct)
            && !device.IsTrusted;

        // 8. Generate token pair
        var sessionId = Guid.NewGuid();
        var tokenPair = tokens.Generate(user.Id, sessionId, device.Id, deviceCtx.ClientType);

        // 9. Persist session
        var session = new Session
        {
            Id                = sessionId,
            UserId            = user.Id,
            DeviceId          = device.Id,
            AccessTokenHash   = tokens.HashToken(tokenPair.AccessToken),
            RefreshTokenHash  = tokens.HashToken(tokenPair.RefreshToken),
            AccessExpiresAt   = tokenPair.AccessExpiresAt,
            RefreshExpiresAt  = tokenPair.RefreshExpiresAt,
            IpAddress         = ip,
            // If MFA is required, immediately revoke until challenge passes
            IsActive          = !requiresMfa,
        };
        db.Sessions.Add(session);

        // 10. Audit success
        await AppendAuditAsync(user.Id, session.Id, device.Id,
            AuthEventType.LoginSuccess, EventStatus.Success, ip, null, ct);

        if (requiresMfa)
            await AppendAuditAsync(user.Id, session.Id, device.Id,
                AuthEventType.MfaChallenged, EventStatus.Pending, ip, null, ct);

        await db.SaveChangesAsync(ct);

        // 11. Deliver refresh token
        //     If MFA challenge is pending, we return a short-lived pre-auth token
        //     instead of the real refresh token. The real one is issued after MFA passes.
        //     For simplicity here, we omit the pre-auth token and let the client poll
        //     the /mfa/verify endpoint with the session ID.
        RefreshTokenDelivery.DeliveryResult? delivery = null;
        if (!requiresMfa)
        {
            delivery = tokenDelivery.Deliver(
                http.Response,
                tokenPair.RefreshToken,
                tokenPair.RefreshExpiresAt,
                deviceCtx.ClientType);
        }

        return new LoginResponse(
            UserId:                user.Id,
            AccessToken:           requiresMfa ? string.Empty : tokenPair.AccessToken,
            AccessExpiresAt:       tokenPair.AccessExpiresAt,
            RefreshToken:          delivery?.TokenForBody,
            RequiresMfaChallenge:  requiresMfa);
    }

    // ── Device upsert ─────────────────────────────────────────────────────

    private async Task<Device> UpsertDeviceAsync(
        Guid userId, DeviceContext ctx, CancellationToken ct)
    {
        // Match by UA hash for web, or by Platform+OS for mobile/desktop
        Device? existing = ctx.UaHash != null
            ? await db.Devices.FirstOrDefaultAsync(
                d => d.UserId == userId && d.UaHash == ctx.UaHash && d.IsActive, ct)
            : await db.Devices.FirstOrDefaultAsync(
                d => d.UserId == userId
                  && d.ClientType == ctx.ClientType
                  && d.Os == ctx.Os
                  && d.OsVersion == ctx.OsVersion
                  && d.IsActive, ct);

        if (existing != null)
        {
            existing.LastSeenAt  = DateTimeOffset.UtcNow;
            existing.AppVersion  = ctx.OsVersion; // update app version on each login
            return existing;
        }

        var device = new Device
        {
            UserId      = userId,
            ClientType  = ctx.ClientType,
            Platform    = ctx.Platform,
            Os          = ctx.Os,
            OsVersion   = ctx.OsVersion,
            UaHash      = ctx.UaHash,
            Fingerprint = ctx.Fingerprint,
            Name        = ctx.Name,
        };
        db.Devices.Add(device);

        await AppendAuditAsync(userId, null, device.Id,
            AuthEventType.DeviceRegistered, EventStatus.Success, null, null, ct);

        return device;
    }

    // ── Audit helpers ─────────────────────────────────────────────────────

    private async Task AppendAuditAsync(
        Guid? userId, Guid? sessionId, Guid? deviceId,
        AuthEventType eventType, EventStatus status,
        string? ip, string? failureReason,
        CancellationToken ct)
    {
        db.AuthEvents.Add(new AuthEvent
        {
            UserId        = userId,
            SessionId     = sessionId,
            DeviceId      = deviceId,
            EventType     = eventType,
            Status        = status,
            IpAddress     = ip,
            FailureReason = failureReason,
        });
        // Don't SaveChanges here — caller batches all writes
    }

    private async Task RecordFailedAttemptAnonymousAsync(
        string email, string? ip, CancellationToken ct)
    {
        db.AuthEvents.Add(new AuthEvent
        {
            EventType     = AuthEventType.LoginFailed,
            Status        = EventStatus.Failure,
            IpAddress     = ip,
            FailureReason = "Unknown email.",
            Metadata      = new() { ["attempted_email"] = email },
        });
        await db.SaveChangesAsync(ct);
    }
}