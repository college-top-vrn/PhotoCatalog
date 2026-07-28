using System;
using System.Collections.Generic;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces;

/// <summary>
///     Абстрактная сущность для всех издателей, предоставляющая механизм работы с наблюдателями.
/// </summary>
public abstract class ObservableEntity : Entity
{
    private readonly List<IObserver> _observers = [];

    /// <summary>
    ///     Конструктор.
    /// </summary>
    /// <param name="id">идентификатор сущности.</param>
    /// <param name="userId">идентификатор владельца сущности.</param>
    protected ObservableEntity(Guid id, Guid userId) : base(id, userId) { }

    /// <summary>
    ///     Добавляет наблюдателя.
    /// </summary>
    /// <param name="observer">добавляемый наблюдатель.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Observable.SuchObserverAlreadyExists"/>,
    ///                 если данный наблюдатель уже существует.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid Add(IObserver observer)
    {
        if (_observers.Contains(observer))
            return ResultVoid.Failure(DomainErrors.Observable.SuchObserverAlreadyExists);

        _observers.Add(observer);

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Удаляет наблюдателя.
    /// </summary>
    /// <param name="observer">удаляемый наблюдатель</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Observable.SuchObserverNotExists"/>,
    ///                 если данный наблюдатель не существует.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid Delete(IObserver observer)
    {
        if (!_observers.Contains(observer))
            return ResultVoid.Failure(DomainErrors.Observable.SuchObserverNotExists);

        _observers.Remove(observer);

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Уведомляет наблюдателей.
    /// </summary>
    /// <param name="event">событие для уведомления.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Observable.ObserversNotExist"/>,
    ///                 если наблюдатели отсутствуют.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid Notify(Event @event)
    {
        if (_observers.Count == 0)
            return ResultVoid.Failure(DomainErrors.Observable.ObserversNotExist);

        foreach (var observer in _observers) observer.Process(@event);

        return ResultVoid.Success();
    }
}