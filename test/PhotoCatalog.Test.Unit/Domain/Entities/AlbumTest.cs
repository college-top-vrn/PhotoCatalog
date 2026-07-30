using System;
using System.Linq;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit.Domain.Entities;

public class AlbumTest
{
    [Fact]
    public void Create_CreatingAlbumWithCorrectValues_ReturnsResultWithAlbumAndSuccess()
    {
        Result<Album> result = Album
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                "Test",
                [
                    Guid.CreateVersion7(),
                    Guid.CreateVersion7()
                ]
            );

        Assert.True(
            result.Value!.Id != Guid.Empty ||
            result.Value.UserId != Guid.Empty ||
            result.Value.Name.Value.Length != 0 ||
            result.Value.PhotoIds.Count != 0 &&
            result.IsSuccess
        );
    }

    [Fact]
    public void Create_NotCreatingAlbumWithIncorrectName_ReturnsResultWithFailure()
    {
        Result<Album> result1 = Album.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "TestingTheMostAwesomeNameThatHaveEverExistedInThisWorld",
            [
                Guid.CreateVersion7(),
                Guid.CreateVersion7()
            ]
        );

        Assert.True(
            result1.Value is null &&
            result1.IsFailure
        );

        Result<Album> result2 = Album.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "",
            [
                Guid.CreateVersion7(),
                Guid.CreateVersion7()
            ]
        );

        Assert.True(
            result2.Value is null &&
            result2.IsFailure
        );
    }

    [Fact]
    public void DeepCopy_DeeplyCopyingOriginalObject_ReturnsAlbum()
    {
        Album original = Album
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                "Test",
                [
                    Guid.CreateVersion7(),
                    Guid.CreateVersion7()
                ]
            )
            .Value!;

        Album copy = original.DeepCopy();

        Assert.True(
            original.Id == copy.Id &&
            original.UserId == copy.UserId &&
            original.Name == copy.Name &&
            original.PhotoIds.Count == copy.PhotoIds.Count
        );
    }

    [Fact]
    public void DeepCopy_DeeplyCopiedObjectChangesNotAffectingOriginalObjectValues()
    {
        Album original = Album
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                "Original",
                [
                    Guid.CreateVersion7(),
                    Guid.CreateVersion7()
                ]
            )
            .Value!;

        Album copy = original.DeepCopy();

        copy.Rename("Copy");
        copy.AddPhoto(Guid.CreateVersion7());

        Assert.True(
            original.Name != copy.Name &&
            original.PhotoIds.Count != copy.PhotoIds.Count
        );
    }

    [Fact]
    public void Rename_RenamingAlbumWithCorrectValue_ReturnsResultWithSuccess()
    {
        const string oldName = "Test1";
        const string newName = "Test2";

        Result<Album> result = Album.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            oldName,
            [
                Guid.CreateVersion7(),
                Guid.CreateVersion7()
            ]
        );

        var renameResult = result.Value!.Rename(newName);

        Assert.True(
            oldName != result.Value.Name.Value &&
            renameResult.IsSuccess
        );
    }

    [Fact]
    public void Rename_NotRenamingAlbumWithIncorrectValue_ReturnsResultWithFailure()
    {
        const string oldName = "Test1";
        const string newName = "TestingTheMostAwesomeNameThatHaveEverExistedInThisWorld";

        Result<Album> result = Album.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            oldName,
            [
                Guid.CreateVersion7(),
                Guid.CreateVersion7()
            ]
        );

        var renameResult = result.Value!.Rename(newName);

        Assert.True(
            oldName == result.Value.Name.Value &&
            renameResult.IsFailure
        );
    }

    [Fact]
    public void AddPhoto_AddingUniquePhotoId_ReturnsResultWithSuccess()
    {
        Guid photoId = Guid.CreateVersion7();

        Result<Album> result = Album.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Test",
            []
        );

        ResultVoid addResult = result.Value!.AddPhoto(photoId);

        Assert.True(
            addResult.IsSuccess &&
            result.Value.PhotoIds.FirstOrDefault(photoId) == photoId
        );
    }

    [Fact]
    public void AddPhoto_AddingSimilarPhotoId_ReturnsResultWithFailureAndError()
    {
        Guid photoId = Guid.CreateVersion7();

        Result<Album> result = Album
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                "Test",
                [photoId]
            );

        ResultVoid addResult = result.Value!.AddPhoto(photoId);

        Assert.True(
            addResult.IsFailure &&
            addResult.ResultError == DomainErrors.Ids.DuplicatedId &&
            result.Value.PhotoIds.Count == 1
        );
    }

    [Fact]
    public void DeletePhoto_DeletingExistingPhotoId_ReturnsResultWithSuccess()
    {
        Guid photoId = Guid.CreateVersion7();

        Result<Album> result = Album.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Test",
            [photoId]
        );

        ResultVoid deleteResult = result.Value!.DeletePhoto(photoId);

        Assert.True(
            deleteResult.IsSuccess &&
            result.Value.PhotoIds.Count == 0
        );
    }

    [Fact]
    public void DeletePhoto_DeletingNonExistingPhotoId_ReturnsResultWithFailureAndError()
    {
        Guid photoId = Guid.CreateVersion7();

        Result<Album> result = Album
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                "Test",
                [Guid.CreateVersion7()]
            );

        ResultVoid deleteResult = result.Value!.DeletePhoto(photoId);

        Assert.True(
            deleteResult.IsFailure &&
            deleteResult.ResultError == DomainErrors.Ids.IdNotFound
        );
    }
}