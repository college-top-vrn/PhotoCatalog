using Microsoft.AspNetCore.Http;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Extensions;

/// <summary>
///     Методы расширения <see cref="Result{T}" /> для работы с HTTP.
/// </summary>
public static class ResultHttpExtensions
{
    /// <summary>
    ///     Преобразует <see cref="Result{T}" /> в <see cref="IResult" /> с соответствующим HTTP-статусом.
    /// </summary>
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? result.Value is null ? Results.NoContent() : Results.Ok(result.Value)
            : result.Error.ToHttpResult();
    }
}