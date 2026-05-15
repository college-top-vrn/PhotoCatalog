using Microsoft.AspNetCore.Http;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Extensions;

/// <summary>
///     Методы расширения для <see cref="Error" />.
/// </summary>
public static class ErrorExtensions
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
            var code when code.EndsWith(".NotFound") => Results.NotFound(error.Message),

            var code when code.EndsWith(".CannotMoveToSelf") || code.Contains("Duplicate")
                                                             || code.EndsWith(".CycleDetected") ||
                                                             code.EndsWith(".OrphanedFile")
                                                             || code.EndsWith(".HasChildren")
                => Results.Conflict(error.Message),

            var code when code.StartsWith("Cache.") || code.StartsWith("Database.")
                                                    || code.StartsWith("FileStorage.") ||
                                                    code.StartsWith("MetadataExtractor.")
                                                    || code.StartsWith("Transactions.")
                => Results.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError),

            _ => Results.BadRequest(error.Message)
        };
    }
}