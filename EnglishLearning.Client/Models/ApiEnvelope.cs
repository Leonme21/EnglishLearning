namespace EnglishLearning.Client.Models;

public record ApiResult<T>(T? Data, ApiError? Error, bool IsSuccess)
{
    public static ApiResult<T> Success(T data) => new(data, null, true);
    public static ApiResult<T> Failure(ApiError error) => new(default, error, false);
}

public record ApiError(string Code, string Message);

public record ApiResponse<T>(bool Success, T Data, string CorrelationId);
public record ApiErrorResponse(ApiError Error);
