using NSubstitute;

using PhotoCatalog.Application.UseCases;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

using Serilog;
using Serilog.Core;

using Xunit;

namespace PhotoCatalog.Test.Unit.Application.UseCases;

/// <summary>
/// Модульные тесты для AddTagToPhotoUseCase.Execute.
/// Тестирование в изоляции с использованием моков всех внешних зависимостей.
/// Библиотека моков: NSubstitute.
/// </summary>
public class AddTagToPhotoUseCaseTests
{
    private readonly ITagQueryRepository _tagQueryRepositoryMock;
    private readonly IPhotoQueryRepository _photoQueryRepositoryMock;
    private readonly IPhotoCommandRepository _photoCommandRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly AddTagToPhotoUseCase _useCase;

    public AddTagToPhotoUseCaseTests()
    {
        _tagQueryRepositoryMock = Substitute.For<ITagQueryRepository>();
        _photoQueryRepositoryMock = Substitute.For<IPhotoQueryRepository>();
        _photoCommandRepositoryMock = Substitute.For<IPhotoCommandRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        var logger = Logger.None;

        _useCase = new AddTagToPhotoUseCase(
            _tagQueryRepositoryMock,
            _photoQueryRepositoryMock,
            _photoCommandRepositoryMock,
            _unitOfWorkMock,
            logger);
    }

    #region Задача 3.1: Успешное добавление тега к фотографии

    [Fact]
    public void Execute_ValidPhotoAndTag_ShouldReturnSuccessAndCommitTransaction()
    {
        // Arrange
        const int photoId = 1;
        const int tagId = 100;

        // Создаём объекты через рефлексию
        var expectedPhoto = CreatePhotoWithId(photoId);
        var expectedTag = CreateTagWithId(tagId);

        // Настройка моков
        _tagQueryRepositoryMock.GetById(tagId).Returns(Result.Success<Tag>(expectedTag));
        _photoQueryRepositoryMock.GetById(photoId).Returns(Result.Success<Photo>(expectedPhoto));
        _unitOfWorkMock.BeginTransaction().Returns(ResultVoid.Success());
        _photoCommandRepositoryMock.Update(Arg.Any<Photo>()).Returns(ResultVoid.Success());
        _unitOfWorkMock.Commit().Returns(ResultVoid.Success());

        // Act
        var result = _useCase.Execute(photoId, tagId);

        // Assert
        Assert.True(result.IsSuccess);

        _photoCommandRepositoryMock.Received(1).Update(Arg.Any<Photo>());
        _unitOfWorkMock.Received(1).BeginTransaction();
        _unitOfWorkMock.Received(1).Commit();
        _unitOfWorkMock.Received(0).Rollback();
    }

    #endregion

    #region Задача 3.2: Ошибка «Тег не найден»

    [Fact]
    public void Execute_TagNotFound_ShouldReturnTagNotFoundErrorAndNotCallFurtherOperations()
    {
        // Arrange
        const int photoId = 1;
        const int tagId = 100;

        var expectedError = new ResultError("Tag.NotFound", "Тег не найден");

        _tagQueryRepositoryMock.GetById(tagId).Returns(Result.Failure<Tag>(expectedError));

        // Act
        var result = _useCase.Execute(photoId, tagId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Code, result.ResultError.Code);

        _photoQueryRepositoryMock.Received(0).GetById(Arg.Any<int>());
        _photoCommandRepositoryMock.Received(0).Update(Arg.Any<Photo>());
        _unitOfWorkMock.Received(0).BeginTransaction();
        _unitOfWorkMock.Received(0).Commit();
    }

    #endregion

    #region Задача 3.3: Ошибка «Фотография не найдена»

    [Fact]
    public void Execute_PhotoNotFound_ShouldReturnPhotoNotFoundErrorAndNotCallFurtherOperations()
    {
        // Arrange
        const int photoId = 1;
        const int tagId = 100;

        var expectedTag = CreateTagWithId(tagId);
        var expectedError = DomainErrors.Photo.NotFound;

        _tagQueryRepositoryMock.GetById(tagId).Returns(Result.Success<Tag>(expectedTag));
        _photoQueryRepositoryMock.GetById(photoId).Returns(Result.Failure<Photo>(expectedError));

        // Act
        var result = _useCase.Execute(photoId, tagId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Code, result.ResultError.Code);

        _unitOfWorkMock.Received(0).BeginTransaction();
        _photoCommandRepositoryMock.Received(0).Update(Arg.Any<Photo>());
    }

    #endregion

    #region Задача 3.4: Попытка добавления дубликата тега

    [Fact]
    public void Execute_DuplicateTag_ShouldReturnDuplicateTagErrorAndNotCallTransactionOrUpdate()
    {
        // Arrange
        const int photoId = 1;
        const int tagId = 100;

        var photo = CreatePhotoWithId(photoId);
        var tag = CreateTagWithId(tagId);

        // Добавляем тег первый раз через доменный метод
        var addResult = photo.AddTag(tagId);
        Assert.True(addResult.IsSuccess);

        _tagQueryRepositoryMock.GetById(tagId).Returns(Result.Success<Tag>(tag));
        _photoQueryRepositoryMock.GetById(photoId).Returns(Result.Success<Photo>(photo));

        // Act - повторная попытка добавить тот же тег
        var result = _useCase.Execute(photoId, tagId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Photo.DuplicateTag.Code, result.ResultError.Code);

        _unitOfWorkMock.Received(0).BeginTransaction();
        _photoCommandRepositoryMock.Received(0).Update(Arg.Any<Photo>());
    }

    #endregion

    #region Вспомогательные методы

    private static Photo CreatePhotoWithId(int id)
    {
        var constructor = typeof(Photo).GetConstructor(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            System.Type.EmptyTypes,
            null);

        var photo = (Photo)constructor?.Invoke(null)!;
        SetIdViaReflection(photo, id);
        return photo;
    }

    private static Tag CreateTagWithId(int id)
    {
        var constructor = typeof(Tag).GetConstructor(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            System.Type.EmptyTypes,
            null);

        var tag = (Tag)constructor?.Invoke(null)!;
        SetIdViaReflection(tag, id);
        return tag;
    }

    private static void SetIdViaReflection(object entity, int id)
    {
        var field = entity.GetType().GetField("<Ids>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(entity, id);
    }

    #endregion
}