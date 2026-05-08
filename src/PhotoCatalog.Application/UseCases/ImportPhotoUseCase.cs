using System.IO;

using Microsoft.Extensions.Logging;

using PhotoCatalog.Application.DTOs;
using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

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
    private readonly ILogger<ImportPhotoUseCase> _logger;
    private readonly IFileMetadataExtractor _metadataExtractor;
    private readonly IPhotoRepository _photoRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    ///     Инициализирует новый экземпляр класса <see cref="ImportPhotoUseCase" />.
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
        _logger.LogInformation("Начало импорта фотографии. Путь: {SourcePath}", request.SourcePath);

        return _fileStorage.FileExists(request.SourcePath)
            .ToResult(ApplicationErrors.Files.FileNotFound)
            .OnSuccess(_ => _logger.LogInformation("Файл найден: {SourcePath}", request.SourcePath))
            .OnFailure(_ => _logger.LogWarning("Файл не найден: {SourcePath}", request.SourcePath))
            .Then(_ => _metadataExtractor.CalculateHash(request.SourcePath))
            .OnSuccess(hash => _logger.LogDebug("Хэш вычислен: {Hash}", hash))
            .Then(hash => _metadataExtractor.GetDimensions(request.SourcePath)
                .Transform(dimensions => (hash, dimensions)))
            .OnSuccess(tuple => _logger.LogDebug("Размеры получены: {Width}x{Height}", tuple.dimensions.Width, tuple.dimensions.Height))
            .Then(tuple => _fileStorage.StoreFile(request.SourcePath, Path.GetFileName(request.SourcePath))
                .Transform(filePath => (tuple.hash, tuple.dimensions, filePath)))
            .OnSuccess(tuple => _logger.LogDebug("Файл скопирован: {FilePath}", tuple.filePath))
            .Then(tuple => Photo.Create(tuple.filePath)
                .OnSuccess(photo => _logger.LogDebug("Сущность Photo создана: {FilePath}", tuple.filePath))
                .OnFailure(error => _fileStorage.DeleteFile(tuple.filePath))
                .Transform(photo => (tuple.hash, tuple.dimensions, photo)))
            .Then(tuple =>
            {
                tuple.photo.UpdateHash(tuple.hash);
                tuple.photo.SetDimensions(tuple.dimensions);
                return Result<Photo>.Success(tuple.photo);
            })
            .Then(photo => _unitOfWork.BeginTransaction()
                .ToResult()
                .Ensure(beginResult => beginResult.IsSuccess, ApplicationErrors.Transactions.StartTransactions)
                .Then(_ => _photoRepository.Add(photo))
                .Then(() => _unitOfWork.Commit())
                .ToResult()
                .Ensure(commitResult => commitResult.IsSuccess, ApplicationErrors.Transactions.CommitFailed)
                .Transform(_ => photo))
            .OnSuccess(photo =>
                _logger.LogInformation("Импорт фотографии успешно завершен. PhotoId: {PhotoId}", photo.Id))
            .OnFailure(error =>
                _logger.LogError("Ошибка импорта: {ErrorCode} - {ErrorMessage}", error.Code, error.Message))
            .Transform(photo => new PhotoResponse(
                photo.Id,
                photo.RealPath,
                photo.FileHash,
                photo.Dimensions.Width,
                photo.Dimensions.Height,
                photo.AddedAt,
                photo.TagIds));
    }
}