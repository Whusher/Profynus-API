using Microsoft.AspNetCore.Http;
using Profynus.Domain.Auth.Enums;

namespace Profynus.Application.Common.RefreshToken;

/// <summary>
/// Encapsulates the platform-aware refresh token delivery strategy.
///
/// The problem:
///   Safari on iOS 16.4+ and macOS Ventura+ enforces ITP (Intelligent Tracking
///   Prevention). When a third-party context is involved, Safari either blocks
///   Set-Cookie entirely or requires the Partitioned attribute (CHIPS).
///   However, CHIPS (Cookies Having Independent Partitioned State) is a Chrome
///   proposal — Safari does NOT support the __Host- prefix + Partitioned
///   combination in the same way Chrome does.
///
///   In practice for a first-party auth endpoint (same domain), SameSite=Strict
///   + Secure + HttpOnly works fine on Safari. The headaches arise when:
///     1. Your auth API is on a different subdomain (auth.profynus.com) from
///        the app (app.profynus.com) → SameSite=Lax is needed, still works.
///     2. The API is cross-origin (different eTLD+1) → cookies are blocked on
///        Safari regardless. Fall back to body delivery.
///
/// Strategy implemented here:
///   Web (same-origin or subdomain):
///     → HttpOnly, Secure, SameSite=Strict cookie named "__Host-rt"
///       The __Host- prefix enforces: Secure, no Domain attribute, Path=/.
///       This is the strongest cookie security posture available.
///
///   Web (cross-origin / detected Safari ITP issue):
///     → We detect via User-Agent whether the browser is Safari on iOS/macOS.
///       If so, and if the SameSite context would be cross-site, we fall back
///       to returning the refresh token in the response body and let the client
///       store it in memory (not localStorage — that's XSS-accessible).
///       Clients should use the Authorization header on subsequent calls.
///
///   Mobile (iOS / Android):
///     → Refresh token in response body. Native apps manage their own secure
///        storage (Keychain on iOS, Keystore on Android). Never use cookies.
///
///   Desktop (.NET WPF/WinUI):
///     → Refresh token in response body. App uses Windows Credential Manager
///        or DPAPI to persist the token.
/// </summary>
public class RefreshTokenDelivery
{
    private const string CookieName    = "__Host-rt";
    private const string LaxCookieName = "profynus_rt"; // fallback when __Host- not usable

    public record DeliveryResult(
        bool   SentAsCookie,
        string? TokenForBody);   // null when delivered via cookie

    /// <summary>
    /// Sets the refresh token cookie (web) or returns the token for body
    /// inclusion (mobile/desktop/Safari-cross-origin).
    /// </summary>
    public DeliveryResult Deliver(
        HttpResponse response,
        string refreshToken,
        DateTimeOffset expiresAt,
        ClientType clientType,
        bool isCrossOrigin = true)
    {
        if (clientType != ClientType.Web)
        {
            // Mobile and Desktop: caller embeds token in response body
            return new DeliveryResult(SentAsCookie: false, TokenForBody: refreshToken);
        }

        // Web — detect Safari ITP scenarios
        var ua = response.HttpContext.Request.Headers.UserAgent.ToString();
        if (isCrossOrigin && IsSafariIos(ua))
        {
            // Cross-origin + Safari: cookies won't stick.
            // Return token in body; JS client keeps it in memory only.
            return new DeliveryResult(SentAsCookie: false, TokenForBody: refreshToken);
        }

        SetWebCookie(response, refreshToken, expiresAt, isCrossOrigin);
        return new DeliveryResult(SentAsCookie: true, TokenForBody: null);
    }

    public void ClearCookie(HttpResponse response, bool isCrossOrigin = false)
    {
        var name = isCrossOrigin ? LaxCookieName : CookieName;
        response.Cookies.Delete(name, new CookieOptions
        {
            Path     = "/",
            Secure   = true,
            HttpOnly = true,
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static void SetWebCookie(
        HttpResponse response,
        string refreshToken,
        DateTimeOffset expiresAt,
        bool isCrossOrigin)
    {
        if (!isCrossOrigin)
        {
            // Same-origin: use __Host- prefix for maximum security.
            // __Host- requires: Secure, no Domain, Path=/
            // SameSite=Strict prevents CSRF entirely.
            response.Cookies.Append(CookieName, refreshToken, new CookieOptions
            {
                HttpOnly  = true,
                Secure    = true,
                SameSite  = SameSiteMode.Strict,
                Expires   = expiresAt,
                Path      = "/",
                // Do NOT set Domain — required by __Host- prefix semantics.
            });
        }
        else
        {
            // Subdomain scenario (auth.profynus.com → app.profynus.com):
            // Must use SameSite=Lax and set Domain for subdomain sharing.
            // Cannot use __Host- prefix here. Named "profynus_rt" instead.
            // Add Partitioned for Chrome CHIPS support (Safari ignores it safely).  [CHANGE WHEN LOGIC OF SESSION AND SERVICES ARE AVAILABLE]
            response.Cookies.Append(LaxCookieName, refreshToken, new CookieOptions
            {
                HttpOnly   = true,
                Secure     = true,
                SameSite   = SameSiteMode.None,
                Expires    = expiresAt,
                Path       = "/",
                // Domain     = "vps-master.duckdns.org",
                // Extensions["Partitioned"] = "" would go here for Chrome CHIPS
                // ASP.NET Core 8 doesn't expose this natively yet; use middleware.
            });
        }
    }

    /// <summary>
    /// Detects Safari on iOS / iPadOS / macOS.
    /// Heuristic: UA contains "Safari" but not "Chrome" or "Chromium"
    /// (Chrome on iOS still reports Safari in UA).
    /// </summary>
    private static bool IsSafariIos(string userAgent) =>
        userAgent.Contains("Safari", StringComparison.OrdinalIgnoreCase)
        && !userAgent.Contains("Chrome",   StringComparison.OrdinalIgnoreCase)
        && !userAgent.Contains("Chromium", StringComparison.OrdinalIgnoreCase)
        && !userAgent.Contains("Edg",      StringComparison.OrdinalIgnoreCase)
        && (userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase)
         || userAgent.Contains("iPad",   StringComparison.OrdinalIgnoreCase)
         || userAgent.Contains("Macintosh", StringComparison.OrdinalIgnoreCase));
}