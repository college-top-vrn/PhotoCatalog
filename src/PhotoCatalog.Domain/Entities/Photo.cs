using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects.Photo;

namespace PhotoCatalog.Domain.Entities;

/// <summary>
///     Представляет программную доменную сущность физической фотографии, хранящейся в S3-хранилище.
/// </summary>
public sealed class Photo : Entity, IDeeplyCopyable<Photo>
{
    /// <summary>
    ///     Размер фотографии в битах.
    /// </summary>
    public Size Size { get; private set; }

    /// <summary>
    ///     Формат фотографии.
    /// </summary>
    public Mime Mime { get; private set; }

    /// <summary>
    ///     Ключ доступа к физической фотографии в S3-хранилище.
    /// </summary>
    public StorageKey StorageKey { get; private set; }

    /// <summary>
    ///     Метаданные фотографии.
    /// </summary>
    public Metadata Metadata { get; private set; }


    private readonly List<Guid> _tagIds;

    /// <summary>
    ///     Иммутабельный список идентификаторов.
    /// </summary>
    public IImmutableList<Guid> TagIds => _tagIds.ToImmutableList();

    private Photo(
        Guid id,
        Guid userId,
        Size size,
        Mime mime,
        StorageKey storageKey,
        Metadata metadata,
        List<Guid> tagIds)
        : base(id, userId)
    {
        Size = size;
        Mime = mime;
        StorageKey = storageKey;
        Metadata = metadata;
        _tagIds = tagIds;
    }

    /// <summary>
    ///     Создаёт новую фотографию.
    /// </summary>
    /// <param name="id">идентификатор фотографии.</param>
    /// <param name="userId">идентификатор владельца фотографии.</param>
    /// <param name="size">размер фотографии в битах.</param>
    /// <param name="mime">формат фотографии.</param>
    /// <param name="storageKey">ключ доступа к физической фотографии в S3-хранилище.</param>
    /// <param name="metadata">метаданные фотографии.</param>
    /// <param name="tags">список идентификаторов тегов фотографии.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех с созданной фотографией;
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static Result<Photo> Create(
        Guid id,
        Guid userId,
        Size size,
        Mime mime,
        StorageKey storageKey,
        Metadata metadata,
        List<Guid> tags)
    {
        // TODO: реализовать валидатор MIME

        var photo = new Photo(
            id,
            userId,
            size,
            mime,
            storageKey,
            metadata,
            tags
        );

        return Result.Success(photo);
    }

    /// <inheritdoc />
    public Photo DeepCopy()
    {
        List<Guid> newTagIds = _tagIds
            .Select(ti => new Guid(ti.ToString()))
            .ToList();

        Photo clone = new(
            Id,
            UserId,
            Size,
            Mime,
            StorageKey,
            Metadata,
            newTagIds
        );

        return clone;
    }

    /// <summary>
    ///     Изменить размер файла.
    /// </summary>
    /// <param name="newSize">новый размер.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid Resize(Size newSize)
    {
        Size = newSize;

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Изменить MIME-файла.
    /// </summary>
    /// <param name="newMime">новый MIME.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid ChangeMime(Mime newMime)
    {
        Mime = newMime;

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Изменить ключ хранения файла.
    /// </summary>
    /// <param name="newStorageKey">новый ключ хранения.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid ChangeStorageKey(StorageKey newStorageKey)
    {
        StorageKey = newStorageKey;

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Изменить метаданные файла.
    /// </summary>
    /// <param name="newMetadata">новые метаданные.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid ChangeMetadata(Metadata newMetadata)
    {
        Metadata = newMetadata;

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
    public ResultVoid AddTag(Guid id)
    {
        if (_tagIds.Contains(id))
        {
            return ResultVoid.Failure(DomainErrors.Ids.DuplicatedId);
        }

        _tagIds.Add(id);

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
    public ResultVoid DeleteTag(Guid id)
    {
        return _tagIds.Remove(id)
            ? ResultVoid.Success()
            : ResultVoid.Failure(DomainErrors.Ids.IdNotFound);
    }
}