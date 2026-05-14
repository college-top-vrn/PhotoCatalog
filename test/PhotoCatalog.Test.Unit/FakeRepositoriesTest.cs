using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Fakes;

using Xunit;

namespace PhotoCatalog.Test.Unit;

public static class FakeRepositoriesTest
{
    private static readonly FakeAlbumRepository FakeAlbumRepository = new();

    [Fact]
    public static void AlbumRepository_AddAlbum_WithRightValues()
    {
        FakeAlbumRepository.Add(Album.Create("Test", 0).Value);

        var albumName = FakeAlbumRepository.GetById(1).Value.Name;

        Assert.Equal("Test", albumName);
    }

    [Fact]
    public static void AlbumRepository_AddAlbum_AddingNull()
    {
        var result = FakeAlbumRepository.Add(null).Error;

        var expectedResult = new Error("AlbumRepository.CantAddAlbum",
            "Не удалось добавить альбом");

        Assert.Equal(result.Code, expectedResult.Code);
        Assert.Equal(result.Message, expectedResult.Message);
    }

    [Fact]
    public static void AlbumRepository_UpdateAlbum_WithRightValues()
    {
        FakeAlbumRepository.Add(Album.Create("Test", 1).Value);

        var oldAlbum = FakeAlbumRepository.GetById(1);

        FakeAlbumRepository.Update(Album.Create("Test2", 1).Value);

        var newAlbum = FakeAlbumRepository.GetById(1);

        Assert.NotEqual(oldAlbum.Value.Name, newAlbum.Value.Name);
    }

    [Fact]
    public static void AlbumRepository_UpdateAlbum_UpdatingWithNonexistentId()
    {
        var resultError = FakeAlbumRepository.Update(Album.Create("Test3", 40).Value).Error;

        var expectedError = new Error("AlbumRepository.CantDeleteAlbum",
            "Не удалось удалить альбом");

        Assert.Equal(resultError.Code, expectedError.Code);
        Assert.Equal(resultError.Message, expectedError.Message);
    }

    [Fact]
    public static void AlbumRepository_UpdateAlbum_UpdatingWithNull()
    {
        var resultError = FakeAlbumRepository.Update(null).Error;

        var expectedError = new Error("AlbumRepository.CantAddNull",
            "Нельзя добавлять null");

        Assert.Equal(resultError.Code, expectedError.Code);
        Assert.Equal(resultError.Message, expectedError.Message);
    }

    [Fact]
    public static void AlbumRepository_DeleteAlbum_WithExistingId()
    {
        var album = Album.Create("Test", 1).Value;
        
        FakeAlbumRepository.Add(album);

        var addedAlbum = FakeAlbumRepository.GetById(1).Value;
        
        Assert.Equal(addedAlbum.Name, album.Name);
        Assert.Equal(addedAlbum.Id, album.Id);

        FakeAlbumRepository.Delete(1);

        var searchResult = FakeAlbumRepository.GetById(1);

        var expectedError = new Error("AlbumRepository.AlbumNotFound",
            "Не удалось найти альбом по идентификатору");
        
        Assert.Equal(expectedError.Code, searchResult.Error.Code);
        Assert.Equal(expectedError.Message, searchResult.Error.Message);
    }

    [Fact]
    public static void AlbumRepository_DeleteAlbum_WithNonexistentId()
    {
        var album = Album.Create("Test", 1).Value;
        
        FakeAlbumRepository.Add(album);

        var addedAlbum = FakeAlbumRepository.GetById(1).Value;
        
        Assert.Equal(addedAlbum.Name, album.Name);
        Assert.Equal(addedAlbum.Id, album.Id);

        var deleteResult = FakeAlbumRepository.Delete(2);

        var expectedError = new Error("AlbumRepository.CantDeleteAlbum",
            "Не удалось удалить альбом");
        
        Assert.Equal(expectedError.Code, deleteResult.Error.Code);
        Assert.Equal(expectedError.Message, deleteResult.Error.Message);
    }
}