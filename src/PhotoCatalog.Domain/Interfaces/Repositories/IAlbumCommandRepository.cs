using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Repositories;

/// <summary>
///     Представляет контракт для операций записи данных об альбомах в соответствии с принципом CQRS.
/// </summary>
/// <remarks>
///     <para>
///         Данный интерфейс содержит методы, которые изменяют состояние системы (создание, обновление, удаление).
///         Все операции записи должны быть строго интегрированы с <see cref="IUnitOfWork" /> для обеспечения
///         атомарности и целостности данных.
///     </para>
///     <para>
///         <b>Важно:</b> Реализации этого интерфейса НЕ должны вызывать фиксацию транзакции самостоятельно.
///         Управление транзакциями (Commit/Rollback) является ответственностью вызывающего кода (обработчика/сервиса),
///         который использует <see cref="IUnitOfWork" />.
///     </para>
/// </remarks>
public interface IAlbumCommandRepository
{
    /// <summary>
    ///     Добавляет новый альбом в систему.
    /// </summary>
    /// <param name="album">Объект альбома для добавления. Не может быть null.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="ResultVoid.Success" />, если альбом успешно добавлен в базу данных;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ResultVoid.Failure" /> с ошибкой
    ///                 <see cref="DomainErrors.Album.NullAlbum" />, если передан null;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ResultVoid.Failure" /> с ошибкой
    ///                 <see cref="InfrastructureErrors.Database.ConstraintViolation" />, если
    ///                 <see cref="Album.FolderId" /> ссылается на несуществующую папку или
    ///                 <see cref="Album.Id" /> уже существует;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ResultVoid.Failure" /> с ошибкой
    ///                 <see cref="InfrastructureErrors.Database.ConnectionFailed" />, если произошла
    ///                 ошибка доступа к данным.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     Метод добавляет не только запись в таблицу Albums, но и все связи с фотографиями
    ///     в таблицу AlbumPhotos. Операция выполняется в контексте текущей транзакции
    ///     <see cref="IUnitOfWork" />.
    /// </remarks>
    ResultVoid Add(Album album);

    /// <summary>
    ///     Обновляет данные существующего альбома.
    /// </summary>
    /// <param name="album">Объект альбома с обновленными данными. Не может быть null.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="ResultVoid.Success" />, если альбом успешно обновлен;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ResultVoid.Failure" /> с ошибкой
    ///                 <see cref="DomainErrors.Album.NullAlbum" />, если передан null;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ResultVoid.Failure" /> с ошибкой
    ///                 <see cref="InfrastructureErrors.Database.NotFound" />, если альбом с указанным
    ///                 идентификатором не существует;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ResultVoid.Failure" /> с ошибкой
    ///                 <see cref="InfrastructureErrors.Database.ConstraintViolation" />, если
    ///                 <see cref="Album.FolderId" /> ссылается на несуществующую папку;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ResultVoid.Failure" /> с ошибкой
    ///                 <see cref="InfrastructureErrors.Database.ConnectionFailed" />, если произошла
    ///                 ошибка доступа к данным.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     Метод полностью синхронизирует связи альбома с фотографиями:
    ///     <list type="number">
    ///         <item>
    ///             <description>Удаляет все существующие связи альбома с фотографиями;</description>
    ///         </item>
    ///         <item>
    ///             <description>Добавляет новые связи из <see cref="Album.PhotoIds" />.</description>
    ///         </item>
    ///     </list>
    ///     Операция выполняется в контексте текущей транзакции <see cref="IUnitOfWork" />.
    /// </remarks>
    ResultVoid Update(Album album);

    /// <summary>
    ///     Удаляет альбом из системы по его идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор альбома для удаления.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="ResultVoid.Success" />, если альбом успешно удален;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ResultVoid.Failure" /> с ошибкой
    ///                 <see cref="InfrastructureErrors.Database.NotFound" />, если альбом с указанным
    ///                 идентификатором не существует;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ResultVoid.Failure" /> с ошибкой
    ///                 <see cref="InfrastructureErrors.Database.ConstraintViolation" />, если существуют
    ///                 внешние ключи, ссылающиеся на этот альбом (если не настроено каскадное удаление);
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ResultVoid.Failure" /> с ошибкой
    ///                 <see cref="InfrastructureErrors.Database.ConnectionFailed" />, если произошла
    ///                 ошибка доступа к данным.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         При удалении альбома также автоматически удаляются все связи этого альбома
    ///         с фотографиями из таблицы AlbumPhotos (каскадное удаление).
    ///     </para>
    ///     <para>
    ///         Сами фотографии при этом не удаляются, только их принадлежность к данному альбому.
    ///     </para>
    ///     <para>
    ///         Операция выполняется в контексте текущей транзакции <see cref="IUnitOfWork" />.
    ///     </para>
    /// </remarks>
    ResultVoid Delete(int id);
}