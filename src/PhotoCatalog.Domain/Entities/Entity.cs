using System;
using System.Collections.Generic;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Entities;

/// <summary>
/// Абстрактная сущность всех доменных моделей.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Идентификатор сущности.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Идентификатор владельца сущности.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Список доменных событий.
    /// </summary>
    protected List<object> DomainEvents { get; } = [];

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="id">идентификатор сущности.</param>
    /// <param name="userId">идентификатор владельца сущности.</param>
    protected Entity(Guid id, Guid userId)
    {
        Id = id;
        UserId = userId;
    }

    /// <summary>
    ///     Добавляет доменное событие в список.
    /// </summary>
    /// <param name="domainEvent">доменное событие.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Entity.SuchDomainEventAlreadyExists" />,
    ///                 если переданное доменное событие уже есть в списке.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    protected ResultVoid AddDomainEvent(object domainEvent)
    {
        if (DomainEvents.Contains(domainEvent))
        {
            return ResultVoid.Failure(DomainErrors.Entity.SuchDomainEventAlreadyExists);
        }

        DomainEvents.Add(domainEvent);

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Очищает список доменных событий.
    /// </summary>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Entity.DomainEventListIsAlreadyEmpty" />,
    ///                 если список уже пустой.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    protected ResultVoid ClearDomainEventList()
    {
        if (DomainEvents.Count == 0)
        {
            return ResultVoid.Failure(DomainErrors.Entity.DomainEventListIsAlreadyEmpty);
        }

        DomainEvents.Clear();

        return ResultVoid.Success();
    }
}