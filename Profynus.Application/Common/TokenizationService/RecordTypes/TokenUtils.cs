namespace Profynus.Application.Common.TokenizationService.RecordTypes;

public record TokenPair(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessExpiresAt,
    DateTimeOffset RefreshExpiresAt);

public record TokenConfig(
    string SecretKey,
    string Issuer,
    string Audience);
