using System;

using Microsoft.Extensions.Logging;

using PhotoCatalog.Application.Errors;
using PhotoCatalog.Application.Fakes;
using PhotoCatalog.Application.UseCases;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.ValueObjects;
using PhotoCatalog.Infrastructure.Fakes;

using Serilog.Core;

using Xunit;

namespace PhotoCatalog.Test.Unit.Application.UseCases;

/// <summary>
///     Содержит модульные тесты для проверки работы DeletePhotoUseCase.
/// </summary>
public class DeletePhotoUseCaseTests
{
    private readonly IPhotoQueryRepository _photoQueryRepository;
    private readonly IPhotoCommandRepository _photoCommandRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Serilog.ILogger _logger;


    public DeletePhotoUseCaseTests(IPhotoQueryRepository photoQueryRepository,
        IPhotoCommandRepository photoCommandRepository, IFileStorage fileStorage, IUnitOfWork unitOfWork,
        Serilog.ILogger logger)
    {
        _photoQueryRepository = photoQueryRepository;
        _photoCommandRepository = photoCommandRepository;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _logger = Logger.None;
    }

    /// <summary>
    ///    Успешное удаление фотографии и физического файла.
    /// </summary>
    [Fact]
    public void Execute_WhenPhotoExistsAndCommitSuccess_ShouldDeletePhotoAndFile()
    {
        const int photoId = 1;
        const string realPath = "test/PhotoCatalog.Test.Integration/MagicScalerThumbnailServiceTests/landscape_photo.jpeg";

        var photo = Photo.Create(realPath).Value;
        typeof(Photo).GetProperty("Id")?.SetValue(photo, photoId);
        photo.SetDimensions(Dimensions.Create(1920, 1080).Value);

        _photoQueryRepository.GetById(photoId);
        _unitOfWork.Commit();
        
        
    }
}