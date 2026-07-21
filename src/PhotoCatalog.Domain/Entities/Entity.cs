using System;

namespace PhotoCatalog.Domain.Entities;

/// <summary>
/// Абстрактная сущность всех доменных моделей.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="id"></param>
    protected Entity(Guid id)
    {
        Id = id;
    }
}