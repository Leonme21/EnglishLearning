namespace EnglishLearning.Domain.Services;

public enum OtpPurpose { Login, Registration, PasswordReset, Unavailable }

public sealed record OtpIdentity(string Email, OtpPurpose Purpose, int UserId = 0,
    string? Name = null, string? PasswordHash = null);

public sealed record OtpDelivery(string ChallengeId, string Code, OtpIdentity Identity);

public interface IOtpService
{
    OtpDelivery? Begin(OtpIdentity identity);
    OtpDelivery? Resend(string challengeId);
    OtpIdentity? Consume(string challengeId, string code, bool resetPassword = false);
}
