using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Repositories;

/// <summary>
///     Представляет механизм для изменения данных БД.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface ICommandRepository<in TEntity> where TEntity : Entity
{
    /// <summary>
    ///     Добавляет сущность.
    /// </summary>
    /// <param name="entity">сущность.</param>
    /// <returns></returns>
    ResultVoid Add(TEntity entity);
    /// <summary>
    ///     Обновляет сущность.
    /// </summary>
    /// <param name="entity">сущность.</param>
    /// <returns></returns>
    ResultVoid Update(TEntity entity);
    /// <summary>
    ///     Удаляет сущность.
    /// </summary>
    /// <param name="entity">сущность.</param>
    /// <returns></returns>
    ResultVoid Delete(TEntity entity);
}