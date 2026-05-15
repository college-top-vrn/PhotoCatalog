using System;

using Microsoft.AspNetCore.Http;

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
        return Result<TValue>.Success(value);
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
            ? Result<TValue>.Success(value)
            : Result<TValue>.Failure(errorIfNull);
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
            return Result<TValue>.Success(action());
        }
        catch (Exception ex)
        {
            return Result<TValue>.Failure(errorHandler(ex));
        }
    }

    /// <summary>
    ///     Выполняет переход от текущих данных к новым данным.
    ///     Следующий шаг выполняется только при успешном статусе текущего результата.
    /// </summary>
    /// <typeparam name="TValue">Исходный тип данных.</typeparam>
    /// <typeparam name="TNextValue">Тип данных следующего шага.</typeparam>
    /// <param name="result">Текущий результат.</param>
    /// <param name="nextStep">Функция, возвращающая новый результат вычислений.</param>
    /// <returns>Результат выполнения следующего шага, либо проброшенная ошибка.</returns>
    /// <example>
    ///     <code>
    ///     Result&lt;Details&gt; result = GetEntity()
    ///         .Then(entity => FetchDetails(entity.Id));
    ///     </code>
    /// </example>
    public static Result<TNextValue> Then<TValue, TNextValue>(
        this Result<TValue>? result,
        Func<TValue, Result<TNextValue>> nextStep)
    {
        if (result is null)
        {
            return Result<TNextValue>.Failure(SystemErrors.NullResult);
        }

        return result.IsFailure
            ? Result<TNextValue>.Failure(result.Error)
            : nextStep(result.Value!);
    }

    /// <summary>
    ///     Выполняет переход от результата с данными к операции, не возвращающей значения.
    /// </summary>
    /// <typeparam name="TValue">Тип текущих данных.</typeparam>
    /// <param name="result">Текущий результат.</param>
    /// <param name="nextStep">Функция, возвращающая результат без данных.</param>
    /// <returns>Статус выполнения следующего шага, либо проброшенная ошибка.</returns>
    /// <example>
    ///     <code>
    ///     ResultVoid saveResult = CreateEntity()
    ///         .Then(entity => Repository.Save(entity));
    ///     </code>
    /// </example>
    public static ResultVoid Then<TValue>(
        this Result<TValue>? result,
        Func<TValue, ResultVoid> nextStep)
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
    /// <typeparam name="TValue">Тип текущих данных.</typeparam>
    /// <typeparam name="TNextValue">Тип возвращаемых данных из рискованного кода.</typeparam>
    /// <param name="result">Текущий результат.</param>
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
    public static Result<TNextValue> ThenTry<TValue, TNextValue>(
        this Result<TValue>? result,
        Func<TValue, TNextValue> nextStep,
        Func<Exception, Error> errorHandler)
    {
        if (result is null)
        {
            return Result<TNextValue>.Failure(SystemErrors.NullResult);
        }

        if (result.IsFailure)
        {
            return Result<TNextValue>.Failure(result.Error);
        }

        try
        {
            return Result<TNextValue>.Success(nextStep(result.Value!));
        }
        catch (Exception ex)
        {
            return Result<TNextValue>.Failure(errorHandler(ex));
        }
    }

    /// <summary>
    ///     Безопасно выполняет рискованное действие с данными без возврата новых значений.
    /// </summary>
    /// <typeparam name="TValue">Тип текущих данных.</typeparam>
    /// <param name="result">Текущий результат.</param>
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
    public static ResultVoid ThenTry<TValue>(
        this Result<TValue>? result,
        Action<TValue> nextStep,
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
    /// <typeparam name="TValue">Исходный тип данных.</typeparam>
    /// <typeparam name="TNextValue">Целевой тип данных.</typeparam>
    /// <param name="result">Текущий результат.</param>
    /// <param name="mapper">Функция трансформации.</param>
    /// <returns>Результат с преобразованными данными, либо проброшенная ошибка.</returns>
    /// <example>
    ///     <code>
    ///     Result&lt;Guid&gt; idResult = entityResult
    ///         .Transform(entity => entity.Id);
    ///     </code>
    /// </example>
    public static Result<TNextValue> Transform<TValue, TNextValue>(
        this Result<TValue>? result,
        Func<TValue, TNextValue> mapper)
    {
        if (result is null)
        {
            return Result<TNextValue>.Failure(SystemErrors.NullResult);
        }

        return result.IsFailure
            ? Result<TNextValue>.Failure(result.Error)
            : Result<TNextValue>.Success(mapper(result.Value!));
    }

    /// <summary>
    ///     Проверяет успешный результат на соответствие заданному бизнес-правилу.
    ///     Прерывает цепочку указанной ошибкой, если условие не выполнено.
    /// </summary>
    /// <typeparam name="TValue">Тип проверяемых данных.</typeparam>
    /// <param name="result">Текущий результат.</param>
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
    public static Result<TValue> Ensure<TValue>(
        this Result<TValue>? result,
        Func<TValue, bool> predicate,
        Error error)
    {
        if (result is null)
        {
            return Result<TValue>.Failure(SystemErrors.NullResult);
        }

        if (result.IsFailure)
        {
            return Result<TValue>.Failure(result.Error);
        }

        return predicate(result.Value!)
            ? result
            : Result<TValue>.Failure(error);
    }

    /// <summary>
    ///     Выполняет проверку через стороннюю операцию без возврата данных.
    ///     Сохраняет исходные данные для дальнейшей обработки в цепочке.
    /// </summary>
    /// <typeparam name="TValue">Тип сохраняемых данных.</typeparam>
    /// <param name="result">Текущий результат.</param>
    /// <param name="checker">Функция проверки, возвращающая статус.</param>
    /// <returns>Исходный результат при успехе, либо провальный с ошибкой проверки.</returns>
    /// <example>
    ///     <code>
    ///     Result&lt;Document&gt; result = documentResult
    ///         .Check(doc => Security.ValidateSignature(doc))
    ///         .Then(doc => Process(doc));
    ///     </code>
    /// </example>
    public static Result<TValue> Check<TValue>(
        this Result<TValue>? result,
        Func<TValue, ResultVoid> checker)
    {
        if (result is null)
        {
            return Result<TValue>.Failure(SystemErrors.NullResult);
        }

        if (result.IsFailure)
        {
            return Result<TValue>.Failure(result.Error);
        }

        ResultVoid checkResult = checker(result.Value!);

        return checkResult.IsFailure
            ? Result<TValue>.Failure(checkResult.Error)
            : result;
    }

    /// <summary>
    ///     Выполняет проверку через операцию с возвратом данных.
    ///     Игнорирует возвращаемые данные проверки, сохраняя только исходный объект в цепочке.
    /// </summary>
    /// <typeparam name="TValue">Тип сохраняемых данных.</typeparam>
    /// <typeparam name="TOther">Тип игнорируемых данных проверки.</typeparam>
    /// <param name="result">Текущий результат.</param>
    /// <param name="checker">Функция проверки, возвращающая сторонний результат.</param>
    /// <returns>Исходный результат при успехе, либо провальный с ошибкой проверки.</returns>
    /// <example>
    ///     <code>
    ///     Result&lt;Order&gt; result = orderResult
    ///         .Check(order => ValidateCustomer(order.CustomerId)) // Возвращает Result&lt;Customer&gt;
    ///         .Then(order => Ship(order)); 
    ///     </code>
    /// </example>
    public static Result<TValue> Check<TValue, TOther>(
        this Result<TValue>? result,
        Func<TValue, Result<TOther>> checker)
    {
        if (result is null)
        {
            return Result<TValue>.Failure(SystemErrors.NullResult);
        }

        if (result.IsFailure)
        {
            return Result<TValue>.Failure(result.Error);
        }

        Result<TOther> checkResult = checker(result.Value!);

        return checkResult.IsFailure
            ? Result<TValue>.Failure(checkResult.Error)
            : result;
    }

    /// <summary>
    ///     Выполняет побочное действие (например, логирование) только при успешном результате.
    /// </summary>
    /// <typeparam name="TValue">Тип данных.</typeparam>
    /// <param name="result">Текущий результат.</param>
    /// <param name="action">Действие, использующее успешные данные.</param>
    /// <returns>Исходный результат без изменений.</returns>
    /// <example>
    ///     <code>
    ///     result.OnSuccess(entity => Log.Info($"Сущность {entity.Id} обработана"));
    ///     </code>
    /// </example>
    public static Result<TValue> OnSuccess<TValue>(this Result<TValue>? result, Action<TValue> action)
    {
        if (result is not null && result.IsSuccess)
        {
            action(result.Value!);
        }

        return result ?? Result<TValue>.Failure(SystemErrors.NullResult);
    }

    /// <summary>
    ///     Выполняет побочное действие только при наличии ошибки.
    /// </summary>
    /// <typeparam name="TValue">Тип данных.</typeparam>
    /// <param name="result">Текущий результат.</param>
    /// <param name="action">Действие для логирования или обработки ошибки.</param>
    /// <returns>Исходный результат без изменений.</returns>
    /// <example>
    ///     <code>
    ///     result.OnFailure(error => Log.Error($"Сбой бизнес-логики: {error.Message}"));
    ///     </code>
    /// </example>
    public static Result<TValue> OnFailure<TValue>(this Result<TValue>? result, Action<Error> action)
    {
        if (result is null)
        {
            action(SystemErrors.NullResult);
            return Result<TValue>.Failure(SystemErrors.NullResult);
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
    /// <typeparam name="TValue">Тип данных в результате.</typeparam>
    /// <typeparam name="TLanding">Целевой тип возвращаемого значения.</typeparam>
    /// <param name="result">Финальный результат.</param>
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
    public static TLanding Finally<TValue, TLanding>(
        this Result<TValue>? result,
        Func<TValue, TLanding> success,
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