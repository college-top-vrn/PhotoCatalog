using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.EventBus;

/// <summary>
///     Представляет механизм для реализации обработки события.
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IHandler
{
    /// <summary>
    ///     Обрабатывает переданное событие.
    /// </summary>
    /// <param name="domainEvent">событие.</param>
    ResultVoid Handle(Event domainEvent);
}