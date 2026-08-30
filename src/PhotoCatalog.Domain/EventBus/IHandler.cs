using System.Threading;
using System.Threading.Tasks;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.EventBus;

/// <summary>
///     Представляет механизм для реализации обработки события.
/// </summary>
public interface IHandler<in TEvent>
{
    /// <summary>
    ///     Обрабатывает переданное событие.
    /// </summary>
    /// <param name="domainEvent">событие.</param>
    /// <param name="cancellationToken">токен отмены.</param>
    ValueTask<ResultVoid> HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}