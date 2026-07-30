// using System.Collections.Concurrent;
// using System.Collections.Generic;
//
// using PhotoCatalog.Domain.Entities;
// using PhotoCatalog.Domain.Primitives;
// using PhotoCatalog.Infrastructure.Errors;
// using PhotoCatalog.Infrastructure.Fakes;
//
// using Xunit;
//
// namespace PhotoCatalog.Test.Unit.Application.Fakes;
//
// // TODO: Исправить предупрждения
// /// <summary>
// ///     Тесты для Fake-репозиториев альбомов (CQRS).
// /// </summary>
// public class FakeRepositoriesTests
// {
//     private readonly FakeAlbumCommandRepository _commandRepository;
//     private readonly FakeAlbumQueryRepository _queryRepository;
//
//     public FakeRepositoriesTests()
//     {
//         ConcurrentDictionary<int, Album> sharedStorage = new();
//         _commandRepository = new FakeAlbumCommandRepository(sharedStorage);
//         _queryRepository = new FakeAlbumQueryRepository(sharedStorage);
//     }
//
//     /// <summary>
//     ///     Тест: добавление корректного альбома проходит успешно.
//     /// </summary>
//     [Fact]
//     public void AlbumCommandRepositoryAddAlbumWithRightValuesReturnsSuccess()
//     {
//         // Arrange
//         (bool isSuccess, Album? album, _) = Album.Create(0, TODO, TODO, TODO);
//         Assert.True(isSuccess);
//
//         // Act
//         ResultVoid result = _commandRepository.AddPhoto(album!);
//
//         // Assert
//         Assert.True(result.IsSuccess);
//
//         // Проверяем через QueryRepository, что альбом добавился
//         Result<Album> getResult = _queryRepository.GetById(1);
//         Assert.True(getResult.IsSuccess);
//         if (getResult.Value != null)
//         {
//             Assert.Equal("Тестовый альбом", getResult.Value.Name);
//         }
//
//         Assert.NotNull(album);
//     }
//
//     /// <summary>
//     ///     Тест: удаление существующего альбома проходит успешно.
//     /// </summary>
//     [Fact]
//     public void AlbumCommandRepositoryDeleteAlbumWithExistingIdReturnsSuccess()
//     {
//         // Arrange
//         Result<Album> createResult = Album.Create(0, TODO, TODO, TODO);
//         Assert.True(createResult.IsSuccess);
//         _commandRepository.AddPhoto(createResult.Value);
//
//         Result<Album> getBeforeDelete = _queryRepository.GetById(1);
//         Assert.True(getBeforeDelete.IsSuccess);
//
//         // Act
//         ResultVoid result = _commandRepository.Delete(1);
//
//         // Assert
//         Assert.True(result.IsSuccess);
//
//         // Проверяем через QueryRepository, что альбом удалён
//         Result<Album> getAfterDelete = _queryRepository.GetById(1);
//         Assert.True(getAfterDelete.IsFailure);
//         Assert.Equal(InfrastructureErrors.Database.NotFound.Code, getAfterDelete.ResultError.Code);
//     }
//
//     /// <summary>
//     ///     Тест: удаление несуществующего альбома возвращает ошибку NotFound.
//     /// </summary>
//     [Fact]
//     public void AlbumCommandRepositoryDeleteAlbumWithNonexistentIdReturnsNotFoundError()
//     {
//         // Act
//         ResultVoid result = _commandRepository.Delete(999);
//
//         // Assert
//         Assert.True(result.IsFailure);
//         Assert.Equal(InfrastructureErrors.Database.NotFound.Code, result.ResultError.Code);
//         Assert.Equal(InfrastructureErrors.Database.NotFound.Message, result.ResultError.Message);
//     }
//
//     /// <summary>
//     ///     Тест: обновление несуществующего альбома возвращает ошибку NotFound.
//     /// </summary>
//     [Fact]
//     public void AlbumCommandRepositoryUpdateAlbumUpdatingWithNonexistentIdReturnsNotFoundError()
//     {
//         // Arrange
//         Result<Album> createResult = Album.Create(999, TODO, TODO, TODO);
//         Assert.True(createResult.IsSuccess);
//         Album album = createResult.Value;
//
//         // Act
//         ResultVoid result = _commandRepository.Update(album);
//
//         // Assert
//         Assert.True(result.IsFailure);
//         Assert.Equal(InfrastructureErrors.Database.NotFound.Code, result.ResultError.Code);
//         Assert.Equal(InfrastructureErrors.Database.NotFound.Message, result.ResultError.Message);
//     }
//
//     /// <summary>
//     ///     Тест: обновление существующего альбома проходит успешно.
//     /// </summary>
//     [Fact]
//     public void AlbumCommandRepositoryUpdateAlbumWithRightValuesReturnsSuccess()
//     {
//         // Arrange
//         Result<Album> createResult = Album.Create(0, TODO, TODO, TODO);
//         Assert.True(createResult.IsSuccess);
//         _commandRepository.AddPhoto(createResult.Value);
//
//         Result<Album> getAfterAdd = _queryRepository.GetById(1);
//         Assert.True(getAfterAdd.IsSuccess);
//
//         ResultVoid renameResult = getAfterAdd.Value.Rename("Новое имя");
//         Assert.True(renameResult.IsSuccess);
//
//         // Act
//         ResultVoid result = _commandRepository.Update(getAfterAdd.Value);
//
//         // Assert
//         Assert.True(result.IsSuccess);
//
//         Result<Album> getAfterUpdate = _queryRepository.GetById(1);
//         Assert.True(getAfterUpdate.IsSuccess);
//         Assert.Equal("Новое имя", getAfterUpdate.Value.Name);
//     }
//
//     /// <summary>
//     ///     Тест: получение альбома по ID через QueryRepository.
//     /// </summary>
//     [Fact]
//     public void AlbumQueryRepositoryGetByIdExistingAlbumReturnsAlbum()
//     {
//         // Arrange
//         Result<Album> createResult = Album.Create(0, TODO, TODO, TODO);
//         Assert.True(createResult.IsSuccess);
//         _commandRepository.AddPhoto(createResult.Value);
//
//         // Act
//         Result<Album> result = _queryRepository.GetById(1);
//
//         // Assert
//         Assert.True(result.IsSuccess);
//         Assert.Equal("Альбом для поиска", result.Value.Name);
//     }
//
//     /// <summary>
//     ///     Тест: получение несуществующего альбома возвращает NotFound.
//     /// </summary>
//     [Fact]
//     public void AlbumQueryRepositoryGetByIdNonexistentAlbumReturnsNotFoundError()
//     {
//         // Act
//         Result<Album> result = _queryRepository.GetById(999);
//
//         // Assert
//         Assert.True(result.IsFailure);
//         Assert.Equal(InfrastructureErrors.Database.NotFound.Code, result.ResultError.Code);
//     }
//
//     /// <summary>
//     ///     Тест: получение альбомов по ID папки.
//     /// </summary>
//     [Fact]
//     public void AlbumQueryRepositoryGetByFolderIdReturnsAlbumsInFolder()
//     {
//         // Arrange
//         Result<Album> album1 = Album.Create(0, TODO, TODO, TODO);
//         Result<Album> album2 = Album.Create(0, TODO, TODO, TODO);
//
//         _commandRepository.AddPhoto(album1.Value);
//         _commandRepository.AddPhoto(album2.Value);
//
//         Album? album1FromRepo = _queryRepository.GetById(1).Value;
//         Album? album2FromRepo = _queryRepository.GetById(2).Value;
//
//         typeof(Album).GetProperty("FolderId")?.SetValue(album1FromRepo, 10);
//         typeof(Album).GetProperty("FolderId")?.SetValue(album2FromRepo, 10);
//
//         _commandRepository.Update(album1FromRepo);
//         _commandRepository.Update(album2FromRepo);
//
//         // Act
//         Result<IReadOnlyCollection<Album>> result = _queryRepository.GetByFolderId(10);
//
//         // Assert
//         Assert.True(result.IsSuccess);
//         Assert.Equal(2, result.Value.Count);
//         Assert.All(result.Value, album => Assert.Equal(10, album.FolderId));
//     }
//
//     /// <summary>
//     ///     Тест: получение пустой коллекции для папки без альбомов.
//     /// </summary>
//     [Fact]
//     public void AlbumQueryRepositoryGetByFolderIdEmptyFolderReturnsEmptyCollection()
//     {
//         // Act
//         Result<IReadOnlyCollection<Album>> result = _queryRepository.GetByFolderId(999);
//
//         // Assert
//         Assert.True(result.IsSuccess);
//         Assert.Empty(result.Value);
//     }
// }