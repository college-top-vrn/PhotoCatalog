using System;
using System.Collections.Generic;
using System.Collections.Immutable;

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

    private readonly List<Guid> _photoIds;

    /// <summary>
    ///     Иммутабельный список идентификаторов.
    /// </summary>
    public IImmutableList<Guid> PhotoIds => _photoIds.ToImmutableList();

    private Album(Guid id, Guid userId, Name name, List<Guid> photoIds) : base(id, userId)
    {
        Name = name;
        _photoIds = photoIds;
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
        Album clone = new(Id, UserId, Name, _photoIds);

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
    
    /// <summary>
    ///     Добавляет идентификатор в список.
    /// </summary>
    /// <param name="id">идентификатор.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Ids.DuplicatedId"/>, если данный тег уже привязан.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid Add(Guid id)
    {
        if (_photoIds.Contains(id))
        {
            return ResultVoid.Failure(DomainErrors.Ids.DuplicatedId);
        }

        _photoIds.Add(id);
        return ResultVoid.Success();
    }

    /// <summary>
    ///     Удаляет идентификатор из списка.
    /// </summary>
    /// <param name="id">идентификатор.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Ids.IdNotFound"/>, если тег не найден.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid Remove(Guid id)
    {
        return _photoIds.Remove(id)
            ? ResultVoid.Success()
            : ResultVoid.Failure(DomainErrors.Ids.IdNotFound);
    }
}