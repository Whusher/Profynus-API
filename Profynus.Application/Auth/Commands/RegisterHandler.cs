using Microsoft.AspNetCore.Http;
using Profynus.Domain.Auth.Enums;
using Microsoft.EntityFrameworkCore;
using Profynus.Domain.Auth.Entities;
using Profynus.Application.DTO.Auth;
using Profynus.Application.Common.Helpers;
using Profynus.Application.Common.Exception;
using Profynus.Application.Common.RefreshToken;
using Profynus.Infrastructure.Persistence.Context;
using Profynus.Application.Common.EncryptionService;
using Profynus.Application.Common.TokenizationService;
using Profynus.Application.User;

namespace Profynus.Application.Auth.Commands;

// ── Handler ───────────────────────────────────────────────────────────────────

public class RegisterHandler(
    MasterDbContext db,
    UserService userService,
    PasswordService passwords,
    TokenService tokens,
    DeviceResolver deviceResolver,
    RefreshTokenDelivery tokenDelivery)
{
    public async Task<RegisterResponse> HandleAsync(
        RegisterRequest request,
        HttpContext http,
        CancellationToken ct = default)
    {
        try
        {
            // 1. Validate uniqueness
            var emailNorm = request.Email.Trim().ToLowerInvariant();
            if (await db.Users.AnyAsync(u => u.Email == emailNorm, ct))
                throw new ConflictException("Email already registered.");
            
            // 1.1 Validate username in case of race condition logins
            var availableUsername = await userService.ValidateUsername(request.Username!);
            if (!availableUsername.AvailableUsername)
            {
                throw new ConflictException("Username already taken.");
            }
            
            // 2. Create user
            var user = new Domain.User.Entities.User
            {
                Email = emailNorm,
                Username = request.Username?.Trim(),
                FirstName = request.FirstName?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            db.Users.Add(user);

            // 3. Hash password (Argon2id)
            var credential = new Credential
            {
                User         = user,          // ← navigation property
                PasswordHash = passwords.Hash(request.Password),
                Algorithm    = PwdAlgorithm.Argon2id,
            };
            db.Credentials.Add(credential);

            // 4. Register device
            var deviceCtx = deviceResolver.Resolve(http.Request);
            var device = new Device
            {
                User        = user,           // ← navigation property
                ClientType  = deviceCtx.ClientType,
                Platform    = deviceCtx.Platform,
                Os          = deviceCtx.Os,
                OsVersion   = deviceCtx.OsVersion,
                UaHash      = deviceCtx.UaHash,
                Fingerprint = deviceCtx.Fingerprint,
                Name        = deviceCtx.Name,
            };
            db.Devices.Add(device);

            // 5. Generate token pair
            var tokenPair = tokens.Generate(user.Id, Guid.NewGuid(), device.Id, deviceCtx.ClientType);

            // 6. Session — assign navigations
            var session = new Session
            {
                User             = user,      // ← navigation property
                Device           = device,    // ← navigation property
                AccessTokenHash  = tokens.HashToken(tokenPair.AccessToken),
                RefreshTokenHash = tokens.HashToken(tokenPair.RefreshToken),
                AccessExpiresAt  = tokenPair.AccessExpiresAt,
                RefreshExpiresAt = tokenPair.RefreshExpiresAt,
                IpAddress        = http.Connection.RemoteIpAddress?.ToString(),
            };
            db.Sessions.Add(session);

            // 7. Audit log — assign navigations
            db.AuthEvents.Add(new AuthEvent
            {
                User      = user,             // ← navigation property
                Session   = session,          // ← navigation property
                Device    = device,           // ← navigation property
                EventType = AuthEventType.LoginSuccess,
                Status    = EventStatus.Success,
                IpAddress = session.IpAddress,
                Metadata  = new() { ["action"] = "register" },
            });

            await db.SaveChangesAsync(ct);

            // 8. Deliver refresh token (cookie vs body, platform-aware)  [WAIT UNTIL GET EMAIL VERIFICATION TO ALLOW REGISTER]
            // var delivery = tokenDelivery.Deliver(
            //     http.Response,
            //     tokenPair.RefreshToken,
            //     tokenPair.RefreshExpiresAt,
            //     deviceCtx.ClientType);

            return new RegisterResponse(
                UserId:          user.Id,
                AccessToken:     "",//tokenPair.AccessToken,
                AccessExpiresAt: DateTimeOffset.UtcNow, //tokenPair.AccessExpiresAt,
                RefreshToken:    ""//delivery.TokenForBody
            );
        }
        finally
        {
            // Refresh usernames cache
            await userService.RefreshUsernameCache();
        }
        
    }
}