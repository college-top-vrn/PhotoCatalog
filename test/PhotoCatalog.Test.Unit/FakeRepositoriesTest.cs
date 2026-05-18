using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Fakes;

using Xunit;

namespace PhotoCatalog.Test.Unit;

public static class FakeRepositoriesTest
{
    [Fact]
    public static void AlbumRepository_AddAlbum_WithRightValues()
    {
        FakeAlbumQueryRepository fakeAlbumQueryRepository = new();
        FakeAlbumCommandRepository fakeAlbumCommandRepository = new();


        fakeAlbumCommandRepository.Add(Album.Create("Test", 0).Value);

        string albumName = fakeAlbumQueryRepository.GetById(1).Value.Name;

        Assert.Equal("Test", albumName);
    }

    [Fact]
    public static void AlbumRepository_AddAlbum_AddingNull()
    {
        FakeAlbumCommandRepository fakeAlbumCommandRepository = new();

        Error result = fakeAlbumCommandRepository.Add(null).Error;

        Error expectedResult = new("AlbumRepository.CantAddAlbum",
            "Не удалось добавить альбом");

        Assert.Equal(result.Code, expectedResult.Code);
        Assert.Equal(result.Message, expectedResult.Message);
    }

    [Fact]
    public static void AlbumRepository_UpdateAlbum_WithRightValues()
    {
        FakeAlbumCommandRepository fakeAlbumCommandRepository = new();
        FakeAlbumQueryRepository fakeAlbumQueryRepository = new();

        fakeAlbumCommandRepository.Add(Album.Create("Test", 1).Value);

        Result<Album> oldAlbum = fakeAlbumQueryRepository.GetById(1);

        fakeAlbumCommandRepository.Update(Album.Create("Test2", 1).Value);

        Result<Album> newAlbum = fakeAlbumQueryRepository.GetById(1);

        Assert.NotEqual(oldAlbum.Value.Name, newAlbum.Value.Name);
    }

    [Fact]
    public static void AlbumRepository_UpdateAlbum_UpdatingWithNonexistentId()
    {
        FakeAlbumCommandRepository fakeAlbumCommandRepository = new();

        Error resultError = fakeAlbumCommandRepository.Update(Album.Create("Test3", 40).Value).Error;

        Error expectedError = new("AlbumRepository.CantDeleteAlbum",
            "Не удалось удалить альбом");

        Assert.Equal(resultError.Code, expectedError.Code);
        Assert.Equal(resultError.Message, expectedError.Message);
    }

    [Fact]
    public static void AlbumRepository_UpdateAlbum_UpdatingWithNull()
    {
        FakeAlbumCommandRepository fakeAlbumCommandRepository = new();

        Error resultError = fakeAlbumCommandRepository.Update(null).Error;

        Error expectedError = new("AlbumRepository.AlbumIsNull",
            "Альбом является null");

        Assert.Equal(resultError.Code, expectedError.Code);
        Assert.Equal(resultError.Message, expectedError.Message);
    }

    [Fact]
    public static void AlbumRepository_DeleteAlbum_WithExistingId()
    {
        FakeAlbumCommandRepository fakeAlbumCommandRepository = new();
        FakeAlbumQueryRepository fakeAlbumQueryRepository = new();

        Album? album = Album.Create("Test", 1).Value;

        fakeAlbumCommandRepository.Add(album);

        Album? addedAlbum = fakeAlbumQueryRepository.GetById(1).Value;

        Assert.Equal(addedAlbum.Name, album.Name);
        Assert.Equal(addedAlbum.Id, album.Id);

        fakeAlbumCommandRepository.Delete(1);

        Result<Album> searchResult = fakeAlbumQueryRepository.GetById(1);

        Error expectedError = new("AlbumRepository.AlbumNotFound",
            "Не удалось найти альбом по идентификатору");

        Assert.Equal(expectedError.Code, searchResult.Error.Code);
        Assert.Equal(expectedError.Message, searchResult.Error.Message);
    }

    [Fact]
    public static void AlbumRepository_DeleteAlbum_WithNonexistentId()
    {
        FakeAlbumCommandRepository fakeAlbumCommandRepository = new();
        FakeAlbumQueryRepository fakeAlbumQueryRepository = new();

        Album? album = Album.Create("Test", 1).Value;

        fakeAlbumCommandRepository.Add(album);

        Album? addedAlbum = fakeAlbumQueryRepository.GetById(1).Value;

        Assert.Equal(addedAlbum.Name, album.Name);
        Assert.Equal(addedAlbum.Id, album.Id);

        ResultVoid deleteResult = fakeAlbumCommandRepository.Delete(2);

        Error expectedError = new("AlbumRepository.CantDeleteAlbum",
            "Не удалось удалить альбом");

        Assert.Equal(expectedError.Code, deleteResult.Error.Code);
        Assert.Equal(expectedError.Message, deleteResult.Error.Message);
    }
}