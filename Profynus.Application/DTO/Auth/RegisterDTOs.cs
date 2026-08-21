namespace Profynus.Application.DTO.Auth;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string? Username);

public record RegisterResponse(
    Guid   UserId,
    string AccessToken,
    DateTimeOffset AccessExpiresAt,
    // RefreshToken is only populated when NOT delivered as a cookie
    // (i.e. mobile, desktop, or Safari cross-origin web).
    string? RefreshToken,

    // User feedback
    string? Message = null, // Message to include additional information of the error if something went wrong
    bool Success = true // Operation result
);