using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using PhotoCatalog.Domain.Interfaces;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;

namespace PhotoCatalog.Domain.Entities;

/// <summary>
///     Представляет доменную сущность альбом.
/// </summary>
public sealed class Album : Entity, IDeeplyCopyable<Album>
{
    /// <summary>
    ///     Имя альбома.
    /// </summary>
    public Name Name { get; private set; }

    /// <summary>
    ///     Репозиторий идентификаторов фотографий.
    /// </summary>
    public IdRepository PhotoIds { get; }

    private Album(
        Guid id,
        Guid userId,
        Name name,
        List<Guid> photoIds
    ) : base(id, userId)
    {
        Name = name;
        PhotoIds = IdRepository.Create(photoIds).Value!;
    }

    /// <summary>
    ///     Создаёт новый альбом.
    /// </summary>
    /// <param name="id">идентификатор альбома.</param>
    /// <param name="userId">идентификатор владельца альбома</param>
    /// <param name="name">имя альбома.</param>
    /// <param name="photoIds">список идентификаторов фотографий альбома.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех с созданным альбомом;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Album.EmptyName" />, если имя пустое.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static Result<Album> Create(
        Guid id,
        Guid userId,
        string name,
        List<Guid> photoIds)
    {
        var result = Name.Create(name);

        return result.IsFailure
            ? Result.Failure<Album>(result.ResultError)
            : Result.Success(new Album(
                id,
                userId,
                result.Value!,
                photoIds
            ));
    }

    /// <inheritdoc />
    public Album DeepCopy()
    {
        List<Guid> photoIds = new(PhotoIds.Ids);

        Album clone = new(
            Id,
            UserId,
            Name,
            photoIds
        );

        return clone;
    }

    /// <summary>
    ///     Изменяет имя альбома на новое значение.
    /// </summary>
    /// <param name="newName">новое имя альбома.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех при успешном переименовании;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Album.EmptyName" />, если новое имя пустое.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid Rename(string newName)
    {
        var result = Name.Create(newName);

        if (result.IsFailure)
        {
            return ResultVoid.Failure(result.ResultError);
        }

        Name = result.Value!;

        return ResultVoid.Success();
    }
}