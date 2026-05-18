using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Repositories;

public interface IPhotoCommandRepository
{
    /// <summary>
    ///     Добавляет новую фотографию в репозиторий.
    /// </summary>
    /// <param name="photo">Фото для добавления.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Успех, если фотография успешно добавлена</description>
    ///         </item>
    ///         <item>
    ///             <description>Инфраструктурную ошибку при сбое базы данных</description>
    ///         </item>
    ///     </list>
    /// </returns>
    ResultVoid Add(Photo? photo);

    /// <summary>
    ///     Обновляет существующую фотографию.
    /// </summary>
    /// <param name="photo">Обновленное фото.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Успех, если фотография успешно обновлена</description>
    ///         </item>
    ///         <item>
    ///             <description>Ошибка NotFound, если фотография не найдена</description>
    ///         </item>
    ///         <item>
    ///             <description>Инфраструктурную ошибку при сбое базы данных</description>
    ///         </item>
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
    ///         <item>
    ///             <description>Успех, если фотография успешно удалена</description>
    ///         </item>
    ///         <item>
    ///             <description>Ошибка NotFound, если фотография не найдена</description>
    ///         </item>
    ///         <item>
    ///             <description>Инфраструктурную ошибку при сбое базы данных</description>
    ///         </item>
    ///     </list>
    /// </returns>
    ResultVoid Delete(int id);
}