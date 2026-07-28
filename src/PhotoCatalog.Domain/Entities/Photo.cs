using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using PhotoCatalog.Domain.Interfaces;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects.Photo;

namespace PhotoCatalog.Domain.Entities;

/// <summary>
///     Представляет программную доменную сущность физической фотографии, хранящейся в S3-хранилище.
/// </summary>
public sealed class Photo : ObservableEntity, IDeeplyCopyable<Photo>
{
    /// <summary>
    ///     Дата и время съёмки фотографии.
    /// </summary>
    public CapturedAt CapturedAt { get; }

    /// <summary>
    ///     Размер фотографии в битах.
    /// </summary>
    public Size Size { get; }

    /// <summary>
    ///     Формат фотографии.
    /// </summary>
    public Mime Mime { get; }

    /// <summary>
    ///     Ключ доступа к физической фотографии в S3-хранилище.
    /// </summary>
    public StorageKey StorageKey { get; }

    /// <summary>
    ///     Метаданные фотографии.
    /// </summary>
    public Metadata Metadata { get; }


    private readonly List<Guid> _tagIds;

    /// <summary>
    ///     Иммутабельный список идентификаторов.
    /// </summary>
    public IImmutableList<Guid> TagIds => _tagIds.ToImmutableList();

    private Photo(
        Guid id,
        Guid userId,
        CapturedAt capturedAt,
        Size size,
        Mime mime,
        StorageKey storageKey,
        Metadata metadata,
        List<Guid> tagIds)
        : base(id, userId)
    {
        CapturedAt = capturedAt;
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
    /// <param name="capturedAt">дата и время съёмки фотографии.</param>
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
        CapturedAt capturedAt,
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
            capturedAt,
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
        Photo clone = new(
            Id,
            UserId,
            CapturedAt,
            Size,
            Mime,
            StorageKey,
            Metadata,
            _tagIds
        );

        return clone;
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
    public ResultVoid AttachTag(Guid id)
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
    public ResultVoid DetachTag(Guid id)
    {
        return _tagIds.Remove(id)
            ? ResultVoid.Success()
            : ResultVoid.Failure(DomainErrors.Ids.IdNotFound);
    }
}