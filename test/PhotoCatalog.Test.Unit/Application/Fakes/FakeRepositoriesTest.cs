using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Fakes;

using Xunit;

namespace PhotoCatalog.Test.Unit.Application.Fakes;

/// <summary>
///     Содержит модульные тесты для проверки работы FakeAlbumRepository.
/// </summary>
public static class FakeRepositoriesTest
{
    /// <summary>
    ///     Проверяет, что альбом успешно добавляется в репозиторий при передаче корректных значений.
    /// </summary>
    [Fact]
    public static void AlbumRepository_AddAlbum_WithRightValues()
    {
        FakeAlbumRepository fakeAlbumRepository = new();

        fakeAlbumRepository.Add(Album.Create("Test", 0).Value);

        string albumName = fakeAlbumRepository.GetById(1).Value.Name;

        Assert.Equal("Test", albumName);
    }

    /// <summary>
    ///     Проверяет, что при попытке добавить null возвращается ожидаемая ошибка.
    /// </summary>
    [Fact]
    public static void AlbumRepository_AddAlbum_AddingNull()
    {
        FakeAlbumRepository fakeAlbumRepository = new();

        Error result = fakeAlbumRepository.Add(null).Error;

        Error expectedResult = new("AlbumRepository.CantAddAlbum",
            "Не удалось добавить альбом");

        Assert.Equal(result.Code, expectedResult.Code);
        Assert.Equal(result.Message, expectedResult.Message);
    }

    /// <summary>
    ///     Проверяет, что альбом успешно обновляется при передаче корректных значений.
    /// </summary>
    [Fact]
    public static void AlbumRepository_UpdateAlbum_WithRightValues()
    {
        FakeAlbumRepository fakeAlbumRepository = new();

        fakeAlbumRepository.Add(Album.Create("Test", 1).Value);

        Result<Album> oldAlbum = fakeAlbumRepository.GetById(1);

        fakeAlbumRepository.Update(Album.Create("Test2", 1).Value);

        Result<Album> newAlbum = fakeAlbumRepository.GetById(1);

        Assert.NotEqual(oldAlbum.Value.Name, newAlbum.Value.Name);
    }

    /// <summary>
    ///     Проверяет, что при попытке обновить альбом с несуществующим идентификатором возвращается ошибка.
    /// </summary>
    [Fact]
    public static void AlbumRepository_UpdateAlbum_UpdatingWithNonexistentId()
    {
        FakeAlbumRepository fakeAlbumRepository = new();

        Error resultError = fakeAlbumRepository.Update(Album.Create("Test3", 40).Value).Error;

        Error expectedError = new("AlbumRepository.CantDeleteAlbum",
            "Не удалось удалить альбом");

        Assert.Equal(resultError.Code, expectedError.Code);
        Assert.Equal(resultError.Message, expectedError.Message);
    }

    /// <summary>
    ///     Проверяет, что при попытке обновить null возвращается ожидаемая ошибка.
    /// </summary>
    [Fact]
    public static void AlbumRepository_UpdateAlbum_UpdatingWithNull()
    {
        FakeAlbumRepository fakeAlbumRepository = new();

        Error resultError = fakeAlbumRepository.Update(null).Error;

        Error expectedError = new("AlbumRepository.AlbumIsNull",
            "Альбом является null");

        Assert.Equal(resultError.Code, expectedError.Code);
        Assert.Equal(resultError.Message, expectedError.Message);
    }

    /// <summary>
    ///     Проверяет, что существующий альбом успешно удаляется.
    /// </summary>
    [Fact]
    public static void AlbumRepository_DeleteAlbum_WithExistingId()
    {
        FakeAlbumRepository fakeAlbumRepository = new();

        Album? album = Album.Create("Test", 1).Value;

        fakeAlbumRepository.Add(album);

        Album? addedAlbum = fakeAlbumRepository.GetById(1).Value;

        Assert.Equal(addedAlbum.Name, album.Name);
        Assert.Equal(addedAlbum.Id, album.Id);

        fakeAlbumRepository.Delete(1);

        Result<Album> searchResult = fakeAlbumRepository.GetById(1);

        Error expectedError = new("AlbumRepository.AlbumNotFound",
            "Не удалось найти альбом по идентификатору");

        Assert.Equal(expectedError.Code, searchResult.Error.Code);
        Assert.Equal(expectedError.Message, searchResult.Error.Message);
    }

    /// <summary>
    ///     Проверяет, что при попытке удалить альбом с несуществующим идентификатором возвращается ошибка.
    /// </summary>
    [Fact]
    public static void AlbumRepository_DeleteAlbum_WithNonexistentId()
    {
        FakeAlbumRepository fakeAlbumRepository = new();

        Album? album = Album.Create("Test", 1).Value;

        fakeAlbumRepository.Add(album);

        Album? addedAlbum = fakeAlbumRepository.GetById(1).Value;

        Assert.Equal(addedAlbum.Name, album.Name);
        Assert.Equal(addedAlbum.Id, album.Id);

        ResultVoid deleteResult = fakeAlbumRepository.Delete(2);

        Error expectedError = new("AlbumRepository.CantDeleteAlbum",
            "Не удалось удалить альбом");

        Assert.Equal(expectedError.Code, deleteResult.Error.Code);
        Assert.Equal(expectedError.Message, deleteResult.Error.Message);
    }
}