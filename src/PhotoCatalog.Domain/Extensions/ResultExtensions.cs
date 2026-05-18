using System;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Extensions;

/// <summary>
///     Методы расширения для последовательной обработки результатов, содержащих данные.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    ///     Создает успешный результат из существующего значения.
    /// </summary>
    /// <typeparam name="TValue">Тип преобразуемого значения.</typeparam>
    /// <param name="value">Значение для инициализации цепочки.</param>
    /// <returns>Успешный результат с данными, либо провальный с системной ошибкой пустого значения.</returns>
    /// <example>
    ///     <code>
    ///     Result&lt;string&gt; result = "initial context".ToResult();
    ///     </code>
    /// </example>
    public static Result<TValue> ToResult<TValue>(this TValue value)
    {
        return Result.Success(value);
    }

    /// <summary>
    ///     Безопасно преобразует потенциально пустое значение в результат, заменяя null на ошибку.
    /// </summary>
    /// <typeparam name="TValue">Тип преобразуемого значения.</typeparam>
    /// <param name="value">Значение, полученное из внешнего источника.</param>
    /// <param name="errorIfNull">Доменная ошибка, сигнализирующая об отсутствии данных.</param>
    /// <returns>Успешный результат с данными, либо провальный с переданной ошибкой.</returns>
    /// <example>
    ///     <code>
    ///     Result&lt;Entity&gt; result = repository.Find(id)
    ///         .ToResult(new Error("Entity.NotFound", "Запись не найдена"));
    ///     </code>
    /// </example>
    public static Result<TValue> ToResult<TValue>(this TValue? value, Error errorIfNull)
    {
        return value is not null
            ? Result.Success(value)
            : Result.Failure<TValue>(errorIfNull);
    }

    /// <summary>
    ///     Инициирует безопасное выполнение функции, перехватывая любые исключения внешнего кода.
    ///     Используется как стартовая фабрика, а не метод расширения.
    /// </summary>
    /// <typeparam name="TValue">Тип возвращаемых данных.</typeparam>
    /// <param name="action">Рискованная функция получения данных.</param>
    /// <param name="errorHandler">Функция, преобразующая исключение в доменную ошибку.</param>
    /// <returns>Успешный результат с данными, либо провальный с перехваченной ошибкой.</returns>
    /// <example>
    ///     <code>
    ///     Result&lt;string&gt; configResult = ResultExtensions.TryCatch(
    ///         () => File.ReadAllText("config.json"),
    ///         ex => new Error("Config.ReadFailed", ex.Message)
    ///     );
    ///     </code>
    /// </example>
    public static Result<TValue> TryCatch<TValue>(
        Func<TValue> action,
        Func<Exception, Error> errorHandler)
    {
        try
        {
            return Result.Success(action());
        }
        catch (Exception ex)
        {
            return Result.Failure<TValue>(errorHandler(ex));
        }
    }

    /// <param name="result">Текущий результат.</param>
    /// <typeparam name="TValue">Исходный тип данных.</typeparam>
    extension<TValue>(Result<TValue>? result)
    {
        /// <summary>
        ///     Выполняет переход от текущих данных к новым данным.
        ///     Следующий шаг выполняется только при успешном статусе текущего результата.
        /// </summary>
        /// <typeparam name="TNextValue">Тип данных следующего шага.</typeparam>
        /// <param name="nextStep">Функция, возвращающая новый результат вычислений.</param>
        /// <returns>Результат выполнения следующего шага, либо проброшенная ошибка.</returns>
        /// <example>
        ///     <code>
        ///     Result&lt;Details&gt; result = GetEntity()
        ///         .Then(entity => FetchDetails(entity.Id));
        ///     </code>
        /// </example>
        public Result<TNextValue> Then<TNextValue>(Func<TValue, Result<TNextValue>> nextStep)
        {
            if (result is null)
            {
                return Result.Failure<TNextValue>(SystemErrors.NullResult);
            }

            return result.IsFailure
                ? Result.Failure<TNextValue>(result.Error)
                : nextStep(result.Value!);
        }

        /// <summary>
        ///     Выполняет переход от результата с данными к операции, не возвращающей значения.
        /// </summary>
        /// <param name="nextStep">Функция, возвращающая результат без данных.</param>
        /// <returns>Статус выполнения следующего шага, либо проброшенная ошибка.</returns>
        /// <example>
        ///     <code>
        ///     ResultVoid saveResult = CreateEntity()
        ///         .Then(entity => Repository.Save(entity));
        ///     </code>
        /// </example>
        public ResultVoid Then(Func<TValue, ResultVoid> nextStep)
        {
            if (result is null)
            {
                return ResultVoid.Failure(SystemErrors.NullResult);
            }

            return result.IsFailure
                ? ResultVoid.Failure(result.Error)
                : nextStep(result.Value!);
        }

        /// <summary>
        ///     Безопасно выполняет следующий шаг вычислений, изолируя выброс исключений.
        /// </summary>
        /// <typeparam name="TNextValue">Тип возвращаемых данных из рискованного кода.</typeparam>
        /// <param name="nextStep">Рискованная функция трансформации данных.</param>
        /// <param name="errorHandler">Функция перехвата исключения.</param>
        /// <returns>Успешный результат с новыми данными, либо провальный с перехваченной ошибкой.</returns>
        /// <example>
        ///     <code>
        ///     Result&lt;Data&gt; parsedResult = jsonStringResult
        ///         .ThenTry(
        ///             json => JsonSerializer.Deserialize&lt;Data&gt;(json),
        ///             ex => new Error("Json.ParseError", "Неверный формат.")
        ///         );
        ///     </code>
        /// </example>
        public Result<TNextValue> ThenTry<TNextValue>(Func<TValue, TNextValue> nextStep,
            Func<Exception, Error> errorHandler)
        {
            if (result is null)
            {
                return Result.Failure<TNextValue>(SystemErrors.NullResult);
            }

            if (result.IsFailure)
            {
                return Result.Failure<TNextValue>(result.Error);
            }

            try
            {
                return Result.Success(nextStep(result.Value!));
            }
            catch (Exception ex)
            {
                return Result.Failure<TNextValue>(errorHandler(ex));
            }
        }

        /// <summary>
        ///     Безопасно выполняет рискованное действие с данными без возврата новых значений.
        /// </summary>
        /// <param name="nextStep">Рискованное действие.</param>
        /// <param name="errorHandler">Функция перехвата исключения.</param>
        /// <returns>Успешный статус выполнения, либо провальный с перехваченной ошибкой.</returns>
        /// <example>
        ///     <code>
        ///     ResultVoid pushResult = entityResult
        ///         .ThenTry(
        ///             entity => MessageBus.Publish(entity),
        ///             ex => new Error("Bus.PublishError", "Сбой брокера.")
        ///         );
        ///     </code>
        /// </example>
        public ResultVoid ThenTry(Action<TValue> nextStep,
            Func<Exception, Error> errorHandler)
        {
            if (result is null)
            {
                return ResultVoid.Failure(SystemErrors.NullResult);
            }

            if (result.IsFailure)
            {
                return ResultVoid.Failure(result.Error);
            }

            try
            {
                nextStep(result.Value!);
                return ResultVoid.Success();
            }
            catch (Exception ex)
            {
                return ResultVoid.Failure(errorHandler(ex));
            }
        }

        /// <summary>
        ///     Безопасно преобразует данные в новый формат. Функция маппинга не генерирует ошибок домена.
        /// </summary>
        /// <typeparam name="TNextValue">Целевой тип данных.</typeparam>
        /// <param name="mapper">Функция трансформации.</param>
        /// <returns>Результат с преобразованными данными, либо проброшенная ошибка.</returns>
        /// <example>
        ///     <code>
        ///     Result&lt;Guid&gt; idResult = entityResult
        ///         .Transform(entity => entity.Id);
        ///     </code>
        /// </example>
        public Result<TNextValue> Transform<TNextValue>(Func<TValue, TNextValue> mapper)
        {
            if (result is null)
            {
                return Result.Failure<TNextValue>(SystemErrors.NullResult);
            }

            return result.IsFailure
                ? Result.Failure<TNextValue>(result.Error)
                : Result.Success(mapper(result.Value!));
        }

        /// <summary>
        ///     Проверяет успешный результат на соответствие заданному бизнес-правилу.
        ///     Прерывает цепочку указанной ошибкой, если условие не выполнено.
        /// </summary>
        /// <param name="predicate">Функция-условие.</param>
        /// <param name="error">Ошибка, возвращаемая при несоблюдении условия.</param>
        /// <returns>Исходные данные при успехе, либо провальный результат с ошибкой.</returns>
        /// <example>
        ///     <code>
        ///     Result&lt;int&gt; validAge = ageResult
        ///         .Ensure(
        ///             age => age >= 18, 
        ///             new Error("Validation.Age", "Требуется совершеннолетие.")
        ///         );
        ///     </code>
        /// </example>
        public Result<TValue> Ensure(Func<TValue, bool> predicate,
            Error error)
        {
            if (result is null)
            {
                return Result.Failure<TValue>(SystemErrors.NullResult);
            }

            if (result.IsFailure)
            {
                return Result.Failure<TValue>(result.Error);
            }

            return predicate(result.Value!)
                ? result
                : Result.Failure<TValue>(error);
        }

        /// <summary>
        ///     Выполняет проверку через стороннюю операцию без возврата данных.
        ///     Сохраняет исходные данные для дальнейшей обработки в цепочке.
        /// </summary>
        /// <param name="checker">Функция проверки, возвращающая статус.</param>
        /// <returns>Исходный результат при успехе, либо провальный с ошибкой проверки.</returns>
        /// <example>
        ///     <code>
        ///     Result&lt;Document&gt; result = documentResult
        ///         .Check(doc => Security.ValidateSignature(doc))
        ///         .Then(doc => Process(doc));
        ///     </code>
        /// </example>
        public Result<TValue> Check(Func<TValue, ResultVoid> checker)
        {
            if (result is null)
            {
                return Result.Failure<TValue>(SystemErrors.NullResult);
            }

            if (result.IsFailure)
            {
                return Result.Failure<TValue>(result.Error);
            }

            ResultVoid checkResult = checker(result.Value!);

            return checkResult.IsFailure
                ? Result.Failure<TValue>(checkResult.Error)
                : result;
        }

        /// <summary>
        ///     Выполняет проверку через операцию с возвратом данных.
        ///     Игнорирует возвращаемые данные проверки, сохраняя только исходный объект в цепочке.
        /// </summary>
        /// <typeparam name="TOther">Тип игнорируемых данных проверки.</typeparam>
        /// <param name="checker">Функция проверки, возвращающая сторонний результат.</param>
        /// <returns>Исходный результат при успехе, либо провальный с ошибкой проверки.</returns>
        /// <example>
        ///     <code>
        ///     Result&lt;Order&gt; result = orderResult
        ///         .Check(order => ValidateCustomer(order.CustomerId)) // Возвращает Result&lt;Customer&gt;
        ///         .Then(order => Ship(order)); 
        ///     </code>
        /// </example>
        public Result<TValue> Check<TOther>(Func<TValue, Result<TOther>> checker)
        {
            if (result is null)
            {
                return Result.Failure<TValue>(SystemErrors.NullResult);
            }

            if (result.IsFailure)
            {
                return Result.Failure<TValue>(result.Error);
            }

            Result<TOther> checkResult = checker(result.Value!);

            return checkResult.IsFailure
                ? Result.Failure<TValue>(checkResult.Error)
                : result;
        }

        /// <summary>
        ///     Выполняет побочное действие (например, логирование) только при успешном результате.
        /// </summary>
        /// <param name="action">Действие, использующее успешные данные.</param>
        /// <returns>Исходный результат без изменений.</returns>
        /// <example>
        ///     <code>
        ///     result.OnSuccess(entity => Log.Info($"Сущность {entity.Id} обработана"));
        ///     </code>
        /// </example>
        public Result<TValue> OnSuccess(Action<TValue> action)
        {
            if (result is not null && result.IsSuccess)
            {
                action(result.Value!);
            }

            return result ?? Result.Failure<TValue>(SystemErrors.NullResult);
        }

        /// <summary>
        ///     Выполняет побочное действие только при наличии ошибки.
        /// </summary>
        /// <param name="action">Действие для логирования или обработки ошибки.</param>
        /// <returns>Исходный результат без изменений.</returns>
        /// <example>
        ///     <code>
        ///     result.OnFailure(error => Log.Error($"Сбой бизнес-логики: {error.Message}"));
        ///     </code>
        /// </example>
        public Result<TValue> OnFailure(Action<Error> action)
        {
            if (result is null)
            {
                action(SystemErrors.NullResult);
                return Result.Failure<TValue>(SystemErrors.NullResult);
            }

            if (result.IsFailure)
            {
                action(result.Error);
            }

            return result;
        }

        /// <summary>
        ///     Завершает цепочку операций, принудительно извлекая данные и конвертируя их в финальный тип.
        /// </summary>
        /// <typeparam name="TLanding">Целевой тип возвращаемого значения.</typeparam>
        /// <param name="success">Маппер для успешных данных.</param>
        /// <param name="failure">Маппер для ошибки.</param>
        /// <returns>Объект целевого типа, созданный на основе успеха или провала.</returns>
        /// <example>
        ///     <code>
        ///     IActionResult response = result.Finally(
        ///         success: entity => Ok(entity),
        ///         failure: error => BadRequest(error)
        ///     );
        ///     </code>
        /// </example>
        public TLanding Finally<TLanding>(Func<TValue, TLanding> success,
            Func<Error, TLanding> failure)
        {
            if (result is null)
            {
                return failure(SystemErrors.NullResult);
            }

            return result.IsSuccess
                ? success(result.Value!)
                : failure(result.Error);
        }
    }
}