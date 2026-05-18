using System.Collections.Generic;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Repositories;

/// <summary>
///     Интерфейс для получения тегов из базы данных.
///     Методы этого интерфейса гарантированно не изменяют состояние базы данных.
/// </summary>
public interface ITagQueryRepository
{
    /// <summary>
    ///     Получает тег по его уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор тега.</param>
    /// <returns>Результат получения тега.</returns>
    Result<Tag> GetById(int id);

    /// <summary>
    ///     Получает все теги.
    /// </summary>
    /// <returns>Результат получения коллекции тегов.</returns>
    Result<IEnumerable<Tag>> GetAll();

    /// <summary>
    ///     Получает тег по его уникальному текстовому имени.
    /// </summary>
    /// <param name="name">Имя тега</param>
    /// <returns>Результат получения тега.</returns>
    Result<Tag> GetByName(string name);
}