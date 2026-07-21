using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Entities;

/// <summary>
///     Представляет доменную сущность альбом.
/// </summary>
/// <remarks>
///     Отвечает за содержимое списка идентификаторов фотографий.
/// </remarks>
public sealed class Album : Entity, IDeeplyCopyable<Album>
{
    /// <summary>
    ///     Имя.
    /// </summary>
    public string Name { get; private set; }

    private readonly List<Guid> _photoIds = [];

    /// <summary>
    ///     Иммутабельная коллекция идентификаторов фотографий, принадлежащих альбому.
    /// </summary>
    public IImmutableList<Guid> PhotoIds => _photoIds.ToImmutableList();

    /// <summary>
    ///     Инициализирует новый экземпляр класса <see cref="Album" /> с указанным идентификатором и именем.
    /// </summary>
    /// <param name="id">идентификатор.</param>
    /// <param name="name">имя.</param>
    private Album(Guid id, string name) : base(id) => Name = name;

    /// <summary>
    ///     Создаёт новый экземпляр альбома с проверкой валидности наименования.
    /// </summary>
    /// <param name="id">идентификатор создаваемого альбома.</param>
    /// <param name="name">имя создаваемого альбома.</param>
    /// <returns>
    ///     Результат операции:
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
    public static Result<Album> Create(Guid id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Album>(DomainErrors.Album.EmptyName);
        }

        string trimmedName = name.Trim();

        return Result.Success(new Album(id, trimmedName));
    }

    /// <inheritdoc />
    public Album DeepCopy()
    {
        Album clone = new(Id, Name);

        clone._photoIds.AddRange(_photoIds);

        return clone;
    }

    /// <summary>
    ///     Изменяет имя альбома на новое значение.
    /// </summary>
    /// <param name="newName">новое имя альбома.</param>
    /// <returns>
    ///     Результат операции:
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
    ///     Восстанавливает коллекцию идентификаторов фотографий при материализации объекта из базы данных.
    /// </summary>
    /// <param name="photoIds">коллекция идентификаторов фотографий для восстановления.</param>
    /// <returns>всегда успешный результат выполнения операции.</returns>
    internal ResultVoid RestorePhotos(IEnumerable<Guid> photoIds)
    {
        _photoIds.AddRange(photoIds);

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Добавляет идентификатор фотографии в альбом.
    /// </summary>
    /// <param name="photoId">идентификатор добавляемой фотографии.</param>
    /// <returns>
    ///     Результат операции:
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
    ///     Удаляет фотографию из альбома.
    /// </summary>
    /// <param name="photoId">идентификатор удаляемой фотографии.</param>
    /// <returns>
    /// Результат операции:
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