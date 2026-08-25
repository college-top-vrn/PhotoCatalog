// using System;
//
// using Microsoft.AspNetCore.Http;
//
// using PhotoCatalog.Domain.Primitives;
//
// namespace PhotoCatalog.Infrastructure.Extensions;
//
// /// <summary>
// ///     Методы расширения <see cref="ResultError" /> для работы с HTTP.
// /// </summary>
// public static class ResultErrorHttpExtensions
// {
//     /// <summary>
//     ///     Метод расширения для преобразования <see cref="ResultError" /> в <see cref="IResult" />.
//     /// </summary>
//     /// <param name="resultError"></param>
//     /// <returns></returns>
//     public static IResult ToHttpResult(this ResultError resultError)
//     {
//         return resultError.Code switch
//         {
//             var code when code.EndsWith(".NotFound", StringComparison.CurrentCulture) =>
//                 Results.NotFound(resultError.Message),
//
//             var code when code.EndsWith(".CannotMoveToSelf", StringComparison.CurrentCulture) ||
//                           code.Contains("Duplicate")
//                           || code.EndsWith(".CycleDetected", StringComparison.CurrentCulture) ||
//                           code.EndsWith(".OrphanedFile", StringComparison.CurrentCulture)
//                           || code.EndsWith(".HasChildren", StringComparison.CurrentCulture)
//                 => Results.Conflict(resultError.Message),
//
//             var code when code.StartsWith("Cache.", StringComparison.CurrentCulture) ||
//                           code.StartsWith("Database.", StringComparison.CurrentCulture)
//                           || code.StartsWith("FileStorage.", StringComparison.CurrentCulture) ||
//                           code.StartsWith("MetadataExtractor.", StringComparison.CurrentCulture)
//                           || code.StartsWith("Transactions.", StringComparison.CurrentCulture)
//                 => Results.Problem(resultError.Message, statusCode: StatusCodes.Status500InternalServerError),
//
//             _ => Results.BadRequest(resultError.Message)
//         };
//     }
// }