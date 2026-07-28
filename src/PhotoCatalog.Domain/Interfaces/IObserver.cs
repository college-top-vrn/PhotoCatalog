using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces;

/// <summary>
///     Представляет механизм для создания наблюдателя.
/// </summary>
public interface IObserver
{
    /// <summary>
    ///     Обрабатывает переданное событие.
    /// </summary>
    /// <param name="event">событие.</param>
    /// <returns></returns>
    ResultVoid Process(Event @event);
}