using System.Collections.Generic;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Repositories;

/// <summary>
///     Репозиторий запросов для фотографий (CQS — операции чтения).
///     Содержит только методы выборки данных; не изменяет состояние базы данных.
/// </summary>
/// <remarks>
///     Реализация:
///     - Может работать независимо от транзакций записи.
///     - Для чтения используются отдельные, легковесные подключения к SQLite.
///     - Все операции чтения возвращают <see cref="Result{T}"/> для унифицированной обработки ошибок.
///     - Коллекции результатов возвращаются как неизменяемые (IReadOnlyCollection&lt;T&gt;) для защиты от модификаций.
/// </remarks>
public interface IPhotoQueryRepository
{
    /// <summary>
    ///     Получает фотографию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор фотографии.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item><description>Успех с фотографией, если она найдена.</description></item>
    ///         <item><description>Ошибка NotFound, если фотография не найдена.</description></item>
    ///         <item><description>Инфраструктурную ошибку при сбое базы данных.</description></item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     - Не изменяет состояние базы данных.
    ///     - Может выполняться вне транзакции записи.
    /// </remarks>
    Result<Photo> GetById(int id);

    /// <summary>
    ///     Получает фотографию по реальному пути к файлу.
    /// </summary>
    /// <param name="realPath">Реальный путь к файлу фотографии.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item><description>Успех с фотографией, если она найдена.</description></item>
    ///         <item><description>Ошибка NotFound, если фотография не найдена.</description></item>
    ///         <item><description>Инфраструктурную ошибку при сбое базы данных.</description></item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     - Используется для поиска сущности по физическому пути к файлу.
    ///     - Не изменяет состояние базы данных.
    /// </remarks>
    Result<Photo> GetByPath(string realPath);

    /// <summary>
    ///     Получает все фотографии альбома.
    /// </summary>
    /// <param name="albumId">Идентификатор альбома.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item><description>Успех с неизменяемой коллекцией фотографий альбома (может быть пустой).</description></item>
    ///         <item><description>Инфраструктурную ошибку при сбое базы данных.</description></item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     - Коллекция возвращается как IReadOnlyCollection&lt;Photo&gt; для защиты от модификаций.
    ///     - Не изменяет состояние базы данных.
    /// </remarks>
    Result<IReadOnlyCollection<Photo>> GetByAlbumId(int albumId);

    /// <summary>
    ///     Получает фотографии по списку идентификаторов тегов.
    /// </summary>
    /// <param name="tagIds">Идентификаторы тегов.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item><description>Успех с неизменяемой коллекцией фотографий, содержащих указанные теги (может быть пустой).</description></item>
    ///         <item><description>Инфраструктурную ошибку при сбое базы данных.</description></item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     - Коллекция возвращается как IReadOnlyCollection&lt;Photo&gt; для защиты от модификаций.
    ///     - Не изменяет состояние базы данных.
    /// </remarks>
    Result<IReadOnlyCollection<Photo>> GetByTags(IEnumerable<int> tagIds);

    /// <summary>
    ///     Получает все фотографии.
    /// </summary>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item><description>Успех со списком фотографий (может быть пустой).</description></item>
    ///         <item><description>Инфраструктурную ошибку при сбое базы данных.</description></item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     - Возвращает Enumerable, что позволяет ленивую и постраничную загрузку.
    ///     - Не изменяет состояние базы данных.
    /// </remarks>
    Result<IEnumerable<Photo>> GetAll();
}