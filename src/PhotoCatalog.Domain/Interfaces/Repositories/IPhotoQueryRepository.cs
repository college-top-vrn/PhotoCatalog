using System.Collections.Generic;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Repositories;

public interface IPhotoQueryRepository
{
    /// <summary>
    ///     Получает фотографию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор фотографии.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Успех с фотографией, если она найдена</description>
    ///         </item>
    ///         <item>
    ///             <description>Ошибка NotFound, если фотография не найдена</description>
    ///         </item>
    ///         <item>
    ///             <description>Инфраструктурную ошибку при сбое базы данных</description>
    ///         </item>
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
    ///         <item>
    ///             <description>Успех с фотографией, если она найдена</description>
    ///         </item>
    ///         <item>
    ///             <description>Ошибка NotFound, если фотография не найдена</description>
    ///         </item>
    ///         <item>
    ///             <description>Инфраструктурную ошибку при сбое базы данных</description>
    ///         </item>
    ///     </list>
    /// </returns>
    Result<Photo> GetByPath(string realPath);

    /// <summary>
    ///     Получает все фотографии альбома.
    /// </summary>
    /// <param name="albumId">Идентификатор альбома.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Успех с неизменяемой коллекцией фотографий альбома (может быть пустой)</description>
    ///         </item>
    ///         <item>
    ///             <description>Инфраструктурную ошибку при сбое базы данных</description>
    ///         </item>
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
    ///         <item>
    ///             <description>Успех с неизменяемой коллекцией фотографий, содержащих указанные теги (может быть пустой)</description>
    ///         </item>
    ///         <item>
    ///             <description>Инфраструктурную ошибку при сбое базы данных</description>
    ///         </item>
    ///     </list>
    /// </returns>
    Result<IReadOnlyCollection<Photo>> GetByTags(IEnumerable<int> tagIds);
    /// <summary>
    ///     Получает все фотографии.
    /// </summary>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Успех со списком фотографий(может быть пустой)</description>
    ///         </item>
    ///         <item>
    ///             <description>Инфраструктурную ошибку при сбое базы данных</description>
    ///         </item>
    ///     </list>
    /// </returns>
    Result<IEnumerable<Photo>> GetAll();
}