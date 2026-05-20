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
        FakeAlbumQueryRepository fakeAlbumQueryRepository = new();
        FakeAlbumCommandRepository fakeAlbumCommandRepository = new();


        fakeAlbumCommandRepository.Add(Album.Create("Test", 0).Value);

        string albumName = fakeAlbumQueryRepository.GetById(1).Value.Name;

        Assert.Equal("Test", albumName);
    }

    /// <summary>
    ///     Проверяет, что при попытке добавить null возвращается ожидаемая ошибка.
    /// </summary>
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

    /// <summary>
    ///     Проверяет, что альбом успешно обновляется при передаче корректных значений.
    /// </summary>
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

    /// <summary>
    ///     Проверяет, что при попытке обновить альбом с несуществующим идентификатором возвращается ошибка.
    /// </summary>
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

    /// <summary>
    ///     Проверяет, что при попытке обновить null возвращается ожидаемая ошибка.
    /// </summary>
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

    /// <summary>
    ///     Проверяет, что существующий альбом успешно удаляется.
    /// </summary>
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

    /// <summary>
    ///     Проверяет, что при попытке удалить альбом с несуществующим идентификатором возвращается ошибка.
    /// </summary>
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