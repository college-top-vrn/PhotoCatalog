using Microsoft.AspNetCore.Http;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Extensions;

/// <summary>
///     Методы расширения <see cref="ResultVoid" /> для работы с HTTP.
/// </summary>
public static class ResultVoidHttpExtensions
{
    /// <summary>
    ///     Преобразует <see cref="ResultVoid" /> в <see cref="IResult" />.
    /// </summary>
    public static IResult ToHttpResult(this ResultVoid result)
    {
        return result.IsSuccess
            ? Results.NoContent()
            : result.Error.ToHttpResult();
    }
}