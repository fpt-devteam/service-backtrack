namespace Backtrack.Core.WebApi.Contracts.Common;

public sealed record ApiResponse<T>
{
    public required bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }
    public required string CorrelationId { get; init; }

    public static ApiResponse<T> SuccessResponse(T data, string correlationId)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Error = null,
            CorrelationId = correlationId
        };
    }

    public static ApiResponse<T> ErrorResponse(ApiError error, string correlationId)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Error = error,
            CorrelationId = correlationId
        };
    }
}

public sealed record ApiError
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public object? Details { get; init; }
}
