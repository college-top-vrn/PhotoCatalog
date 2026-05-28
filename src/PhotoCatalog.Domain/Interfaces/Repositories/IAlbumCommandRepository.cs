using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Repositories;

/// <summary>
///     Контракт для операций записи альбомов в соответствии с CQRS.
/// </summary>
/// <remarks>
///     Содержит методы, изменяющие состояние системы.
///     Реализации используют подключение и транзакцию от IUnitOfWork.
///     Репозиторий НЕ вызывает Commit — это ответственность вызывающего кода.
/// </remarks>
public interface IAlbumCommandRepository
{
    /// <summary>
    ///     Добавляет новый альбом в систему.
    /// </summary>
    /// <param name="album">Объект альбома для добавления.</param>
    /// <returns>
    ///     Успех: альбом добавлен.
    ///     Ошибка: NullAlbum — передан null, ConstraintViolation — нарушены ограничения,
    ///     ConnectionFailed — проблема с БД.
    /// </returns>
    ResultVoid Add(Album album);

    /// <summary>
    ///     Обновляет данные существующего альбома.
    /// </summary>
    /// <param name="album">Объект с обновленными данными.</param>
    /// <returns>
    ///     Успех: альбом обновлен.
    ///     Ошибка: NullAlbum — передан null, NotFound — альбом не существует,
    ///     ConstraintViolation — нарушены ограничения, ConnectionFailed — проблема с БД.
    /// </returns>
    ResultVoid Update(Album album);

    /// <summary>
    ///     Удаляет альбом из системы по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор альбома.</param>
    /// <returns>
    ///     Успех: альбом удален.
    ///     Ошибка: NotFound — альбом не существует,
    ///     ConstraintViolation — есть зависимые записи, ConnectionFailed — проблема с БД.
    /// </returns>
    ResultVoid Delete(int id);
}