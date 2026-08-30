using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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
    ///     Цвет альбома.
    /// </summary>
    public ColorHex ColorHex { get; private set; }

    private readonly List<Guid> _photoIds;

    /// <summary>
    ///     Иммутабельный список идентификаторов.
    /// </summary>
    public IImmutableList<Guid> PhotoIds => _photoIds.ToImmutableList();

    private Album(
        Guid id,
        Guid userId,
        Name name,
        ColorHex colorHex,
        List<Guid> photoIds
    ) : base(id, userId)
    {
        Name = name;
        ColorHex = colorHex;
        _photoIds = photoIds;
    }

    /// <summary>
    ///     Создаёт новый альбом.
    /// </summary>
    /// <param name="id">идентификатор альбома.</param>
    /// <param name="userId">идентификатор владельца альбома.</param>
    /// <param name="name">имя альбома.</param>
    /// <param name="colorHex"></param>
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
    public static Result<Album> Create(
        Guid id,
        Guid userId,
        Name name,
        ColorHex colorHex,
        List<Guid> photoIds
    )
    {
        return Result.Success(new Album(
            id,
            userId,
            name,
            colorHex,
            photoIds
        ));
    }

    /// <inheritdoc />
    public Album DeepCopy()
    {
        List<Guid> newPhotoIds = _photoIds
            .Select(pi => new Guid(pi.ToString()))
            .ToList();

        Album clone = new(Id, UserId, Name, ColorHex, newPhotoIds);

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
    public ResultVoid Rename(Name newName)
    {
        Name = newName;

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Перекрашивает альбом.
    /// </summary>
    /// <param name="newColor">новый цвет.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid Recolor(ColorHex newColor)
    {
        ColorHex = newColor;

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
    public ResultVoid AddPhoto(Guid id)
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
    public ResultVoid DeletePhoto(Guid id)
    {
        return _photoIds.Remove(id)
            ? ResultVoid.Success()
            : ResultVoid.Failure(DomainErrors.Ids.IdNotFound);
    }
}