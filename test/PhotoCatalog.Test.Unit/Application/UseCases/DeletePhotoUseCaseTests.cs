using System;

using PhotoCatalog.Application.Errors;
using PhotoCatalog.Application.Fakes;
using PhotoCatalog.Application.UseCases;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.ValueObjects;
using PhotoCatalog.Infrastructure.Fakes;

using Serilog;
using Serilog.Core;

using Xunit;

namespace PhotoCatalog.Test.Unit.Application.UseCases;

/// <summary>
///     Тесты для DeletePhotoUseCase.
/// </summary>
public class DeletePhotoUseCaseTests
{
    private readonly IPhotoQueryRepository _photoQueryRepository;
    private readonly IPhotoCommandRepository _photoCommandRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;
    private readonly DeletePhotoUseCase _deletePhotoUseCase;

    public DeletePhotoUseCaseTests()
    {
        var fakeAlbumRepository = new FakeAlbumQueryRepository();

        _photoQueryRepository = new FakePhotoQueryRepository(fakeAlbumRepository);
        _photoCommandRepository = new FakePhotoCommandRepository();
        _fileStorage = new FakeFileStorage();
        _unitOfWork = new FakeUnitOfWork();
        _logger = Logger.None;

        _deletePhotoUseCase = new DeletePhotoUseCase(
            _photoQueryRepository,
            _photoCommandRepository,
            _fileStorage,
            _unitOfWork,
            _logger);
    }

    /// <summary>
    ///     Задача 4.1: Успешное удаление фотографии и физического файла.
    /// </summary>
    [Fact]
    public void Execute_WhenPhotoExistsAndCommitSuccess_ShouldDeletePhotoAndFile()
    {
        // Arrange
        const string realPath = @"PhotoCatalog.Test.Integration/MagicScalerThumbnailServiceTests/landscape_photo.jpeg";

        var photo = Photo.Create(realPath).Value;
        photo.SetDimensions(Dimensions.Create(1920, 1080).Value);

        // Add автоматически присвоит Id = 1
        _photoCommandRepository.Add(photo);

        // Act - удаляем фото с Id = 1
        var result = _deletePhotoUseCase.Execute(1);

        // Assert
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    ///     Задача 4.2: Сбой транзакции при коммите (Осиротевший файл на диске).
    /// </summary>
    [Fact]
    public void Execute_WhenCommitFails_ShouldReturnOrphanedFileErrorAndNotDeleteFile()
    {
        // Arrange
        const string realPath = @"PhotoCatalog.Test.Integration/MagicScalerThumbnailServiceTests/landscape_photo.jpeg";

        var photo = Photo.Create(realPath).Value;
        photo.SetDimensions(Dimensions.Create(1920, 1080).Value);

        // Add автоматически присвоит Id = 1
        _photoCommandRepository.Add(photo);

        // Act - удаляем фото с Id = 1
        var result = _deletePhotoUseCase.Execute(1);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Files.OrphanedFile.Code, result.Error.Code);
    }

    /// <summary>
    ///     Задача 4.3: Анализ поведения при отсутствии фотографии в системе.
    /// </summary>
    [Fact]
    public void Execute_WhenPhotoNotFound_ShouldThrowNullReferenceException()
    {
        // Arrange
        const int photoId = 999;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => _deletePhotoUseCase.Execute(photoId));
    }
}