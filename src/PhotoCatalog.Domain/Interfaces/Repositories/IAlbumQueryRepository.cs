using System.Collections.Generic;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Repositories;

/// <summary>
///     Представляет контракт для операций чтения данных об альбомах в соответствии с принципом CQRS.
/// </summary>
/// <remarks>
///     <para>
///         Данный интерфейс предназначен исключительно для детерминированных операций выборки данных,
///         которые гарантированно не изменяют состояние базы данных. Все методы возвращают иммутабельные
///         коллекции (<see cref="IReadOnlyCollection{T}" />) для предотвращения случайной модификации
///         объектов до их обработки бизнес-логикой.
///     </para>
///     <para>
///         Реализации этого интерфейса должны работать независимо от контекста транзакций,
///         открывая собственные легковесные подключения к базе данных без накладных расходов
///         на транзакции и блокировки строк.
///     </para>
/// </remarks>
public interface IAlbumQueryRepository
{
    /// <summary>
    ///     Получает альбом по его уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор альбома.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="Result{T}.Success" /> с найденным альбомом, если альбом существует
    ///                 в системе;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="Result{T}.Failure" /> с ошибкой
    ///                 <see cref="InfrastructureErrors.Database.NotFound" />, если альбом не найден;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="Result{T}.Failure" /> с ошибкой
    ///                 <see cref="InfrastructureErrors.Database.ConnectionFailed" />, если произошла
    ///                 ошибка доступа к данным.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     Метод гарантированно не изменяет состояние базы данных.
    ///     При загрузке альбома также подгружаются все связанные с ним фотографии.
    /// </remarks>
    Result<Album> GetById(int id);

    /// <summary>
    ///     Получает коллекцию всех альбомов, находящихся внутри указанной папки.
    /// </summary>
    /// <param name="folderId">Уникальный идентификатор папки.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="Result{T}.Success" /> с коллекцией альбомов (возможно пустой),
    ///                 если папка существует или не содержит альбомов;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="Result{T}.Failure" /> с ошибкой
    ///                 <see cref="InfrastructureErrors.Database.ConnectionFailed" />, если произошла
    ///                 ошибка доступа к данным.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         Возвращаемая коллекция является иммутабельной (<see cref="IReadOnlyCollection{Album}" />),
    ///         что предотвращает её случайную модификацию.
    ///     </para>
    ///     <para>
    ///         Для каждого альбома в коллекции также подгружаются все связанные фотографии.
    ///     </para>
    ///     <para>
    ///         Если папка не содержит альбомов, возвращается пустая коллекция (не null).
    ///     </para>
    /// </remarks>
    Result<IReadOnlyCollection<Album>> GetByFolderId(int folderId);
}