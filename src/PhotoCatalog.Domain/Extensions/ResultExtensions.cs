using System;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Extensions;

/// <summary>
///     Методы расширения для последовательной обработки результатов.
/// </summary>
/// <remarks>
///     Класс представляет Fluent API для построения цепочек вычислений без использования вложенных <c>if/else</c>.
///     Все методы содержат встроенную защиту от <c>NullReferenceException</c>.
///     В случае передачи <c>null</c> вместо объекта <see cref="Result{T}"/>, методы генерируют системную ошибку.
/// </remarks>
public static class ResultExtensions
{
    /// <summary>
    /// Then: Выполняет следующее действие, которое само может завершиться ошибкой.
    /// </summary>
    /// <typeparam name="TValue">Тип текущего значения</typeparam>
    /// <typeparam name="TNextValue">Тип значения, которое вернёт следующий шаг.</typeparam>
    /// <param name="result">Текущий результат выполнения операции</param>
    /// <param name="nextStep">Функция, содержащая вычисление следующего шага. Должна вернуть <see cref="Result{TNextValue}"/>.</param>
    /// <returns>
    ///     Результат выполнения <paramref name="nextStep"/>, если текущий результат успешен.
    ///     Иначе - пробрасывает текущую ошибку дальше.
    /// </returns>
    /// <remarks>
    ///     <b>Как работает:</b>
    ///     Проверяет статус текущего <paramref name="result"/>. Если он провален (<see cref="Result{T}.IsFailure"/>),
    ///     функция <paramref name="nextStep"/> не вызывается, а ошибка передается дальше.
    /// </remarks>
    /// <example>
    ///     <code>
    ///     // 1. Получаем валидные данные (Result&lt;string&gt;)
    ///     // 2. Передаем их в парсер, который тоже может вернуть ошибку (Result&lt;int&gt;)
    ///     Result&lt;int&gt; numberResult = DataProvider.GetData()
    ///         .Then(text => Parser.ParseToInt(text));
    ///     </code>
    /// </example>
    public static Result<TNextValue> Then<TValue, TNextValue>
        (this Result<TValue>? result, Func<TValue, Result<TNextValue>> nextStep)
    {
        if (result is null) return Result<TNextValue>.Failure(SystemErrors.NullResult);

        return result.IsFailure
            ? Result<TNextValue>.Failure(result.Error)
            : nextStep(result.Value!);
    }


    /// <summary>
    ///     Выполняет операцию, которая не возвращает данных.
    /// </summary>
    /// <typeparam name="TValue">Тип текущего значения.</typeparam>
    /// <param name="result">Текущий результат выполнения операции.</param>
    /// <param name="nextStep">Функция, возвращающая <see cref="ResultVoid"/>.</param>
    /// <returns>
    ///     Успешный или провальный <see cref="ResultVoid"/> в зависимости от итога <paramref name="nextStep"/>.
    /// </returns>
    /// <remarks>
    ///     <b>Как работает:</b>
    ///     Действует аналогично основной перегрузке <c>Then</c>, но осуществляет переход от цепочки с данными (<see cref="Result{T}"/>) 
    ///     к цепочке без данных (<see cref="ResultVoid"/>).
    /// </remarks>
    /// <example>
    ///     <code>
    ///     // 1. Создаем объект (Result&lt;Entity&gt;)
    ///     // 2. Сохраняем его, результат сохранения не содержит новых данных (ResultVoid)
    ///     ResultVoid saveResult = Factory.CreateEntity("Name")
    ///         .Then(entity => Repository.Save(entity));
    ///     </code>
    /// </example>
    public static ResultVoid Then<TValue>(this Result<TValue>? result, Func<TValue, ResultVoid> nextStep)
    {
        if (result is null) return ResultVoid.Failure(SystemErrors.NullResult);
        return result.IsFailure
            ? ResultVoid.Failure(result.Error)
            : nextStep(result.Value!);
    }


    /// <summary>
    ///     Безопасно преобразует успешное значение в новый формат или тип.
    /// </summary>
    /// <typeparam name="TValue">Исходный тип значения.</typeparam>
    /// <typeparam name="TNextValue">Целевой тип значения.</typeparam>
    /// <param name="result">Текущий результат.</param>
    /// <param name="mapper">Функция трансформации данных.</param>
    /// <returns>
    ///     Новый <see cref="Result{TNextValue}"/>, содержащий преобразованные данные, либо проброшенную ошибку.
    /// </returns>
    /// <remarks>
    ///     <b>Как работает:</b>
    ///     В отличие от <c>Then</c>, функция <paramref name="mapper"/> не может вернуть ошибку. 
    ///     Метод автоматически оборачивает результат её выполнения в успешный <see cref="Result{T}"/>.
    /// </remarks>
    /// <example>
    ///     <code>
    ///     // Извлекаем конкретное свойство из успешного объекта
    ///     Result&lt;int&gt; idResult = Repository.GetRecord(1)
    ///         .Transform(record => record.Id); 
    ///     </code>
    /// </example>
    public static Result<TNextValue> Transform<TValue, TNextValue>
        (this Result<TValue>? result, Func<TValue, TNextValue> mapper)
    {
        if (result is null) return Result<TNextValue>.Failure(SystemErrors.NullResult);

        return result.IsFailure
            ? Result<TNextValue>.Failure(result.Error)
            : Result<TNextValue>.Success(mapper(result.Value!));
    }

    /// <summary>
    ///     Выполняет побочное действие только при успешном результате.
    /// </summary>
    /// <typeparam name="TValue">Тип значения.</typeparam>
    /// <param name="result">Текущий результат.</param>
    /// <param name="action">Действие для выполнения (например, логирование).</param>
    /// <returns>Тот же объект <paramref name="result"/> без изменений.</returns>
    /// <remarks>
    ///     <b>Как работает:</b>
    ///     Метод не прерывает цепочку вычислений и не меняет содержащиеся данные. 
    ///     Игнорируется, если результат содержит ошибку.
    /// </remarks>
    /// <example>
    ///     <code>
    ///     .OnSuccess(data => Log.Info($"Успешно обработано: {data}"))
    ///     </code>
    /// </example>
    public static Result<TValue> OnSuccess<TValue>(this Result<TValue>? result, Action<TValue> action)
    {
        if (result is not null && result.IsSuccess) action(result.Value!);

        return result ?? Result<TValue>.Failure(SystemErrors.NullResult);
    }


    /// <summary>
    ///     Выполняет побочное действие только при наличии ошибки.
    /// </summary>
    /// <typeparam name="TValue">Тип значения.</typeparam>
    /// <param name="result">Текущий результат.</param>
    /// <param name="action">Действие для обработки ошибки (например, логирование).</param>
    /// <returns>Тот же объект <paramref name="result"/> без изменений.</returns>
    /// <remarks>
    ///     <b>Как работает:</b>
    ///     Обеспечивает "подглядывание" за состоянием ошибки, не прерывая её движение по цепочке.
    /// </remarks>
    /// <example>
    ///     <code>
    ///     .OnFailure(error => Log.Error($"Сбой операции: {error.Code}"))
    ///     </code>
    /// </example>
    public static Result<TValue> OnFailure<TValue>(this Result<TValue>? result, Action<Error> action)
    {
        if (result is null)
        {
            action(SystemErrors.NullResult);
            return Result<TValue>.Failure(SystemErrors.NullResult);
        }

        if (result.IsFailure) action(result.Error);
        return result;
    }


    /// <summary>
    ///     Завершает цепочку операций и конвертирует результат в финальный тип данных.
    /// </summary>
    /// <typeparam name="TValue">Тип значения внутри результата.</typeparam>
    /// <typeparam name="TLanding">Тип, к которому должны быть приведены и успех, и ошибка.</typeparam>
    /// <param name="result">Финальный объект результата.</param>
    /// <param name="success">Функция, описывающая, во что превратить успешные данные.</param>
    /// <param name="failure">Функция, описывающая, во что превратить ошибку.</param>
    /// <returns>Единое значение типа <typeparamref name="TLanding"/>.</returns>
    /// <remarks>
    ///     <b>Как работает:</b>
    ///     Выступает заменой оператору <c>switch</c>. Гарантирует, что разработчик обработает оба исхода (успех и провал).
    ///     Извлекает данные из контейнера <see cref="Result{T}"/> во внешний код.
    /// </remarks>
    /// <example>
    ///     <code>
    ///     string message = result.Finally(
    ///         success: data => $"Операция успешна: {data}",
    ///         failure: error => $"Произошла ошибка: {error.Message}"
    ///     );
    ///     </code>
    /// </example>
    public static TLanding Finally<TValue, TLanding>(
        this Result<TValue>? result,
        Func<TValue, TLanding> success,
        Func<Error, TLanding> failure)
    {
        if (result is null) return failure(SystemErrors.NullResult);

        return result.IsSuccess
            ? success(result.Value!)
            : failure(result.Error);
    }

    /// <summary>
    ///     Связывание двух операций, не возвращающих данных.
    ///     Выполняет следующий шаг, только если предыдущий ResultVoid был успешен.
    /// </summary>
    /// <param name="result">Текущий результат (без значения).</param>
    /// <param name="nextStep">Функция, возвращающая новый ResultVoid.</param>
    /// <returns>Результат выполнения следующего шага или оригинальная ошибка.</returns>
    /// <example>
    ///     <code>
    ///     // 1. Первая независимая операция (возвращает ResultVoid)
    ///     // 2. Вторая независимая операция (возвращает ResultVoid)
    ///     ResultVoid result = Processor.ExecuteFirstStep()
    ///         .Then(() => Processor.ExecuteSecondStep());
    ///     </code>
    /// </example>
    public static ResultVoid Then(
        this ResultVoid result,
        Func<ResultVoid> nextStep)
    {
        return result.IsFailure
            ? result
            : nextStep();
    }


    /// <summary>
    ///     Переход от операции без результата к операции с результатом.
    ///     Выполняет следующий шаг, только если предыдущий ResultVoid был успешен.
    /// </summary>
    /// <typeparam name="TNextValue">Тип значения, которое вернёт следующий шаг.</typeparam>
    /// <param name="result">Текущий результат (без значения).</param>
    /// <param name="nextStep">Функция, возвращающая Result с данными.</param>
    /// <returns>Результат выполнения следующего шага или перенесённая ошибка из текущего результата.</returns>
    /// <example>
    ///     <code>
    ///     // 1. Проверка предварительных условий (возвращает ResultVoid)
    ///     // 2. Если успешно -> выполняем вычисление (возвращает Result&lt;int&gt;)
    ///     Result&lt;int&gt; result = Validator.CheckState()
    ///         .Then(() => Calculator.ComputeValue(10));
    ///     </code>
    /// </example>
    public static Result<TNextValue> Then<TNextValue>(
        this ResultVoid result,
        Func<Result<TNextValue>> nextStep)
    {
        return result.IsFailure
            ? Result<TNextValue>.Failure(result.Error)
            : nextStep();
    }


    /// <summary>
    ///     Оборачивает значение в объект <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="TValue">Тип преобразуемого значения.</typeparam>
    /// <param name="value">Значение, которое необходимо поместить в контекст результата.</param>
    /// <returns>
    ///     Успешный результат, если значение не равно null. 
    ///     Иначе — провальный результат с системной ошибкой пустого значения.
    /// </returns>
    /// <example>
    ///     <code>
    ///     // Инициализация цепочки из обычного значения
    ///     var result = "initial data".ToResult();
    ///     </code>
    /// </example>
    public static Result<TValue> ToResult<TValue>(this TValue value) => value;

    /// <summary>
    ///     Преобразует потенциально пустое значение в <see cref="Result{T}"/> с заменой null на конкретную ошибку.
    /// </summary>
    /// <typeparam name="TValue">Тип преобразуемого значения.</typeparam>
    /// <param name="value">Значение для проверки на наличие данных.</param>
    /// <param name="errorIfNull">Ошибка, которая будет возвращена в случае отсутствия данных.</param>
    /// <returns>
    ///     Успешный результат с данными, если значение не равно null. 
    ///     В противном случае — провальный результат с переданной ошибкой.
    /// </returns>
    /// <example>
    ///     <code>
    ///     // Обработка данных, которые могут отсутствовать во внешнем источнике например БД
    ///     var result = externalSource.FindData()
    ///         .ToResult(new Error("Data.NotFound", "Данные не обнаружены"));
    ///     </code>
    /// </example>
    public static Result<TValue> ToResult<TValue>(this TValue? value, Error errorIfNull)
    {
        return value is not null
            ? Result<TValue>.Success(value)
            : Result<TValue>.Failure(errorIfNull);
    }


    /// <summary>
    ///     Проверяет успешный результат на соответствие заданному условию.
    ///     Если условие не выполнено, прерывает цепочку и возвращает указанную ошибку.
    /// </summary>
    /// <typeparam name="TValue">Тип значения внутри результата.</typeparam>
    /// <param name="result">Текущий результат.</param>
    /// <param name="predicate">Функция-условие (возвращает true, если условие истинно).</param>
    /// <param name="error">Ошибка, которая вернётся, если условие не выполнено.</param>
    /// <returns>Исходный результат при успехе, либо новый провальный результат с переданной ошибкой.</returns>
    /// <example>
    ///     <code>
    ///     // Проверяем числовое значение на соответствие правилу.
    ///     // Если число меньше или равно нулю, цепочка прервется с указанной ошибкой.
    ///     Result&lt;int&gt; validNumberResult = numericResult
    ///         .Ensure(
    ///             value => value > 0, 
    ///             new Error("Validation.NegativeValue", "Значение должно быть положительным")
    ///         );
    ///     </code>
    /// </example>
    public static Result<TValue> Ensure<TValue>(
        this Result<TValue>? result,
        Func<TValue, bool> predicate,
        Error error)

    {
        if (result is null) return Result<TValue>.Failure(SystemErrors.NullResult);
        if (result.IsFailure) return Result<TValue>.Failure(result.Error);

        return predicate(result.Value!)
            ? result
            : Result<TValue>.Failure(error);
    }
}