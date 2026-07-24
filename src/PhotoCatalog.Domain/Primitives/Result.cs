namespace PhotoCatalog.Domain.Primitives;

/// <summary>
///     Необобщенный базовый класс <see cref="Result{T}" />.
/// </summary>
public static class Result
{
    /// <summary>
    ///     Создает успешный результат с переданным значением.
    ///     Гарантирует функциональную безопасность: при передаче null возвращает системную ошибку.
    /// </summary>
    /// <param name="value">Значение успешной операции.</param>
    /// <returns>Экземпляр <see cref="Result{T}" /> со статусом успеха или провала (если передан null).</returns>
    public static Result<T> Success<T>(T value)
    {
        return value is null
            ? Failure<T>(SystemErrors.NullValue)
            : Result<T>.CreateInternal(value, true, ResultError.None);
    }

    /// <summary>
    ///     Создает провальный результат с указанной ошибкой и значением по умолчанию.
    /// </summary>
    public static Result<T> Failure<T>(ResultError resultError)
    {
        return Result<T>.CreateInternal(default, false, resultError);
    }
}

/// <summary>
///     Представляет результат выполнения операции, возвращающей значение типа <typeparamref name="T" />.
/// </summary>
/// <typeparam name="T">Тип возвращаемого значения.</typeparam>
/// <remarks>
///     ВНИМАНИЕ: Не создавайте объект через конструктор по умолчанию.
///     Используйте фабрики Success/Failure.
/// </remarks>
public class Result<T>
{
    /// <summary>
    ///     Инициализирует внутреннее состояние объекта.
    /// </summary>
    /// <param name="value">Результат операции. Имеет значение по умолчанию, если операция провалена.</param>
    /// <param name="isSuccess">Флаг, указывающий на успешное завершение операции.</param>
    /// <param name="resultError">Детализированная бизнес-ошибка. Равна <see cref="ResultError.None" /> при успехе.</param>
    private Result(T? value, bool isSuccess, ResultError resultError)
    {
        Value = value;
        IsSuccess = isSuccess;
        ResultError = resultError;
    }

    /// <summary>
    ///     Указывает, завершилась ли операция успешно.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    ///     Указывает, завершилась ли операция с ошибкой.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    ///     Объект ошибки. Если операция успешна, содержит <see cref="ResultError.None" />.
    /// </summary>
    public ResultError ResultError { get; }

    /// <summary>
    ///     Возвращает результат операции.
    ///     Если операция завершилась провалом (<see cref="IsFailure" /> равно true), возвращает значение по умолчанию
    ///     (default/null).
    /// </summary>
    public T? Value { get; }

    /// <summary>
    ///     Внутренний метод для сборки объекта необобщенной фабрикой.
    /// </summary>
    /// <param name="value">Результат операции. Имеет значение по умолчанию, если операция провалена.</param>
    /// <param name="isSuccess">Флаг, указывающий на успешное завершение операции.</param>
    /// <param name="resultError">Детализированная бизнес-ошибка. Равна <see cref="ResultError.None" /> при успехе.</param>
    /// <returns>Собранный объект.</returns>
    internal static Result<T> CreateInternal(T? value, bool isSuccess, ResultError resultError)
    {
        return new Result<T>(value, isSuccess, resultError);
    }

    /// <summary>
    ///     Неявно преобразует Result{T} обобщенного типа в базовый ResultVoid.
    ///     Обеспечивает статическую диспетчеризацию (полиморфизм) на этапе компиляции.
    /// </summary>
    public static implicit operator ResultVoid(Result<T> result)
    {
        return result.IsSuccess
            ? ResultVoid.Success()
            : ResultVoid.Failure(result.ResultError);
    }

    /// <summary>
    ///     Деконструирует объект результата для удобного использования в паттерн-матчинге и кортежах.
    /// </summary>
    /// <param name="isSuccess">Флаг успешности.</param>
    /// <param name="value">Значение (или default при ошибке).</param>
    /// <param name="resultError">Объект ошибки.</param>
    public void Deconstruct(out bool isSuccess, out T? value, out ResultError resultError)
    {
        isSuccess = IsSuccess;
        value = Value;
        resultError = ResultError;
    }
}