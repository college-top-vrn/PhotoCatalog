using System.Collections.Generic;

using PhotoCatalog.Domain.Entities;

namespace PhotoCatalog.Domain.Interfaces.Repositories;

/// <summary>
///     Интерфейс репозитория для работы с сущностью Photo.
///     Определяет операции для получения и сохранения фотографий.
/// </summary>
public interface IPhotoRepository
{
    /// <summary>
    ///     Получает фотографию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор фотографии.</param>
    /// <returns>Фото или null, если не найдено.</returns>
    Photo? GetById(int id);

    /// <summary>
    ///     Получает фотографию по реальному пути к файлу.
    /// </summary>
    /// <param name="realPath">Реальный путь к файлу фотографии.</param>
    /// <returns>Фото или null, если не найдено.</returns>
    Photo? GetByPath(string realPath);

    /// <summary>
    ///     Добавляет новую фотографию в репозиторий.
    /// </summary>
    /// <param name="photo">Фото для добавления.</param>
    void Add(Photo photo);

    /// <summary>
    ///     Обновляет существующую фотографию.
    /// </summary>
    /// <param name="photo">Обновленное фото.</param>
    void Update(Photo photo);

    /// <summary>
    ///     Удаляет фотографию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор фото для удаления.</param>
    void Delete(int id);

    /// <summary>
    ///     Получает все фотографии альбома.
    /// </summary>
    /// <param name="albumId">Идентификатор альбома.</param>
    /// <returns>Неизменяемая коллекция фотографий альбома.</returns>
    IReadOnlyCollection<Photo> GetByAlbumId(int albumId);

    /// <summary>
    ///     Получает фотографии по списку идентификаторов тегов.
    /// </summary>
    /// <param name="tagIds">Идентификаторы тегов.</param>
    /// <returns>Неизменяемая коллекция фотографий с указанными тегами.</returns>
    IReadOnlyCollection<Photo> GetByTags(IEnumerable<int> tagIds);
}