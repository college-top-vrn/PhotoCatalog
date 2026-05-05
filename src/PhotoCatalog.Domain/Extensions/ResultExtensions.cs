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
/// </remarks>>
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
    ///     Result&lt;Photo&gt; photoResult = Dimensions.Create(1920, 1080)
    ///         .Then(dims => Photo.Create("path/to/file.jpg", dims));
    ///     </code>
    /// </example>
    public static Result<TNextValue> Then<TValue, TNextValue>
        (this Result<TValue>? result, Func<TValue, Result<TNextValue>> nextStep)
    {
        if (result is null) return Result<TNextValue>.Failure(SystemErrors.NullResult);

        return result.IsFailure ? Result<TNextValue>.Failure(result.Error) : nextStep(result.Value!);
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
    ///     ResultVoid saveResult = Photo.Create("path.jpg", dims)
    ///         .Then(photo => _repository.Add(photo));
    ///     </code>
    /// </example>
    public static ResultVoid Then<TValue>(this Result<TValue>? result, Func<TValue, ResultVoid> nextStep)
    {
        if (result is null) return ResultVoid.Failure(SystemErrors.NullResult);
        return result.IsFailure ? ResultVoid.Failure(result.Error) : nextStep(result.Value!);
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
    ///     Result&lt;int&gt; idResult = _repository.GetPhoto(1)
    ///         .Transform(photo => photo.Id); // Превращаем Photo в int
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
    ///     .OnSuccess(photo => Log.Info($"Успешно обработано фото {photo.Id}"))
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
    ///         success: photo => $"Фото {photo.Id} готово!",
    ///         failure: error => $"Не удалось: {error.Message}"
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
    ///     Проверяет успешный результат на соотвествие заданному условию.
    ///     Если  условие не выполнено, прерывает цепочку и возвращает указанную ошибку
    /// </summary>
    /// <typeparam name="TValue">Тип значения внутри результат</typeparam>
    /// <param name="result">Текущий результат</param>
    /// <param name="predicate">Функция-условие (возвращает true, если условие истино)</param>
    /// <param name="error">Исходный результат при успехе, либо новый провальный результат</param>
    /// <returns></returns>
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