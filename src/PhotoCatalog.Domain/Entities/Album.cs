using System;
using System.Collections.Generic;

using PhotoCatalog.Domain.Interfaces;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;

namespace PhotoCatalog.Domain.Entities;

/// <summary>
///     Представляет программную доменную сущность альбом.
/// </summary>
public sealed class Album : Entity, IDeeplyCopyable<Album>
{
    /// <summary>
    ///     Имя альбома.
    /// </summary>
    public Name Name { get; private set; }

    /// <summary>
    ///     Репозиторий идентификаторов фотографий альбома.
    /// </summary>
    public IdRepository PhotoIdRepository { get; }

    private Album(Guid id, Guid userId, Name name, List<Guid> photoIds) : base(id, userId)
    {
        Name = name;
        PhotoIdRepository = IdRepository.Create(photoIds).Value!;
    }

    /// <summary>
    ///     Создаёт новый альбом.
    /// </summary>
    /// <param name="id">идентификатор альбома.</param>
    /// <param name="userId">идентификатор владельца альбома.</param>
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
    ///                 Ошибка <see cref="DomainErrors.Name.IsEmpty" />, если имя пустое;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Name.IsTooLong" />, если длина имени больше 50.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static Result<Album> Create(Guid id, Guid userId, string name, List<Guid> photoIds)
    {
        Result<Name> result = Name.Create(name);

        return result.IsSuccess
            ? Result.Success(new Album(id, userId, result.Value!, photoIds))
            : Result.Failure<Album>(result.ResultError);
    }

    /// <inheritdoc />
    public Album DeepCopy()
    {
        List<Guid> photoIds = new(PhotoIdRepository.Ids);

        Album clone = new(Id, UserId, Name, photoIds);

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
    ///                 Ошибка <see cref="DomainErrors.Name.IsEmpty" />, если имя пустое;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Name.IsTooLong" />, если длина имени больше 50.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid Rename(string newName)
    {
        Result<Name> result = Name.Create(newName);

        if (result.IsFailure)
        {
            return ResultVoid.Failure(result.ResultError);
        }

        Name = result.Value!;

        return ResultVoid.Success();
    }
}