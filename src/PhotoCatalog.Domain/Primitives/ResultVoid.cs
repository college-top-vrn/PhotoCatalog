namespace PhotoCatalog.Domain.Primitives;

/// <summary>
///     Представляет результат выполнения операции, не возвращающей значение (аналог void).
///     Инкапсулирует логику успеха или провала.
/// </summary>
/// <remarks>
///     ВНИМАНИЕ: Не создавайте объект через конструктор по умолчанию.
///     Для инициализации используйте исключительно статические методы: <see cref="Success" /> или <see cref="Failure" />.
/// </remarks>
public readonly record struct ResultVoid
{
    /// <summary>
    ///     Приватный конструктор для инициализации внутреннего состояния.
    /// </summary>
    /// <param name="isSuccess">Флаг успешности операции.</param>
    /// <param name="resultError">Объект ошибки.</param>
    private ResultVoid(bool isSuccess, ResultError resultError)
    {
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
    ///     Объект детализированной ошибки. Если операция успешна, содержит <see cref="ResultError.None" />.
    /// </summary>
    public ResultError ResultError { get; }

    /// <summary>
    ///     Создает успешный результат без ошибок.
    /// </summary>
    /// <returns>
    ///     Успешный <see cref="ResultVoid" />, где <see cref="IsSuccess" /> равно true, а <see cref="ResultError" /> равно
    ///     <see cref="ResultError.None" />.
    /// </returns>
    public static ResultVoid Success()
    {
        return new ResultVoid(true, ResultError.None);
    }

    /// <summary>
    ///     Создает провальный результат с указанной ошибкой.
    /// </summary>
    /// <param name="resultError">Бизнес-ошибка, объясняющая причину провала.</param>
    /// <returns>
    ///     Провальный <see cref="ResultVoid" />, где <see cref="IsFailure" /> равно true, а <see cref="ResultError" />
    ///     содержит
    ///     переданную ошибку.
    /// </returns>
    public static ResultVoid Failure(ResultError resultError)
    {
        return new ResultVoid(false, resultError);
    }
}