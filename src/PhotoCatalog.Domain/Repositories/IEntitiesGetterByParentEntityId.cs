using System;
using System.Collections.Generic;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Repositories;

/// <summary>
///     Представляет механизм для получения данных БД.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IEntitiesGetterByParentEntityId<TEntity> where TEntity : Entity
{
    /// <summary>
    ///     Получает дочерние сущности по идентификатору родительской сущности и идентификатору владельца.
    /// </summary>
    /// <param name="userId">идентификатор владельца.</param>
    /// <param name="parentEntityId">идентификатор родительской сущности.</param>
    /// <returns></returns>
    Result<List<TEntity>> GetAllByParentEntityId(Guid userId, Guid parentEntityId);
}