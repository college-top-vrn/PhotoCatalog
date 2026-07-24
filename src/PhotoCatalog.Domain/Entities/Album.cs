using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Entities;

/// <summary>
///     Представляет доменную сущность альбом.
/// </summary>
public sealed class Album : Entity, IDeeplyCopyable<Album>
{
    /// <summary>
    ///     Имя альбома.
    /// </summary>
    public string Name { get; private set; }

    private readonly List<Guid> _photoIds;

    /// <summary>
    ///     Иммутабельный список идентификаторов фотографий альбома.
    /// </summary>
    public IImmutableList<Guid> PhotoIds => _photoIds.ToImmutableList();

    private Album(
        Guid id,
        Guid userId,
        string name,
        List<Guid> photoIds
    ) : base(id, userId)
    {
        Name = name;
        _photoIds = photoIds;
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
    public static Result<Album> Create(Guid id, Guid userId, string name, List<Guid> photoIds)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Album>(DomainErrors.Album.EmptyName);
        }

        string trimmedName = name.Trim();

        return Result.Success(new Album(id, userId, trimmedName, photoIds));
    }

    /// <inheritdoc />
    public Album DeepCopy()
    {
        List<Guid> photoIdsCopy = _photoIds.ConvertAll(photoId => Guid.Parse(photoId.ToString()));

        Album clone = new(Id, UserId, Name, photoIdsCopy);

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
        if (string.IsNullOrWhiteSpace(newName))
        {
            return ResultVoid.Failure(DomainErrors.Album.EmptyName);
        }

        Name = newName.Trim();

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Добавляет идентификатор фотографии в альбом.
    /// </summary>
    /// <param name="photoId">идентификатор фотографии.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех при успешном добавлении фотографии;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Album.DuplicatePhoto" />,
    ///                 если фотография уже есть в альбоме.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid AddPhoto(Guid photoId)
    {
        if (_photoIds.Contains(photoId))
        {
            return ResultVoid.Failure(DomainErrors.Album.DuplicatePhoto);
        }

        _photoIds.Add(photoId);

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Удаляет идентификатор фотографии из альбома.
    /// </summary>
    /// <param name="photoId">идентификатор фотографии.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех при успешном удалении фотографии;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.Album.NotFound" />,
    ///                 если фотография не была найдена.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid RemovePhoto(Guid photoId)
    {
        return _photoIds.Remove(photoId)
            ? ResultVoid.Success()
            : ResultVoid.Failure(DomainErrors.Album.NotFound);
    }
}