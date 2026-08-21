
using Microsoft.AspNetCore.Http;
using Profynus.Domain.Auth.Entities;
using Profynus.Domain.Auth.Enums;
using System.Security.Cryptography;
using System.Text;
using UAParser;

namespace Profynus.Application.Common.Helpers;

public record DeviceContext(
    ClientType ClientType,
    string? Platform,
    string? Os,
    string? OsVersion,
    string? UaHash,
    Dictionary<string, object>? Fingerprint,
    string? Name);

/// <summary>
/// Resolves client type from the X-Client-Type header and enriches device
/// metadata from the User-Agent and optional X-Device-* headers sent by
/// mobile and desktop apps.
///
/// Header contract:
///   X-Client-Type : web | mobile | desktop   (required)
///   X-Device-OS   : "iOS 17.4"               (optional, mobile/desktop)
///   X-Device-App  : "1.4.2"                  (optional)
///   X-Device-Name : "Marco's iPhone"         (optional)
/// </summary>
public class DeviceResolver
{
    private static readonly Parser UaParser = Parser.GetDefault();

    public DeviceContext Resolve(HttpRequest request)
    {
        var clientType = ParseClientType(request.Headers["X-Client-Type"]);
        return clientType switch
        {
            ClientType.Web     => ResolveWeb(request),
            ClientType.Mobile  => ResolveMobile(request),
            ClientType.Desktop => ResolveDesktop(request),
            _                  => ResolveWeb(request),
        };
    }

    // ── Per-platform resolution ───────────────────────────────────────────

    private static DeviceContext ResolveWeb(HttpRequest request)
    {
        var ua     = request.Headers.UserAgent.ToString();
        var parsed = UaParser.Parse(ua);
        var uaHash = Sha256Hex(ua);

        return new DeviceContext(
            ClientType: ClientType.Web,
            Platform:   "Web",
            Os:         parsed.OS.Family,
            OsVersion:  parsed.OS.Major,
            UaHash:     uaHash,
            Fingerprint: null,   // canvas fingerprint arrives from JS, not here
            Name:       $"{parsed.UA.Family} on {parsed.OS.Family}");
    }

    private static DeviceContext ResolveMobile(HttpRequest request)
    {
        var osHeader  = request.Headers["X-Device-OS"].ToString();
        var appHeader = request.Headers["X-Device-App"].ToString();
        var ua        = request.Headers.UserAgent.ToString();
        var parsed    = UaParser.Parse(ua);

        // Split "iOS 17.4" → os="iOS", osVersion="17.4"
        var (os, osVersion) = SplitOsHeader(osHeader, parsed.OS.Family, parsed.OS.Major);

        var platform = os switch
        {
            var s when s.Contains("ios", StringComparison.OrdinalIgnoreCase) => "iOS",
            var s when s.Contains("android", StringComparison.OrdinalIgnoreCase) => "Android",
            _ => parsed.OS.Family,
        };

        return new DeviceContext(
            ClientType: ClientType.Mobile,
            Platform:   platform,
            Os:         os,
            OsVersion:  osVersion,
            UaHash:     Sha256Hex(ua),
            Fingerprint: null,
            Name:       request.Headers["X-Device-Name"].ToString().NullIfEmpty()
                        ?? $"{platform} device");
    }

    private static DeviceContext ResolveDesktop(HttpRequest request)
    {
        var osHeader  = request.Headers["X-Device-OS"].ToString();
        var ua        = request.Headers.UserAgent.ToString();
        var parsed    = UaParser.Parse(ua);
        var (os, osVersion) = SplitOsHeader(osHeader, parsed.OS.Family, parsed.OS.Major);

        return new DeviceContext(
            ClientType: ClientType.Desktop,
            Platform:   os,
            Os:         os,
            OsVersion:  osVersion,
            UaHash:     Sha256Hex(ua),
            Fingerprint: null,
            Name:       request.Headers["X-Device-Name"].ToString().NullIfEmpty()
                        ?? $"{os} desktop");
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static ClientType ParseClientType(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "mobile"  => ClientType.Mobile,
            "desktop" => ClientType.Desktop,
            _         => ClientType.Web,
        };

    private static (string os, string? osVersion) SplitOsHeader(
        string header, string fallbackOs, string? fallbackVersion)
    {
        if (string.IsNullOrWhiteSpace(header))
            return (fallbackOs, fallbackVersion);

        var idx = header.LastIndexOf(' ');
        if (idx > 0)
            return (header[..idx].Trim(), header[(idx + 1)..].Trim());

        return (header.Trim(), null);
    }

    private static string Sha256Hex(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

internal static class StringExtensions
{
    public static string? NullIfEmpty(this string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}