using EnglishLearning.Domain.Common;

namespace EnglishLearning.Domain.ValueObjects;

public readonly record struct Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation("Email.Empty", "Email cannot be empty");

        var trimmed = value.Trim();
        if (!trimmed.Contains('@') || !trimmed.Contains('.'))
            return Error.Validation("Email.Invalid", "Email format is invalid");

        if (trimmed.Length > 150)
            return Error.Validation("Email.TooLong", "Email cannot exceed 150 characters");

        return new Email(trimmed.ToLowerInvariant());
    }

    public static implicit operator string(Email email) => email.Value;
    public override string ToString() => Value;
}

public readonly record struct Percentage
{
    public double Value { get; }

    private Percentage(double value) => Value = value;

    public static Result<Percentage> Create(double value)
    {
        if (value < 0 || value > 100)
            return Error.Validation("Percentage.OutOfRange", "Percentage must be between 0 and 100");
        return new Percentage(Math.Round(value, 1));
    }

    public static Percentage Zero => new(0);
    public static Percentage Hundred => new(100);

    public static implicit operator double(Percentage p) => p.Value;
    public static implicit operator Percentage(double d) => Create(d).GetValueOrThrow();
    public override string ToString() => $"{Value:F1}%";
}

public readonly record struct UserId
{
    public int Value { get; }

    internal UserId(int value) => Value = value;

    public static Result<UserId> Create(int value)
    {
        if (value <= 0)
            return Error.Validation("UserId.Invalid", "UserId must be greater than zero");
        return new UserId(value);
    }

    public static implicit operator int(UserId id) => id.Value;
    public static implicit operator UserId(int value) => Create(value).GetValueOrThrow();
    public override string ToString() => Value.ToString();
}

public readonly record struct ExerciseId
{
    public int Value { get; }

    internal ExerciseId(int value) => Value = value;

    public static Result<ExerciseId> Create(int value)
    {
        if (value <= 0)
            return Error.Validation("ExerciseId.Invalid", "ExerciseId must be greater than zero");
        return new ExerciseId(value);
    }

    public static implicit operator int(ExerciseId id) => id.Value;
    public static implicit operator ExerciseId(int value) => Create(value).GetValueOrThrow();
    public override string ToString() => Value.ToString();
}

public readonly record struct LevelThreshold
{
    public string LevelName { get; }
    public int MinPercentage { get; }

    internal LevelThreshold(string levelName, int minPercentage)
    {
        LevelName = levelName;
        MinPercentage = minPercentage;
    }

    public static Result<LevelThreshold> Create(string levelName, int minPercentage)
    {
        if (string.IsNullOrWhiteSpace(levelName))
            return Error.Validation("LevelThreshold.EmptyName", "Level name cannot be empty");
        if (minPercentage < 0 || minPercentage > 100)
            return Error.Validation("LevelThreshold.InvalidPercentage", "Percentage must be between 0 and 100");
        return new LevelThreshold(levelName, minPercentage);
    }
}