using System;
using System.IO;

using Microsoft.Extensions.Logging;

using PhotoCatalog.Application.DTOs;
using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;

namespace PhotoCatalog.Application.UseCases;

/// <summary>
///     Содержит сценарий оркестрации добавления нового фото в каталог.
/// </summary>
/// <remarks>
///     UseCase включает работу с файловой системой (копирование, хэш) и базой данных.
///     Обеспечивает атомарность операции через UnitOfWork и компенсационные механизмы.
/// </remarks>
public class ImportPhotoUseCase
{
    private readonly IFileStorage _fileStorage;
    private readonly IFileMetadataExtractor _metadataExtractor;
    private readonly IPhotoRepository _photoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImportPhotoUseCase> _logger;

    /// <summary>
    ///     Инициализирует новый экземпляр класса <see cref="ImportPhotoUseCase"/>.
    /// </summary>
    /// <param name="fileStorage">Сервис для работы с файловой системой.</param>
    /// <param name="metadataExtractor">Сервис для извлечения метаданных файла.</param>
    /// <param name="photoRepository">Репозиторий для работы с сущностями Photo.</param>
    /// <param name="unitOfWork">Контракт для управления транзакциями.</param>
    /// <param name="logger">Сервис для логирования.</param>
    public ImportPhotoUseCase(
        IFileStorage fileStorage,
        IFileMetadataExtractor metadataExtractor,
        IPhotoRepository photoRepository,
        IUnitOfWork unitOfWork,
        ILogger<ImportPhotoUseCase> logger)
    {
        _fileStorage = fileStorage;
        _metadataExtractor = metadataExtractor;
        _photoRepository = photoRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    ///     Выполняет импорт фотографии в каталог.
    /// </summary>
    /// <param name="request">Запрос на импорт фотографии.</param>
    /// <returns>
    ///     Результат операции:
    ///     <list type="bullet">
    ///         <item><description>Успех с <see cref="PhotoResponse"/>.</description></item>
    ///         <item><description>Ошибка с детализацией причины.</description></item>
    ///     </list>
    /// </returns>
    public Result<PhotoResponse> Execute(ImportPhotoRequest request)
    {
        var fileExistsResult = _fileStorage.FileExists(request.SourcePath);
        if (fileExistsResult.IsFailure)
        {
            _logger.LogWarning("Файл не найден: {SourcePath}", request.SourcePath);
            return Result<PhotoResponse>.Failure(ApplicationErrors.Files.FileNotFound);
        }

        var hashResult = _metadataExtractor.CalculateHash(request.SourcePath);
        if (hashResult.IsFailure)
        {
            _logger.LogError("Ошибка вычисления хэша: {Error}", hashResult.Error.Message);
            return Result<PhotoResponse>.Failure(hashResult.Error);
        }

        var dimensionsResult = _metadataExtractor.GetDimensions(request.SourcePath);
        if (dimensionsResult.IsFailure)
        {
            _logger.LogError("Ошибка получения размеров: {Error}", dimensionsResult.Error.Message);
            return Result<PhotoResponse>.Failure(dimensionsResult.Error);
        }

        var storeResult = _fileStorage.StoreFile(request.SourcePath, Path.GetFileName(request.SourcePath));
        if (storeResult.IsFailure)
        {
            _logger.LogError("Ошибка копирования файла: {Error}", storeResult.Error.Message);
            return Result<PhotoResponse>.Failure(storeResult.Error);
        }

        var newFilePath = storeResult.Value;

        var photoResult = Photo.Create(newFilePath);
        if (photoResult.IsFailure)
        {
            _logger.LogError("Ошибка создания сущности Photo: {Error}", photoResult.Error.Message);
            _fileStorage.DeleteFile(newFilePath);
            return Result<PhotoResponse>.Failure(photoResult.Error);
        }
        

        photoResult.Value.UpdateHash(hashResult.Value);
        photoResult.Value.SetDimensions(dimensionsResult.Value);


        var beginResult = _unitOfWork.BeginTransaction();
        if (beginResult.IsFailure)
        {
            _logger.LogError("Ошибка начала транзакции: {Error}", beginResult.Error.Message);
            _fileStorage.DeleteFile(newFilePath);
            return Result<PhotoResponse>.Failure(beginResult.Error);
        }

        _photoRepository.Add(photoResult.Value);

        var commitResult = _unitOfWork.Commit();
        if (commitResult.IsFailure)
        {
            _logger.LogError("Ошибка коммита транзакции: {Error}", commitResult.Error.Message);
            _fileStorage.DeleteFile(newFilePath);
            return Result<PhotoResponse>.Failure(commitResult.Error);
        }

        var response = new PhotoResponse(
            Id: photoResult.Value.Id,
            RealPath: photoResult.Value.RealPath,
            FileHash: photoResult.Value.FileHash,
            Width: photoResult.Value.Dimensions.Width,
            Height: photoResult.Value.Dimensions.Height,
            AddedAt: photoResult.Value.AddedAt,
            TagIds: photoResult.Value.TagIds);

        return Result<PhotoResponse>.Success(response);
    }
    
}