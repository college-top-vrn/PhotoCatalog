using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;
using PhotoCatalog.Infrastructure.Fakes;
using Xunit;

namespace PhotoCatalog.Tests.Infrastructure.Fakes;

/// <summary>
///     Тесты для Fake-репозиториев альбомов (CQRS).
/// </summary>
public class FakeRepositoriesTests
{
    private readonly ConcurrentDictionary<int, Album> _sharedStorage;
    private readonly FakeAlbumCommandRepository _commandRepository;
    private readonly FakeAlbumQueryRepository _queryRepository;

    public FakeRepositoriesTests()
    {
        _sharedStorage = new ConcurrentDictionary<int, Album>();
        _commandRepository = new FakeAlbumCommandRepository(_sharedStorage);
        _queryRepository = new FakeAlbumQueryRepository(_sharedStorage);
    }

    /// <summary>
    ///     Тест: добавление null альбома возвращает ошибку NullAlbum.
    /// </summary>
    [Fact]
    public void AlbumCommandRepository_AddAlbum_AddingNull_ReturnsNullAlbumError()
    {
        // Act
        ResultVoid result = _commandRepository.Add(null!);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Album.NullAlbum.Code, result.Error.Code);
        Assert.Equal(DomainErrors.Album.NullAlbum.Message, result.Error.Message);
    }

    /// <summary>
    ///     Тест: добавление корректного альбома проходит успешно.
    /// </summary>
    [Fact]
    public void AlbumCommandRepository_AddAlbum_WithRightValues_ReturnsSuccess()
    {
        // Arrange
        Result<Album> createResult = Album.Create("Тестовый альбом", 0);
        Assert.True(createResult.IsSuccess);
        Album album = createResult.Value;

        // Act
        ResultVoid result = _commandRepository.Add(album);

        // Assert
        Assert.True(result.IsSuccess);
        
        // Проверяем через QueryRepository, что альбом добавился
        Result<Album> getResult = _queryRepository.GetById(1);
        Assert.True(getResult.IsSuccess);
        Assert.Equal("Тестовый альбом", getResult.Value.Name);
    }

    /// <summary>
    ///     Тест: удаление существующего альбома проходит успешно.
    /// </summary>
    [Fact]
    public void AlbumCommandRepository_DeleteAlbum_WithExistingId_ReturnsSuccess()
    {
        // Arrange
        Result<Album> createResult = Album.Create("Альбом для удаления", 0);
        Assert.True(createResult.IsSuccess);
        _commandRepository.Add(createResult.Value);
        
        Result<Album> getBeforeDelete = _queryRepository.GetById(1);
        Assert.True(getBeforeDelete.IsSuccess);

        // Act
        ResultVoid result = _commandRepository.Delete(1);

        // Assert
        Assert.True(result.IsSuccess);
        
        // Проверяем через QueryRepository, что альбом удалён
        Result<Album> getAfterDelete = _queryRepository.GetById(1);
        Assert.True(getAfterDelete.IsFailure);
        Assert.Equal(InfrastructureErrors.Database.NotFound.Code, getAfterDelete.Error.Code);
    }

    /// <summary>
    ///     Тест: удаление несуществующего альбома возвращает ошибку NotFound.
    /// </summary>
    [Fact]
    public void AlbumCommandRepository_DeleteAlbum_WithNonexistentId_ReturnsNotFoundError()
    {
        // Act
        ResultVoid result = _commandRepository.Delete(999);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(InfrastructureErrors.Database.NotFound.Code, result.Error.Code);
        Assert.Equal(InfrastructureErrors.Database.NotFound.Message, result.Error.Message);
    }

    /// <summary>
    ///     Тест: обновление null альбома возвращает ошибку NullAlbum.
    /// </summary>
    [Fact]
    public void AlbumCommandRepository_UpdateAlbum_UpdatingWithNull_ReturnsNullAlbumError()
    {
        // Act
        ResultVoid result = _commandRepository.Update(null!);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Album.NullAlbum.Code, result.Error.Code);
        Assert.Equal(DomainErrors.Album.NullAlbum.Message, result.Error.Message);
    }

    /// <summary>
    ///     Тест: обновление несуществующего альбома возвращает ошибку NotFound.
    /// </summary>
    [Fact]
    public void AlbumCommandRepository_UpdateAlbum_UpdatingWithNonexistentId_ReturnsNotFoundError()
    {
        // Arrange
        Result<Album> createResult = Album.Create("Несуществующий альбом", 999);
        Assert.True(createResult.IsSuccess);
        Album album = createResult.Value;

        // Act
        ResultVoid result = _commandRepository.Update(album);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(InfrastructureErrors.Database.NotFound.Code, result.Error.Code);
        Assert.Equal(InfrastructureErrors.Database.NotFound.Message, result.Error.Message);
    }

    /// <summary>
    ///     Тест: обновление существующего альбома проходит успешно.
    /// </summary>
    [Fact]
    public void AlbumCommandRepository_UpdateAlbum_WithRightValues_ReturnsSuccess()
    {
        // Arrange
        Result<Album> createResult = Album.Create("Старое имя", 0);
        Assert.True(createResult.IsSuccess);
        _commandRepository.Add(createResult.Value);
        
        Result<Album> getAfterAdd = _queryRepository.GetById(1);
        Assert.True(getAfterAdd.IsSuccess);
        
        ResultVoid renameResult = getAfterAdd.Value.Rename("Новое имя");
        Assert.True(renameResult.IsSuccess);

        // Act
        ResultVoid result = _commandRepository.Update(getAfterAdd.Value);

        // Assert
        Assert.True(result.IsSuccess);
        
        Result<Album> getAfterUpdate = _queryRepository.GetById(1);
        Assert.True(getAfterUpdate.IsSuccess);
        Assert.Equal("Новое имя", getAfterUpdate.Value.Name);
    }

    /// <summary>
    ///     Тест: получение альбома по ID через QueryRepository.
    /// </summary>
    [Fact]
    public void AlbumQueryRepository_GetById_ExistingAlbum_ReturnsAlbum()
    {
        // Arrange
        Result<Album> createResult = Album.Create("Альбом для поиска", 0);
        Assert.True(createResult.IsSuccess);
        _commandRepository.Add(createResult.Value);

        // Act
        Result<Album> result = _queryRepository.GetById(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Альбом для поиска", result.Value.Name);
    }

    /// <summary>
    ///     Тест: получение несуществующего альбома возвращает NotFound.
    /// </summary>
    [Fact]
    public void AlbumQueryRepository_GetById_NonexistentAlbum_ReturnsNotFoundError()
    {
        // Act
        Result<Album> result = _queryRepository.GetById(999);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(InfrastructureErrors.Database.NotFound.Code, result.Error.Code);
    }

    /// <summary>
    ///     Тест: получение альбомов по ID папки.
    /// </summary>
    [Fact]
    public void AlbumQueryRepository_GetByFolderId_ReturnsAlbumsInFolder()
    {
        // Arrange
        Result<Album> album1 = Album.Create("Альбом 1", 0);
        Result<Album> album2 = Album.Create("Альбом 2", 0);
        
        _commandRepository.Add(album1.Value);
        _commandRepository.Add(album2.Value);
        
        Album? album1FromRepo = _queryRepository.GetById(1).Value;
        Album? album2FromRepo = _queryRepository.GetById(2).Value;
        
        typeof(Album).GetProperty("FolderId")?.SetValue(album1FromRepo, 10);
        typeof(Album).GetProperty("FolderId")?.SetValue(album2FromRepo, 10);
        
        _commandRepository.Update(album1FromRepo);
        _commandRepository.Update(album2FromRepo);

        // Act
        Result<IReadOnlyCollection<Album>> result = _queryRepository.GetByFolderId(10);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, album => Assert.Equal(10, album.FolderId));
    }

    /// <summary>
    ///     Тест: получение пустой коллекции для папки без альбомов.
    /// </summary>
    [Fact]
    public void AlbumQueryRepository_GetByFolderId_EmptyFolder_ReturnsEmptyCollection()
    {
        // Act
        Result<IReadOnlyCollection<Album>> result = _queryRepository.GetByFolderId(999);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}