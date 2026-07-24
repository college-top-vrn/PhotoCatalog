using System;
using System.Collections.Generic;
using System.Text.Json;

using PhotoCatalog.Domain.Interfaces;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;
using PhotoCatalog.Domain.ValueObjects.Photo;

namespace PhotoCatalog.Domain.Entities;

/// <summary>
///     Представляет программную сущность физической фотографии, хранящейся в S3-хранилище.
/// </summary>
public sealed class Photo : Entity, IDeeplyCopyable<Photo>
{
    /// <summary>
    ///     Дата и время съёмки фотографии.
    /// </summary>
    public CapturedAt CapturedAt { get; }

    /// <summary>
    ///     Размер фотографии в битах.
    /// </summary>
    public PhotoSize PhotoSize { get; }

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
    public JsonDocument Metadata { get; }

    /// <summary>
    ///     Репозиторий для управлением списком идентификаторов тегов.
    /// </summary>
    public IdRepository TagIds { get; }

    private Photo(
        Guid id,
        Guid userId,
        CapturedAt capturedAt,
        PhotoSize photoSize,
        Mime mime,
        StorageKey storageKey,
        JsonDocument metadata,
        List<Guid> tagIds) : base(id, userId)
    {
        CapturedAt = capturedAt;
        PhotoSize = photoSize;
        Mime = mime;
        StorageKey = storageKey;
        Metadata = metadata;
        TagIds = IdRepository.Create(tagIds).Value!;
    }

    /// <summary>
    ///     Создаёт новую фотографию.
    /// </summary>
    /// <param name="id">идентификатор фотографии.</param>
    /// <param name="userId">идентификатор владельца фотографии.</param>
    /// <param name="capturedAt">дата и время съёмки фотографии.</param>
    /// <param name="photoSize">размер фотографии в битах.</param>
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
        PhotoSize photoSize,
        Mime mime,
        StorageKey storageKey,
        JsonDocument metadata,
        List<Guid> tags)
    {
        // TODO: реализовать валидатор MIME

        var photo = new Photo(
            id,
            userId,
            capturedAt,
            photoSize,
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
        List<Guid> tagIds = new(TagIds.Ids);

        Photo clone = new(
            Id,
            UserId,
            CapturedAt,
            PhotoSize,
            Mime,
            StorageKey,
            Metadata,
            tagIds
        );

        return clone;
    }
}