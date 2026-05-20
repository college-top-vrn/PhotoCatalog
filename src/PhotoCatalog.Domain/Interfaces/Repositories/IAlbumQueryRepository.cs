using System.Collections.Generic;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Repositories;

/// <summary>
///     Контракт для операций чтения альбомов в соответствии с CQRS.
/// </summary>
/// <remarks>
///     Содержит только детерминированные методы выборки данных.
///     Реализации работают независимо от транзакций и открывают свои подключения.
///     Возвращаемые коллекции иммутабельны.
/// </remarks>
public interface IAlbumQueryRepository
{
    /// <summary>
    ///     Получает альбом по уникальному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор альбома.</param>
    /// <returns>
    ///     Успех: альбом с указанным ID.
    ///     Ошибка: NotFound — альбом не существует, ConnectionFailed — проблема с БД.
    /// </returns>
    Result<Album> GetById(int id);

    /// <summary>
    ///     Получает все альбомы, находящиеся внутри указанной папки.
    /// </summary>
    /// <param name="folderId">Идентификатор папки.</param>
    /// <returns>
    ///     Успех: коллекция альбомов (может быть пустой).
    ///     Ошибка: ConnectionFailed — проблема с БД.
    /// </returns>
    Result<IReadOnlyCollection<Album>> GetByFolderId(int folderId);
}