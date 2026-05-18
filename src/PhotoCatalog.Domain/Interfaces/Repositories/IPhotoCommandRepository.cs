using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Repositories;

/// <summary>
///     Репозиторий команд для фотографий (CQS — операции записи).
///     Содержит методы изменения состояния системы (Add, Update, Delete),
///     которые должны выполняться в рамках транзакции, управляемой <see cref="IUnitOfWork"/>.
/// </summary>
/// <remarks>
///     Реализация:
///     - Не должна самостоятельно фиксировать транзакцию (Commit/Rollback).
///     - Должна использовать подключение и транзакцию, предоставляемые текущим экземпляром SqliteUnitOfWork.
///     - Все операции возвращают <see cref="ResultVoid"/> для типизированной обработки ошибок без исключений.
/// </remarks>
public interface IPhotoCommandRepository
{
    /// <summary>
    ///     Добавляет новую фотографию в репозиторий.
    /// </summary>
    /// <param name="photo">Фото для добавления. Не должно быть null.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item><description>Успех, если фотография успешно добавлена в базу данных.</description></item>
    ///         <item><description>Инфраструктурную ошибку при сбое базы данных.</description></item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     - Операция выполняется в рамках транзакции, управляемой <see cref="IUnitOfWork"/>.
    ///     - При необходимости проверки внешних ключей (например, AlbumId) используется включённый режим PRAGMA foreign_keys = ON.
    /// </remarks>
    ResultVoid Add(Photo? photo);

    /// <summary>
    ///     Обновляет существующую фотографию.
    /// </summary>
    /// <param name="photo">Обновлённое фото. Не должно быть null.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item><description>Успех, если фотография успешно обновлена.</description></item>
    ///         <item><description>Ошибка NotFound, если фотография не найдена.</description></item>
    ///         <item><description>Инфраструктурную ошибку при сбое базы данных.</description></item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     - Метод обновляет только существующую запись по её идентификатору.
    ///     - Операция выполняется в рамках транзакции, управляемой <see cref="IUnitOfWork"/>.
    /// </remarks>
    ResultVoid Update(Photo photo);

    /// <summary>
    ///     Удаляет фотографию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор фото для удаления.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item><description>Успех, если фотография успешно удалена.</description></item>
    ///         <item><description>Ошибка NotFound, если фотография не найдена.</description></item>
    ///         <item><description>Инфраструктурную ошибку при сбое базы данных.</description></item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     - Удаление выполняется в рамках транзакции, управляемой <see cref="IUnitOfWork"/>.
    ///     - SQLite проверяет внешние ключи при включённом PRAGMA foreign_keys = ON.
    /// </remarks>
    ResultVoid Delete(int id);
}