using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Fakes;

using Xunit;

namespace PhotoCatalog.Test.Unit;

public static class FakeRepositoriesTest
{
    [Fact]
    public static void AlbumRepositoryAddAlbumWithRightValues()
    {
        FakeAlbumRepository fakeAlbumRepository = new();

        fakeAlbumRepository.Add(Album.Create("Test", 0).Value);

        //TODO: Проверить на null
        string albumName = fakeAlbumRepository.GetById(1).Value!.Name;

        Assert.Equal("Test", albumName);
    }

    [Fact]
    public static void AlbumRepositoryAddAlbumAddingNull()
    {
        FakeAlbumRepository fakeAlbumRepository = new();

        Error result = fakeAlbumRepository.Add(null).Error;

        Error expectedResult = new("AlbumRepository.CantAddAlbum",
            "Не удалось добавить альбом");

        Assert.Equal(result.Code, expectedResult.Code);
        Assert.Equal(result.Message, expectedResult.Message);
    }

    [Fact]
    public static void AlbumRepositoryUpdateAlbumWithRightValues()
    {
        FakeAlbumRepository fakeAlbumRepository = new();

        fakeAlbumRepository.Add(Album.Create("Test", 1).Value);

        Result<Album> oldAlbum = fakeAlbumRepository.GetById(1);

        fakeAlbumRepository.Update(Album.Create("Test2", 1).Value);

        Result<Album> newAlbum = fakeAlbumRepository.GetById(1);

        //TODO: Проверить на null
        Assert.NotEqual(oldAlbum.Value!.Name, newAlbum.Value!.Name);
    }

    [Fact]
    public static void AlbumRepositoryUpdateAlbumUpdatingWithNonexistentId()
    {
        FakeAlbumRepository fakeAlbumRepository = new();

        Error resultError = fakeAlbumRepository.Update(Album.Create("Test3", 40).Value).Error;

        Error expectedError = new("AlbumRepository.CantDeleteAlbum",
            "Не удалось удалить альбом");

        Assert.Equal(resultError.Code, expectedError.Code);
        Assert.Equal(resultError.Message, expectedError.Message);
    }

    [Fact]
    public static void AlbumRepositoryUpdateAlbumUpdatingWithNull()
    {
        FakeAlbumRepository fakeAlbumRepository = new();

        Error resultError = fakeAlbumRepository.Update(null).Error;

        Error expectedError = new("AlbumRepository.AlbumIsNull",
            "Альбом является null");

        Assert.Equal(resultError.Code, expectedError.Code);
        Assert.Equal(resultError.Message, expectedError.Message);
    }

    [Fact]
    public static void AlbumRepositoryDeleteAlbumWithExistingId()
    {
        FakeAlbumRepository fakeAlbumRepository = new();

        Album? album = Album.Create("Test", 1).Value;

        fakeAlbumRepository.Add(album);

        Album? addedAlbum = fakeAlbumRepository.GetById(1).Value;

        //TODO: Проверить на null
        Assert.Equal(addedAlbum!.Name, album!.Name);
        Assert.Equal(addedAlbum.Id, album.Id);

        fakeAlbumRepository.Delete(1);

        Result<Album> searchResult = fakeAlbumRepository.GetById(1);

        Error expectedError = new("AlbumRepository.AlbumNotFound",
            "Не удалось найти альбом по идентификатору");

        Assert.Equal(expectedError.Code, searchResult.Error.Code);
        Assert.Equal(expectedError.Message, searchResult.Error.Message);
    }

    [Fact]
    public static void AlbumRepositoryDeleteAlbumWithNonexistentId()
    {
        FakeAlbumRepository fakeAlbumRepository = new();

        Album? album = Album.Create("Test", 1).Value;

        fakeAlbumRepository.Add(album);

        Album? addedAlbum = fakeAlbumRepository.GetById(1).Value;

        //TODO: Проверить на null
        Assert.Equal(addedAlbum!.Name, album!.Name);
        Assert.Equal(addedAlbum.Id, album.Id);

        ResultVoid deleteResult = fakeAlbumRepository.Delete(2);

        Error expectedError = new("AlbumRepository.CantDeleteAlbum",
            "Не удалось удалить альбом");

        Assert.Equal(expectedError.Code, deleteResult.Error.Code);
        Assert.Equal(expectedError.Message, deleteResult.Error.Message);
    }
}