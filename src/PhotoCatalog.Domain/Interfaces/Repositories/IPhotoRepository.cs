using System.Collections.Generic;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Repositories;

/// <summary>
///     Интерфейс репозитория для работы с сущностью Photo.
///     Определяет операции для получения и сохранения фотографий.
///     Использует паттерн Result для безопасной передачи инфраструктурных ошибок.
/// </summary>
public interface IPhotoRepository
{
    /// <summary>
    ///     Получает фотографию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор фотографии.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item><description>Успех с фотографией, если она найдена</description></item>
    ///         <item><description>Ошибка NotFound, если фотография не найдена</description></item>
    ///         <item><description>Инфраструктурную ошибку при сбое базы данных</description></item>
    ///     </list>
    /// </returns>
    Result<Photo> GetById(int id);

    /// <summary>
    ///     Получает фотографию по реальному пути к файлу.
    /// </summary>
    /// <param name="realPath">Реальный путь к файлу фотографии.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item><description>Успех с фотографией, если она найдена</description></item>
    ///         <item><description>Ошибка NotFound, если фотография не найдена</description></item>
    ///         <item><description>Инфраструктурную ошибку при сбое базы данных</description></item>
    ///     </list>
    /// </returns>
    Result<Photo> GetByPath(string realPath);

    /// <summary>
    ///     Добавляет новую фотографию в репозиторий.
    /// </summary>
    /// <param name="photo">Фото для добавления.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item><description>Успех, если фотография успешно добавлена</description></item>
    ///         <item><description>Инфраструктурную ошибку при сбое базы данных</description></item>
    ///     </list>
    /// </returns>
    ResultVoid Add(Photo photo);

    /// <summary>
    ///     Обновляет существующую фотографию.
    /// </summary>
    /// <param name="photo">Обновленное фото.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item><description>Успех, если фотография успешно обновлена</description></item>
    ///         <item><description>Ошибка NotFound, если фотография не найдена</description></item>
    ///         <item><description>Инфраструктурную ошибку при сбое базы данных</description></item>
    ///     </list>
    /// </returns>
    ResultVoid Update(Photo photo);

    /// <summary>
    ///     Удаляет фотографию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор фото для удаления.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item><description>Успех, если фотография успешно удалена</description></item>
    ///         <item><description>Ошибка NotFound, если фотография не найдена</description></item>
    ///         <item><description>Инфраструктурную ошибку при сбое базы данных</description></item>
    ///     </list>
    /// </returns>
    ResultVoid Delete(int id);

    /// <summary>
    ///     Получает все фотографии альбома.
    /// </summary>
    /// <param name="albumId">Идентификатор альбома.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item><description>Успех с неизменяемой коллекцией фотографий альбома (может быть пустой)</description></item>
    ///         <item><description>Инфраструктурную ошибку при сбое базы данных</description></item>
    ///     </list>
    /// </returns>
    Result<IReadOnlyCollection<Photo>> GetByAlbumId(int albumId);

    /// <summary>
    ///     Получает фотографии по списку идентификаторов тегов.
    /// </summary>
    /// <param name="tagIds">Идентификаторы тегов.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item><description>Успех с неизменяемой коллекцией фотографий, содержащих указанные теги (может быть пустой)</description></item>
    ///         <item><description>Инфраструктурную ошибку при сбое базы данных</description></item>
    ///     </list>
    /// </returns>
    Result<IReadOnlyCollection<Photo>> GetByTags(IEnumerable<int> tagIds);
}