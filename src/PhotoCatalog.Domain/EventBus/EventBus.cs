using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.EventBus;

/// <summary>
///     Класс, предсставляющий функционал EventBus.
/// </summary>
public sealed class EventBus
{
    private object?[] _handlers = [];
    private readonly Lock _sync = new();

    /// <summary>
    ///     Добавляет обработчика события.
    /// </summary>
    /// <param name="handler">обработчик.</param>
    /// <typeparam name="TEvent">событие.</typeparam>
    /// <typeparam name="TValue">возвращаемое значение обработчика.</typeparam>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="SystemErrors.NullArgument" />, если обработчик является null.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid Subscribe<TEvent>(IHandler<TEvent>? handler)
    {
        const int minSize = 8;

        if (handler is null) return ResultVoid.Failure(SystemErrors.NullArgument);

        int id = EventTypeId<TEvent>.Id;

        if (id >= _handlers.Length)
        {
            int newSize = Math.Max(id, _handlers.Length == 0 ? minSize : _handlers.Length * 2);
            var newArray = new object?[newSize];
            _handlers.CopyTo(newArray, 0);
            _handlers = newArray;
        }

        object? element = _handlers[id];

        switch (element)
        {
            case null:
                {
                    _handlers[id] = handler;
                    break;
                }
            case IHandler<TEvent> singleHandler:
                {
                    _handlers[id] = new[] { singleHandler, handler };
                    break;
                }
            case IHandler<TEvent>[] handlers:
                {
                    IHandler<TEvent>[] newArray = new IHandler<TEvent>[handlers.Length + 1];
                    handlers.CopyTo(newArray, 0);
                    newArray[^1] = handler;
                    _handlers[id] = newArray;
                    break;
                }
        }

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Удаляет обработчика события.
    /// </summary>
    /// <param name="handler">обработчик.</param>
    /// <typeparam name="TEvent">событие.</typeparam>
    /// <typeparam name="TValue">возвращаемое значение обработчика.</typeparam>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="SystemErrors.NullArgument" />, если обработчик является null.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.EventBus.HandlerNotFound" />, если обработчик не найден.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid Unsubscribe<TEvent>(IHandler<TEvent>? handler)
    {
        const int notFound = -1;

        if (handler is null) return ResultVoid.Failure(SystemErrors.NullArgument);

        int id = EventTypeId<TEvent>.Id;

        if (id >= _handlers.Length) return ResultVoid.Failure(DomainErrors.EventBus.HandlerNotFound);

        object? element = _handlers[id];

        switch (element)
        {
            case IHandler<TEvent>:
                {
                    _handlers[id] = null;
                    break;
                }
            case IHandler<TEvent>[] handlers:
                {
                    int index = handlers.IndexOf(handler);

                    if (index == notFound) return ResultVoid.Failure(DomainErrors.EventBus.HandlerNotFound);

                    if (handlers.Length == 2)
                    {
                        _handlers[id] = index == 0 ? handlers[1] : handlers[0];
                        return ResultVoid.Success();
                    }

                    handlers[index] = null!;

                    int newSize = handlers.Length - 1;

                    IHandler<TEvent>[] newArray = new IHandler<TEvent>[newSize];

                    handlers.Where(h => h != null!).ToArray().CopyTo(
                        newArray,
                        0
                    );

                    _handlers[id] = newArray;

                    break;
                }
        }

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Публикует событие для обработчиков.
    /// </summary>
    /// <param name="domainEvent"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEvent"></typeparam>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask<ResultVoid> PublishAsync<TEvent>(
        TEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        if (domainEvent is null) return ResultVoid.Failure(SystemErrors.NullArgument);

        int id = EventTypeId<TEvent>.Id;

        if (id >= _handlers.Length) return ResultVoid.Failure(DomainErrors.EventBus.HandlerNotFound);

        object? element = _handlers[id];

        switch (element)
        {
            case null: return ResultVoid.Failure(SystemErrors.NullValue);
            case IHandler<TEvent> singleHandler:
                {
                    await singleHandler.HandleAsync(domainEvent, cancellationToken);
                    break;
                }
            case IHandler<TEvent>[] handlers:
                {
                    var tasks = new Task[handlers.Length];

                    for (int i = 0; i < handlers.Length; i++)
                    {
                        tasks[i] = handlers[i].HandleAsync(domainEvent, cancellationToken).AsTask();
                    }

                    await Task.WhenAll(tasks).ConfigureAwait(false);
                    break;
                }
        }

        return ResultVoid.Success();
    }
}