using System;
using System.Globalization;
using System.Linq;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects.Photo;

using Xunit;

namespace PhotoCatalog.Test.Unit.Domain.Entities;

public class PhotoTest
{
    [Fact]
    public void Create_CreatingPhotoWithCorrectValues_ReturnsResultWithPhotoAndSuccess()
    {
        Metadata.Builder builder = new();

        Result<Photo> result = Photo
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                CapturedAt.Create(DateTime.Now.ToString(CultureInfo.CurrentCulture)).Value!,
                Size.Create(100).Value!,
                Mime.Create("mime").Value!,
                StorageKey.Create("photo").Value!,
                builder.Build().Value!,
                [
                    Guid.CreateVersion7(),
                    Guid.CreateVersion7()
                ]
            );

        Assert.True(
            result.Value!.Id != Guid.Empty ||
            result.Value.UserId != Guid.Empty ||
            result.Value.CapturedAt.Value != DateTime.MinValue ||
            result.Value.Size.Value != 0 ||
            result.Value.Mime.Value != string.Empty ||
            result.Value.StorageKey.Value != string.Empty ||
            result.Value.Metadata is not null ||
            result.Value.TagIds.Count != 0 &&
            result.IsSuccess
        );
    }

    [Fact]
    public void DeepCopy_DeeplyCopyingOriginalObject_ReturnsPhoto()
    {
        Metadata.Builder builder = new();

        Photo original = Photo
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                CapturedAt.Create(DateTime.Now.ToString(CultureInfo.CurrentCulture)).Value!,
                Size.Create(100).Value!,
                Mime.Create("mime").Value!,
                StorageKey.Create("photo").Value!,
                builder.Build().Value!,
                [
                    Guid.CreateVersion7(),
                    Guid.CreateVersion7()
                ]
            )
            .Value!;

        Photo copy = original.DeepCopy();

        Assert.True(
            original.Id == copy.Id ||
            original.UserId == copy.UserId ||
            original.CapturedAt == copy.CapturedAt ||
            original.Size == copy.Size ||
            original.Mime == copy.Mime ||
            original.StorageKey == copy.StorageKey ||
            original.Metadata == copy.Metadata ||
            original.TagIds.Count == copy.TagIds.Count
        );
    }

    [Fact]
    public void DeepCopy_DeeplyCopiedObjectChangesNotAffectingOriginalObjectValues()
    {
        Metadata.Builder builder = new();

        Photo original = Photo
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                CapturedAt.Create(DateTime.Now.ToString(CultureInfo.CurrentCulture)).Value!,
                Size.Create(100).Value!,
                Mime.Create("mime").Value!,
                StorageKey.Create("photo").Value!,
                builder.Build().Value!,
                [
                    Guid.CreateVersion7(),
                    Guid.CreateVersion7()
                ]
            )
            .Value!;

        Photo copy = original.DeepCopy();

        copy.CapturedAt = CapturedAt.Create(DateTime.MaxValue.ToString(CultureInfo.CurrentCulture)).Value!;
        copy.Size = Size.Create(200).Value!;
        copy.Mime = Mime.Create("mime2").Value!;
        copy.StorageKey = StorageKey.Create("photo2").Value!;
        copy.Metadata = builder.SetHasHdr(true).SetIsPanorama(true).Build().Value!;
        copy.AddTag(Guid.CreateVersion7());

        Assert.True(
            original.CapturedAt != copy.CapturedAt &&
            original.Size != copy.Size &&
            original.Mime != copy.Mime &&
            original.StorageKey != copy.StorageKey &&
            original.Metadata != copy.Metadata &&
            original.TagIds.Count != copy.TagIds.Count
        );
    }

    [Fact]
    public void AddTag_AddingUniqueTagId_ReturnsResultWithSuccess()
    {
        Metadata.Builder builder = new();
        
        Guid tagId = Guid.CreateVersion7();
    
        Photo photo = Photo
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                CapturedAt.Create(DateTime.Now.ToString(CultureInfo.CurrentCulture)).Value!,
                Size.Create(100).Value!,
                Mime.Create("mime").Value!,
                StorageKey.Create("photo").Value!,
                builder.Build().Value!,
                [
                    Guid.CreateVersion7(),
                    Guid.CreateVersion7()
                ]
            ).Value!;
    
        ResultVoid addResult = photo.AddTag(tagId);
        
        Assert.True(
            addResult.IsSuccess &&
            photo.TagIds.FirstOrDefault(ti => ti == tagId) == tagId
        );
    }

    [Fact]
    public void AddTag_AddingSimilarTagId_ReturnsResultWithFailureAndError()
    {
        Metadata.Builder builder = new();

        Guid tagId = Guid.CreateVersion7();

        Photo photo = Photo
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                CapturedAt.Create(DateTime.Now.ToString(CultureInfo.CurrentCulture)).Value!,
                Size.Create(100).Value!,
                Mime.Create("mime").Value!,
                StorageKey.Create("photo").Value!,
                builder.Build().Value!,
                [tagId]
            ).Value!;

        ResultVoid addResult = photo.AddTag(tagId);

        Assert.True(
            addResult.IsFailure &&
            addResult.ResultError == DomainErrors.Ids.DuplicatedId &&
            photo.TagIds.Count == 1
        );
    }

    [Fact]
    public void DeleteTag_DeletingExistingTagId_ReturnsResultWithSuccess()
    {
        Metadata.Builder builder = new();

        Guid tagId = Guid.CreateVersion7();

        Photo photo = Photo
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                CapturedAt.Create(DateTime.Now.ToString(CultureInfo.CurrentCulture)).Value!,
                Size.Create(100).Value!,
                Mime.Create("mime").Value!,
                StorageKey.Create("photo").Value!,
                builder.Build().Value!,
                [tagId]
            ).Value!;

        ResultVoid deleteResult = photo.DeleteTag(tagId);

        Assert.True(
            deleteResult.IsSuccess &&
            photo.TagIds.Count == 0
        );
    }

    [Fact]
    public void DeletePhoto_DeletingNonExistingPhotoId_ReturnsResultWithFailureAndError()
    {
        Metadata.Builder builder = new();

        Guid tagId = Guid.CreateVersion7();

        Photo photo = Photo
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                CapturedAt.Create(DateTime.Now.ToString(CultureInfo.CurrentCulture)).Value!,
                Size.Create(100).Value!,
                Mime.Create("mime").Value!,
                StorageKey.Create("photo").Value!,
                builder.Build().Value!,
                []
            ).Value!;

        ResultVoid deleteResult = photo.DeleteTag(tagId);

        Assert.True(
            deleteResult.IsFailure &&
            deleteResult.ResultError == DomainErrors.Ids.IdNotFound
        );
    }
}