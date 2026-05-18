using System;

using Microsoft.AspNetCore.Http;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Extensions;

/// <summary>
///     Методы расширения <see cref="Error" /> для работы с HTTP.
/// </summary>
public static class ErrorHttpExtensions
{
    /// <summary>
    ///     Метод расширения для преобразования <see cref="Error" /> в <see cref="IResult" />.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static IResult ToHttpResult(this Error error)
    {
        return error.Code switch
        {
            var code when code.EndsWith(".NotFound", StringComparison.CurrentCulture) =>
                Results.NotFound(error.Message),

            var code when code.EndsWith(".CannotMoveToSelf", StringComparison.CurrentCulture) ||
                          code.Contains("Duplicate")
                          || code.EndsWith(".CycleDetected", StringComparison.CurrentCulture) ||
                          code.EndsWith(".OrphanedFile", StringComparison.CurrentCulture)
                          || code.EndsWith(".HasChildren", StringComparison.CurrentCulture)
                => Results.Conflict(error.Message),

            var code when code.StartsWith("Cache.", StringComparison.CurrentCulture) ||
                          code.StartsWith("Database.", StringComparison.CurrentCulture)
                          || code.StartsWith("FileStorage.", StringComparison.CurrentCulture) ||
                          code.StartsWith("MetadataExtractor.", StringComparison.CurrentCulture)
                          || code.StartsWith("Transactions.", StringComparison.CurrentCulture)
                => Results.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError),

            _ => Results.BadRequest(error.Message)
        };
    }
}