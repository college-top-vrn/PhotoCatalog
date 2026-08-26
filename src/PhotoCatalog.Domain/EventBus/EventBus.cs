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
    public ConcurrentDictionary<Type, ImmutableHashSet<IHandler>> EventHandlers { get; } = new();

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
    public ResultVoid Subscribe(Object domainEvent, IHandler handler)
    {
        while (true)
        {
            Type domainEventType = domainEvent.GetType();

            if (EventHandlers.TryGetValue(domainEventType, out ImmutableHashSet<IHandler>? currentHandlers))
            {
                ImmutableHashSet<IHandler> updatedHandlers = currentHandlers.Add(handler);

                if (EventHandlers.TryUpdate(domainEventType, updatedHandlers, currentHandlers))
                {
                    return ResultVoid.Success();
                }
            }
            else
            {
                ImmutableHashSet<IHandler> handlerToAdd = [handler];

                if (EventHandlers.TryAdd(domainEventType, handlerToAdd))
                {
                    return ResultVoid.Success();
                }
            }
        }
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
    public ResultVoid Unsubscribe(Object domainEvent, IHandler handler)
    {
        while (true)
        {
            Type domainEventType = domainEvent.GetType();

            if (EventHandlers.TryGetValue(domainEventType, out ImmutableHashSet<IHandler>? currentHandlers))
            {
                ImmutableHashSet<IHandler> updatedHandlers = currentHandlers.Remove(handler);

                if (EventHandlers.TryUpdate(domainEventType, updatedHandlers, currentHandlers))
                {
                    return ResultVoid.Success();
                }
            }
            else
            {
                return ResultVoid.Failure(DomainErrors.EventBus.KeyNotExists);
            }
        }
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
    public ResultVoid Publish(Object domainEvent)
    {
        while (true)
        {
            Type domainEventType = domainEvent.GetType();

            if (!EventHandlers.ContainsKey(domainEventType))
            {
                return ResultVoid.Failure(DomainErrors.EventBus.KeyNotExists);
            }

            ImmutableHashSet<IHandler> handlers = EventHandlers
                .FirstOrDefault(p => p.Key == domainEventType)
                .Value;

            foreach (var handler in handlers) handler.Handle(domainEvent);

            return ResultVoid.Success();
        }
    }
}