using System;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Repositories;

/// <summary>
///     Представляет механизм для получения данных БД.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IEntityGetterById<TEntity> where TEntity : Entity
{
    /// <summary>
    ///     Получает сущность по её идентификатору и идентификатору владельца.
    /// </summary>
    /// <param name="userId">идентификатор владельца.</param>
    /// <param name="entityId">идентификатор сущности.</param>
    /// <returns></returns>
    Result<TEntity> GetById(Guid userId, Guid entityId);
}