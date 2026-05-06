namespace PhotoCatalog.Domain.Primitives;

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
    /// <param name="value"> Результат операции. Имеет значение по умолчанию, если операция провалена.</param>
    /// <param name="isSuccess">Флаг, указывающий на успешное завершение операции.</param>
    /// <param name="error">Детализированная бизнес-ошибка. Равна <see cref="Primitives.Error.None" /> при успехе.</param>
    private Result(T? value, bool isSuccess, Error error)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
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
    ///     Объект ошибки. Если операция успешна, содержит <see cref="Primitives.Error.None" />.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    ///     Возвращает результат операции.
    ///     Если операция завершилась провалом (<see cref="IsFailure" /> равно true), возвращает значение по умолчанию
    ///     (default/null).
    /// </summary>
    public T? Value { get; }

    /// <summary>
    ///     Создает успешный результат с переданным значением.
    ///     Гарантирует функциональную безопасность: при передаче null возвращает системную ошибку.
    /// </summary>
    /// <param name="value">Значение успешной операции.</param>
    /// <returns>Экземпляр <see cref="Result{T}"/> со статусом успеха или провала (если передан null).</returns>
    public static Result<T> Success(T value)
    {
        return value is null
            ? Failure(SystemErrors.NullValue)
            : new Result<T>(value, true, Error.None);
    }

    /// <summary>
    ///     Создает провальный результат с указанной ошибкой и значением по умолчанию.
    /// </summary>
    public static Result<T> Failure(Error error)
    {
        return new Result<T>(default, false, error);
    }


    /// <summary>
    ///     Неявно преобразует Result{T} обобщенного типа в базовый ResultVoid.
    ///     Обеспечивает статическую диспетчеризацию (полиморфизм) на этапе компиляции.
    /// </summary>
    public static implicit operator ResultVoid(Result<T> result)
    {
        return result.IsSuccess ? ResultVoid.Success() : ResultVoid.Failure(result.Error);
    }

    /// <summary>
    ///     Деконструирует объект результата для удобного использования в паттерн-матчинге и кортежах.
    /// </summary>
    /// <param name="isSuccess">Флаг успешности.</param>
    /// <param name="value">Значение (или default при ошибке).</param>
    /// <param name="error">Объект ошибки.</param>
    public void Deconstruct(out bool isSuccess, out T? value, out Error error)
    {
        isSuccess = IsSuccess;
        value = Value;
        error = Error;
    }
}