using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.EventBus;

/// <summary>
///     Класс, предсставляющий функционал EventBus.
/// </summary>
public class EventBus
{
    /// <summary>
    ///     Обработчики.
    /// </summary>
    private readonly ConcurrentDictionary<Type, ImmutableList<IHandler>> _eventHandlers = new();

    /// <summary>
    ///     Добавить для события нового обработчика.
    /// </summary>
    /// <param name="domainEvent">событие.</param>
    /// <param name="handler">обработчик.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.EventBus.UnableToAddPair"/>;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.EventBus.UnableToUpdatePair"/>;
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid Subscribe(Type domainEvent, IHandler handler)
    {
        if (!_eventHandlers.ContainsKey(domainEvent))
        {
            ImmutableList<IHandler> handlerToAdd = [handler];

            return _eventHandlers.TryAdd(domainEvent, handlerToAdd)
                ? ResultVoid.Success()
                : ResultVoid.Failure(DomainErrors.EventBus.UnableToAddPair);
        }

        _eventHandlers.TryGetValue(domainEvent, out ImmutableList<IHandler>? availableHandlers);

        ImmutableList<IHandler> updatedHandlers = availableHandlers!.Add(handler);

        return _eventHandlers.TryUpdate(domainEvent, updatedHandlers, availableHandlers)
            ? ResultVoid.Success()
            : ResultVoid.Failure(DomainErrors.EventBus.UnableToUpdatePair);
    }

    /// <summary>
    ///     Удалить обработчика для события.
    /// </summary>
    /// <param name="domainEvent">событие.</param>
    /// <param name="handler">обработчик.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.EventBus.KeyNotExists"/>;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.EventBus.UnableToUpdatePair"/>;
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid Unsubscribe(Type domainEvent, IHandler handler)
    {
        if (!_eventHandlers.ContainsKey(domainEvent))
        {
            return ResultVoid.Failure(DomainErrors.EventBus.KeyNotExists);
        }

        _eventHandlers.TryGetValue(domainEvent, out ImmutableList<IHandler>? availableHandlers);

        ImmutableList<IHandler> updatedHandlers = availableHandlers!.Remove(handler);

        return _eventHandlers.TryUpdate(domainEvent, updatedHandlers, availableHandlers)
            ? ResultVoid.Success()
            : ResultVoid.Failure(DomainErrors.EventBus.UnableToUpdatePair);
    }

    /// <summary>
    ///     Вызывает обработчиков для события.
    /// </summary>
    /// <param name="domainEvent">событие.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.EventBus.KeyNotExists"/>;
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid Publish(Event domainEvent)
    {
        Type domainEventType = domainEvent.GetType();

        if (!_eventHandlers.ContainsKey(domainEventType))
        {
            return ResultVoid.Failure(DomainErrors.EventBus.KeyNotExists);
        }

        ImmutableList<IHandler> handlers = _eventHandlers
            .FirstOrDefault(p => p.Key == domainEventType)
            .Value;

        handlers.ForEach(h => h.Handle(domainEvent));

        return ResultVoid.Success();
    }
}