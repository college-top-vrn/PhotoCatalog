using System;
using System.Linq;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;

using Xunit;

namespace PhotoCatalog.Test.Unit.Domain.Entities;

public class AlbumTest
{
    [Fact]
    public void Create_CreatingAlbumWithCorrectValues_ReturnsSuccessWithAlbum()
    {
        Name expectedName = Name.Create("Test").Value!;

        Result<Album> result = Album.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            expectedName, TODO,
            [
                Guid.CreateVersion7(),
                Guid.CreateVersion7()
            ]
        );

        Album album = result.Value!;

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedName, album.Name);
        Assert.NotEmpty(album.PhotoIds);
    }

    [Fact]
    public void DeepCopy_DeeplyCopyingOriginalAlbum_ReturnsAlbumCopy()
    {
        Name name = Name.Create("Test").Value!;

        Album original = Album.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            name, TODO,
            [
                Guid.CreateVersion7(),
                Guid.CreateVersion7()
            ]
        ).Value!;

        Album copy = original.DeepCopy();

        Assert.Equal(original.Id, copy.Id);
        Assert.Equal(original.UserId, copy.UserId);
        Assert.Equal(original.Name, copy.Name);
        Assert.NotSame(original.PhotoIds, copy.PhotoIds);
    }

    [Fact]
    public void DeepCopy_ChangingAlbumCopyWithoutAffectingOriginalAlbum()
    {
        Name originalName = Name.Create("Original").Value!;

        Album original = Album.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            originalName, TODO,
            [
                Guid.CreateVersion7(),
                Guid.CreateVersion7()
            ]
        ).Value!;

        Name copyName = Name.Create("Copy").Value!;

        Album copy = original.DeepCopy();

        copy.Rename(copyName);
        copy.AddPhoto(Guid.CreateVersion7());

        Assert.NotEqual(original.Name, copy.Name);
        Assert.NotEqual(original.PhotoIds.Count, copy.PhotoIds.Count);
    }

    [Fact]
    public void Rename_RenamingAlbumWithCorrectValue_ReturnsResultWithSuccess()
    {
        Name oldName = Name.Create("Old").Value!;

        Album album = Album.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            oldName, TODO,
            [
                Guid.CreateVersion7(),
                Guid.CreateVersion7()
            ]
        ).Value!;

        Name newName = Name.Create("New").Value!;

        ResultVoid renameResult = album.Rename(newName);

        Assert.True(renameResult.IsSuccess);
        Assert.NotEqual(oldName, album.Name);
    }

    [Fact]
    public void AddPhoto_AddingUniquePhotoId_ReturnsSuccess()
    {
        Album album = Album.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Name.Create("Test").Value!, TODO,
            []
        ).Value!;

        Guid photoId = Guid.CreateVersion7();

        ResultVoid addResult = album.AddPhoto(photoId);

        Assert.True(addResult.IsSuccess);
        Assert.Equal(album.PhotoIds.FirstOrDefault(photoId), photoId);
    }

    [Fact]
    public void AddPhoto_AddingSimilarPhotoId_ReturnsFailureWithError()
    {
        Guid photoId = Guid.CreateVersion7();

        Album album = Album
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Name.Create("Test").Value!, TODO,
                [photoId]
            ).Value!;

        ResultVoid addResult = album.AddPhoto(photoId);

        Assert.True(addResult.IsFailure);
        Assert.Equal(addResult.ResultError, DomainErrors.Ids.DuplicatedId);
        Assert.Single(album.PhotoIds);
    }

    [Fact]
    public void DeletePhoto_DeletingExistingPhotoId_ReturnsSuccess()
    {
        Guid photoId = Guid.CreateVersion7();

        Album album = Album
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Name.Create("Test").Value!, TODO,
                [photoId]
            ).Value!;

        ResultVoid deleteResult = album.DeletePhoto(photoId);

        Assert.True(deleteResult.IsSuccess);
        Assert.Empty(album.PhotoIds);
    }

    [Fact]
    public void DeletePhoto_DeletingNonExistingPhotoId_ReturnsFailureWithError()
    {
        Guid photoId = Guid.CreateVersion7();

        Album album = Album
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Name.Create("Test").Value!, TODO,
                [Guid.CreateVersion7()]
            ).Value!;

        ResultVoid deleteResult = album.DeletePhoto(photoId);

        Assert.True(deleteResult.IsFailure);
        Assert.Equal(deleteResult.ResultError, DomainErrors.Ids.IdNotFound);
    }
}