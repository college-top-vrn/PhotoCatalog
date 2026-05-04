using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Repositories;

/// <summary>
///     Интерфейс репозитория для управления
///     иерархией виртуальных папок.
/// </summary>
public interface IFolderRepository
{
    /// <summary>
    ///     Получить папку по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор папки.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Успех с папкой, если она найдена</description>
    ///         </item>
    ///         <item>
    ///             <description>Ошибка NotFound, если папка не найдена</description>
    ///         </item>
    ///         <item>
    ///             <description>Инфраструктурную ошибку при сбое базы данных</description>
    ///         </item>
    ///     </list>
    /// </returns>
    Result<Folder> GetById(int id);

    /// <summary>
    ///     Добавляет новую папку в репозиторий.
    /// </summary>
    /// <param name="folder">Папка для добавления.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Успех, если папка успешно добавлена</description>
    ///         </item>
    ///         <item>
    ///             <description>Инфраструктурную ошибку при сбое базы данных</description>
    ///         </item>
    ///     </list>
    /// </returns>
    ResultVoid Add(Folder folder);

    /// <summary>
    ///     Обновляет существующую папку.
    /// </summary>
    /// <param name="folder">Обновленная папка.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Успех, если папка успешно обновлена</description>
    ///         </item>
    ///         <item>
    ///             <description>Ошибка NotFound, если папка не найдена</description>
    ///         </item>
    ///         <item>
    ///             <description>Инфраструктурную ошибку при сбое базы данных</description>
    ///         </item>
    ///     </list>
    /// </returns>
    ResultVoid Update(Folder folder);

    /// <summary>
    ///     Удаляет папку по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор папки для удаления.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Успех, если папка успешно удалена</description>
    ///         </item>
    ///         <item>
    ///             <description>Ошибка NotFound, если папка не найдена</description>
    ///         </item>
    ///         <item>
    ///             <description>Инфраструктурную ошибку при сбое базы данных</description>
    ///         </item>
    ///     </list>
    /// </returns>
    ResultVoid Delete(int id);
}