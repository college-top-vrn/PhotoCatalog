using System;

namespace PhotoCatalog.Domain.Interfaces;

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
    /// Конструктор.
    /// </summary>
    /// <param name="id">идентификатор сущности.</param>
    /// <param name="userId">идентификатор владельца сущности.</param>
    protected Entity(Guid id, Guid userId)
    {
        Id = id;
        UserId = userId;
    }
}