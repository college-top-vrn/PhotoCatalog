using System;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Extensions;

/// <summary>
///     Методы расширения для последовательной обработки результатов без данных.
/// </summary>
public static class ResultVoidExtensions
{
    /// <summary>
    ///     Безопасно инициирует цепочку вычислений, перехватывая любые исключения внешнего кода.
    ///     Используется как стартовая фабрика, а не метод расширения.
    /// </summary>
    /// <param name="action">Рискованное действие без возвращаемого значения.</param>
    /// <param name="errorHandler">Функция, преобразующая пойманное исключение в доменную ошибку.</param>
    /// <returns>Успешный результат при отсутствии исключений, либо провальный с ошибкой из обработчика.</returns>
    /// <example>
    ///     <code>
    ///     ResultVoid saveResult = ResultVoidExtensions.TryCatch(
    ///         () => _dbContext.SaveChanges(),
    ///         ex => new Error("Database.SaveError", ex.Message)
    ///     );
    ///     </code>
    /// </example>
    public static ResultVoid TryCatch(
        Action action,
        Func<Exception, Error> errorHandler)
    {
        try
        {
            action();
            return ResultVoid.Success();
        }
        catch (Exception ex)
        {
            return ResultVoid.Failure(errorHandler(ex));
        }
    }

    /// <param name="result">Текущий результат без значения.</param>
    extension(ResultVoid result)
    {
        /// <summary>
        ///     Связывает две операции, не возвращающие данных.
        ///     Выполняет следующий шаг только в случае успеха текущего результата.
        /// </summary>
        /// <param name="nextStep">Функция, возвращающая следующий результат выполнения.</param>
        /// <returns>Результат выполнения следующего шага или провальный результат с текущей ошибкой.</returns>
        /// <example>
        ///     <code>
        ///     ResultVoid result = ValidationService.CheckPermissions()
        ///         .Then(() => Database.CommitTransaction());
        ///     </code>
        /// </example>
        public ResultVoid Then(Func<ResultVoid> nextStep)
        {
            return result.IsFailure
                ? result
                : nextStep();
        }

        /// <summary>
        ///     Осуществляет переход от операции без результата к операции с возвращаемыми данными.
        ///     Выполняет вычисление только при успешном завершении предыдущего шага.
        /// </summary>
        /// <typeparam name="TNextValue">Тип значения, которое вернёт следующий шаг.</typeparam>
        /// <param name="nextStep">Функция, возвращающая результат с данными.</param>
        /// <returns>Результат выполнения следующего шага или провальный результат с текущей ошибкой.</returns>
        /// <example>
        ///     <code>
        ///     Result&lt;Entity&gt; result = Security.ValidateAccess()
        ///         .Then(() => Repository.GetEntity(id));
        ///     </code>
        /// </example>
        public Result<TNextValue> Then<TNextValue>(Func<Result<TNextValue>> nextStep)
        {
            return result.IsFailure
                ? Result.Failure<TNextValue>(result.Error)
                : nextStep();
        }

        /// <summary>
        ///     Безопасно выполняет следующий шаг с получением данных, изолируя возможные исключения.
        /// </summary>
        /// <typeparam name="TNextValue">Ожидаемый тип возвращаемых данных.</typeparam>
        /// <param name="nextStep">Рискованная функция вычисления данных.</param>
        /// <param name="errorHandler">Функция перехвата и преобразования исключения.</param>
        /// <returns>Успешный результат с данными, либо провальный с ошибкой (текущей или из перехватчика).</returns>
        /// <example>
        ///     <code>
        ///     Result&lt;byte[]&gt; fileResult = validationResult
        ///         .ThenTry(
        ///             () => File.ReadAllBytes(path),
        ///             ex => new Error("File.ReadError", "Сбой чтения файла.")
        ///         );
        ///     </code>
        /// </example>
        public Result<TNextValue> ThenTry<TNextValue>(Func<TNextValue> nextStep,
            Func<Exception, Error> errorHandler)
        {
            if (result.IsFailure)
            {
                return Result.Failure<TNextValue>(result.Error);
            }

            try
            {
                return Result.Success(nextStep());
            }
            catch (Exception ex)
            {
                return Result.Failure<TNextValue>(errorHandler(ex));
            }
        }

        /// <summary>
        ///     Безопасно выполняет следующее рискованное действие без возврата данных.
        /// </summary>
        /// <param name="nextStep">Рискованное действие (например, вызов стороннего API).</param>
        /// <param name="errorHandler">Функция перехвата и преобразования исключения.</param>
        /// <returns>Успешный результат, либо провальный с ошибкой (текущей или из перехватчика).</returns>
        /// <example>
        ///     <code>
        ///     ResultVoid notifyResult = commitResult
        ///         .ThenTry(
        ///             () => externalService.SendNotification(),
        ///             ex => new Error("Network.SendFailed", "Сбой отправки уведомления.")
        ///         );
        ///     </code>
        /// </example>
        public ResultVoid ThenTry(Action nextStep,
            Func<Exception, Error> errorHandler)
        {
            if (result.IsFailure)
            {
                return result;
            }

            try
            {
                nextStep();
                return ResultVoid.Success();
            }
            catch (Exception ex)
            {
                return ResultVoid.Failure(errorHandler(ex));
            }
        }

        /// <summary>
        ///     Выполняет побочное действие исключительно при успешном статусе результата.
        /// </summary>
        /// <param name="action">Действие для выполнения.</param>
        /// <returns>Исходный объект результата без изменений.</returns>
        /// <example>
        ///     <code>
        ///     result.OnSuccess(() => Logger.Log("Операция успешно завершена."));
        ///     </code>
        /// </example>
        public ResultVoid OnSuccess(Action action)
        {
            if (result.IsSuccess)
            {
                action();
            }

            return result;
        }

        /// <summary>
        ///     Выполняет побочное действие исключительно при наличии ошибки в результате.
        /// </summary>
        /// <param name="action">Действие для обработки ошибки.</param>
        /// <returns>Исходный объект результата без изменений.</returns>
        /// <example>
        ///     <code>
        ///     result.OnFailure(error => Logger.Error($"Сбой: {error.Code}"));
        ///     </code>
        /// </example>
        public ResultVoid OnFailure(Action<Error> action)
        {
            if (result.IsFailure)
            {
                action(result.Error);
            }

            return result;
        }

        /// <summary>
        ///     Завершает цепочку операций, принудительно конвертируя результат в финальный тип данных.
        /// </summary>
        /// <typeparam name="TLanding">Тип возвращаемого значения (например, HTTP-статус).</typeparam>
        /// <param name="success">Функция, возвращающая значение при успехе.</param>
        /// <param name="failure">Функция, преобразующая ошибку в финальное значение.</param>
        /// <returns>Единое значение указанного типа, независимое от статуса успеха.</returns>
        /// <example>
        ///     <code>
        ///     int statusCode = result.Finally(
        ///         success: () => 204, // No Content
        ///         failure: error => 400 // Bad Request
        ///     );
        ///     </code>
        /// </example>
        public TLanding Finally<TLanding>(Func<TLanding> success,
            Func<Error, TLanding> failure)
        {
            return result.IsSuccess
                ? success()
                : failure(result.Error);
        }
    }
}