using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using EnglishLearning.Domain.Services;
using Microsoft.Extensions.Caching.Memory;

namespace EnglishLearning.Infrastructure.Services;

public sealed class OtpService : IOtpService, IDisposable
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ResendInterval = TimeSpan.FromMinutes(1);
    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 10_000 });
    private readonly TimeProvider _clock;
    private readonly object _gate = new();

    public OtpService(TimeProvider clock) => _clock = clock;

    public OtpDelivery? Begin(OtpIdentity identity)
    {
        var key = (identity.Email.Trim().ToUpperInvariant(), identity.Purpose);
        lock (_gate)
        {
            var now = _clock.GetUtcNow();
            var previous = _cache.Get<Challenge>(key);
            if (previous is not null && previous.LastSent + ResendInterval > now)
                return null;

            if (previous is not null)
                _cache.Remove(previous.Id);

            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
            var challenge = new Challenge(id, identity, now + Lifetime);
            var delivery = IssueCode(challenge, now);
            var options = new MemoryCacheEntryOptions().SetSize(1).SetAbsoluteExpiration(Lifetime);
            _cache.Set(key, challenge, options);
            _cache.Set(id, challenge, options);
            return delivery;
        }
    }

    public OtpDelivery? Resend(string challengeId)
    {
        lock (_gate)
        {
            var now = _clock.GetUtcNow();
            var challenge = Find(challengeId, now);
            if (challenge is null || challenge.LastSent + ResendInterval > now || challenge.Attempts >= 5)
                return null;

            return IssueCode(challenge, now);
        }
    }

    public OtpIdentity? Consume(string challengeId, string code, bool resetPassword = false)
    {
        lock (_gate)
        {
            var challenge = Find(challengeId, _clock.GetUtcNow());
            if (challenge is null || challenge.Attempts >= 5 ||
                (challenge.Identity.Purpose is OtpPurpose.PasswordReset or OtpPurpose.Unavailable) != resetPassword)
                return null;

            challenge.Attempts++;
            if (!CryptographicOperations.FixedTimeEquals(challenge.Hash, Hash(code)) ||
                challenge.Identity.Purpose == OtpPurpose.Unavailable)
                return null;

            _cache.Remove(challenge.Id);
            return challenge.Identity;
        }
    }

    private Challenge? Find(string challengeId, DateTimeOffset now)
    {
        var challenge = _cache.Get<Challenge>(challengeId);
        return challenge?.ExpiresAt > now ? challenge : null;
    }

    private static OtpDelivery IssueCode(Challenge challenge, DateTimeOffset now)
    {
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", CultureInfo.InvariantCulture);
        challenge.Hash = Hash(code);
        challenge.LastSent = now;
        return new OtpDelivery(challenge.Id, code, challenge.Identity);
    }

    private static byte[] Hash(string code) => SHA256.HashData(Encoding.UTF8.GetBytes(code));

    public void Dispose() => _cache.Dispose();

    private sealed class Challenge(string id, OtpIdentity identity, DateTimeOffset expiresAt)
    {
        public string Id { get; } = id;
        public OtpIdentity Identity { get; } = identity;
        public DateTimeOffset ExpiresAt { get; } = expiresAt;
        public DateTimeOffset LastSent { get; set; }
        public int Attempts { get; set; }
        public byte[] Hash { get; set; } = [];
    }
}
