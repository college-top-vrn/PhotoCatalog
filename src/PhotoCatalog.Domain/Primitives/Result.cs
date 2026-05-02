using System;

namespace PhotoCatalog.Domain.Primitives;

/// <summary>
/// Представляет результат выполнения операции, не возвращающей значение (аналог void).
/// Инкапсулирует логику успеха или провала.
/// </summary>
/// <remarks>
/// ВНИМАНИЕ: Не создавайте объект через конструктор по умолчанию.
/// Для инициализации используйте исключительно статические методы: <see cref="Success"/> или <see cref="Failure"/>.
/// </remarks>
public readonly record struct Result
{
    /// <summary>
    /// Указывает, завершилась ли операция успешно.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Указывает, завершилась ли операция с ошибкой.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Объект детализированной ошибки. Если операция успешна, содержит <see cref="Primitives.Error.None"/>.
    /// </summary>
    public Error Error { get; }


    /// <summary>
    /// Приватный конструктор для инициализации внутреннего состояния.
    /// </summary>
    /// <param name="isSuccess">Флаг успешности операции.</param>
    /// <param name="error">Объект ошибки.</param>
    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Создает успешный результат без ошибок.
    /// </summary>
    /// <returns>Успешный <see cref="Result"/>, где <see cref="IsSuccess"/> равно true, а <see cref="Error"/> равно <see cref="Primitives.Error.None"/>.</returns>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Создает провальный результат с указанной ошибкой.
    /// </summary>
    /// <param name="error">Бизнес-ошибка, объясняющая причину провала.</param>
    /// <returns>Провальный <see cref="Result"/>, где <see cref="IsFailure"/> равно true, а <see cref="Error"/> содержит переданную ошибку.</returns>
    public static Result Failure(Error error) => new(false, error);
}