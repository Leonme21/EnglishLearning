namespace EnglishLearning.Domain.Common;

public readonly struct Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    private Result(bool isSuccess, Error error)
    {
        if (isSuccess && error.Code != default)
            throw new InvalidOperationException("Successful result cannot have an error");
        if (!isSuccess && error.Code == default)
            throw new InvalidOperationException("Failed result must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, default);
    public static Result Failure(Error error) => new(false, error);

    public static implicit operator Result(Error error) => Failure(error);
}

public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public Error Error { get; }

    private Result(T? value, bool isSuccess, Error error)
    {
        if (isSuccess && error.Code != default)
            throw new InvalidOperationException("Successful result cannot have an error");
        if (!isSuccess && error.Code == default)
            throw new InvalidOperationException("Failed result must have an error");

        Value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value, true, default);
    public static Result<T> Failure(Error error) => new(default, false, error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);

    public T GetValueOrThrow()
    {
        if (IsFailure)
            throw new InvalidOperationException($"Result is failure: {Error.Code} - {Error.Message}");
        return Value!;
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error);
}