using EnglishLearning.Domain.Services;
using EnglishLearning.Infrastructure.Services;

namespace EnglishLearning.Tests.Auth;

public sealed class OtpAuthTests
{
    [Fact]
    public void CodeCanBeConsumedOnlyOnce()
    {
        using var otp = new OtpService(TimeProvider.System);
        var identity = new OtpIdentity("student@example.test", OtpPurpose.Login, 42);
        var challenge = Assert.IsType<OtpDelivery>(otp.Begin(identity));
        Assert.Matches("^[0-9]{6}$", challenge.Code);
        Assert.Equal(identity, otp.Consume(challenge.ChallengeId, challenge.Code));
        Assert.Null(otp.Consume(challenge.ChallengeId, challenge.Code));
        Assert.Null(otp.Resend(challenge.ChallengeId));
    }

    [Fact]
    public void ChallengeExpiresAndThrottlesDelivery()
    {
        var clock = new AdjustableTime();
        using var otp = new OtpService(clock);
        var identity = new OtpIdentity("Student@example.test", OtpPurpose.Registration);
        var initial = Assert.IsType<OtpDelivery>(otp.Begin(identity));

        Assert.Null(otp.Begin(identity with { Email = "student@example.test" }));
        Assert.Null(otp.Resend(initial.ChallengeId));
        clock.Move(TimeSpan.FromMinutes(1));
        Assert.NotNull(otp.Resend(initial.ChallengeId));
        clock.Move(TimeSpan.FromMinutes(4));
        Assert.Null(otp.Consume(initial.ChallengeId, initial.Code));
        Assert.Null(otp.Resend(initial.ChallengeId));
    }

    [Fact]
    public void ResetCodesCannotOpenSessionsAndAttemptsAreLimited()
    {
        using var otp = new OtpService(TimeProvider.System);
        var challenge = Assert.IsType<OtpDelivery>(otp.Begin(new("student@example.test", OtpPurpose.PasswordReset)));
        Assert.Null(otp.Consume(challenge.ChallengeId, challenge.Code));
        for (var attempt = 0; attempt < 5; attempt++)
            Assert.Null(otp.Consume(challenge.ChallengeId, "xxxxxx", resetPassword: true));
        Assert.Null(otp.Consume(challenge.ChallengeId, challenge.Code, resetPassword: true));
    }

    [Fact]
    public async Task ConcurrentVerificationsConsumeOnlyOneCode()
    {
        using var otp = new OtpService(TimeProvider.System);
        var challenge = Assert.IsType<OtpDelivery>(otp.Begin(new("student@example.test", OtpPurpose.Login)));
        var verifications = Enumerable.Range(0, 5).Select(_ =>
            Task.Run(() => otp.Consume(challenge.ChallengeId, challenge.Code)));
        Assert.Single((await Task.WhenAll(verifications)).OfType<OtpIdentity>());
    }

    private sealed class AdjustableTime : TimeProvider
    {
        private DateTimeOffset _now = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => _now;
        public void Move(TimeSpan elapsed) => _now += elapsed;
    }
}
