namespace PhotoCatalog.Domain.Primitives;

/// <summary>
/// Представляет структуру для стандартизированного описания бизнес-ошибок в домене.
/// </summary>
/// <param name="code">
/// Уникальный строковый идентификатор ошибки (например, "Tag.EmptyName"). 
/// Используется для программной проверки типа ошибки на прикладном слое.
/// </param>
/// <param name="message">
/// Понятное описание проблемы на естественном языке. 
/// В первую очередь предназначено для логирования и помощи разработчикам при отладке.
/// </param>
public readonly struct Error(string code, string message)
{
    /// <summary>
    /// Свойство обозначающие код ошибки 
    /// </summary>
    public readonly string Code { get; } = code;

    /// <summary>
    /// Свойство обзначающие человекочитаемое сообщение об ошибки 
    /// </summary>
    public readonly string Message { get; } = message;


    /// <summary>
    /// Специальный объект, обозначающий отсутствие ошибки.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);
}