namespace Profynus.Domain.Auth.Enums;

public enum ClientType { Web, Mobile, Desktop }
public enum MfaMethod { Totp, WebAuthn, Sms, Email }
public enum OAuthProviderName { Google, Github, Microsoft, Apple }
public enum PwdAlgorithm { Bcrypt, Argon2id, Scrypt }

public enum AuthEventType
{
    LoginSuccess, LoginFailed, Logout,
    TokenRefreshed, TokenRevoked,
    MfaChallenged, MfaPassed, MfaFailed,
    DeviceRegistered, DeviceRevoked, DeviceTrusted,
    PasswordResetRequested, PasswordResetCompleted,
    PasswordChanged, EmailVerificationSent, 
    EmailVerified, AccountLocked, AccountUnlocked
}
public enum EventStatus { Success, Failure, Pending }