using System.Collections.Generic;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Repositories;

/// <summary>
///     Интерфейс репозитория для получения
///     данных из иерархии виртуальных папок.
/// </summary>
public interface IFolderQueryRepository
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
}