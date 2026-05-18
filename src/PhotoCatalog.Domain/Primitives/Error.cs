namespace PhotoCatalog.Domain.Primitives;

/// <summary>
///     Представляет структуру для стандартизированного описания бизнес-ошибок в домене.
/// </summary>
/// <param name="Code">
///     Уникальный строковый идентификатор ошибки (например, "Tag.EmptyName").
///     Используется для программной проверки типа ошибки на прикладном слое.
/// </param>
/// <param name="Message">
///     Понятное описание проблемы на естественном языке.
///     В первую очередь предназначено для логирования и помощи разработчикам при отладке.
/// </param>
/// TODO: Переименовать Error, чтобы он не совпадал с названием встроенного в язык одноименного типа
public readonly record struct Error(string Code, string Message)
{
    /// <summary>
    ///     Специальный объект, обозначающий отсутствие ошибки.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);
}