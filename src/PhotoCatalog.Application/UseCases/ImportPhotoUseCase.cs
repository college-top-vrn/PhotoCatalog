using System.IO;

using PhotoCatalog.Application.DTOs;
using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

using Serilog;

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
    private readonly ILogger _logger;
    private readonly IFileMetadataExtractor _metadataExtractor;
    private readonly IPhotoCommandRepository _photoRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    ///     Инициализирует новый экземпляр класса <see cref="ImportPhotoUseCase" />.
    /// </summary>
    /// <param name="fileStorage">Сервис для работы с файловой системой.</param>
    /// <param name="metadataExtractor">Сервис для извлечения метаданных файла.</param>
    /// <param name="photoCommandRepository">Репозиторий для работы с сущностями Photo.</param>
    /// <param name="unitOfWork">Контракт для управления транзакциями.</param>
    /// <param name="logger">Сервис для логирования.</param>
    public ImportPhotoUseCase(
        IFileStorage fileStorage,
        IFileMetadataExtractor metadataExtractor,
        IPhotoCommandRepository photoCommandRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _fileStorage = fileStorage;
        _metadataExtractor = metadataExtractor;
        _photoRepository = photoCommandRepository;
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
    ///         <item>
    ///             <description>Успех с <see cref="PhotoResponse" />.</description>
    ///         </item>
    ///         <item>
    ///             <description>Ошибка с детализацией причины.</description>
    ///         </item>
    ///     </list>
    /// </returns>
    public Result<PhotoResponse> Execute(ImportPhotoRequest request)
    {
        _logger.Information("Начало импорта фотографии. Путь: {SourcePath}", request.SourcePath);

        return _fileStorage.FileExists(request.SourcePath)
            .ToResult(ApplicationErrors.Files.FileNotFound)
            .OnSuccess(_ => _logger.Information("Файл найден: {SourcePath}", request.SourcePath))
            .OnFailure(_ => _logger.Warning("Файл не найден: {SourcePath}", request.SourcePath))
            .Then(_ => _metadataExtractor.CalculateHash(request.SourcePath))
            .OnSuccess(hash => _logger.Debug("Хэш вычислен: {Hash}", hash))
            .Then(hash => _metadataExtractor.GetDimensions(request.SourcePath)
                .Transform(dimensions => (hash, dimensions)))
            .OnSuccess(tuple => _logger.Debug("Размеры получены: {Width}x{Height}", tuple.dimensions.Width,
                tuple.dimensions.Height))
            .Then(tuple => _fileStorage.StoreFile(request.SourcePath, Path.GetFileName(request.SourcePath))
                .Transform(filePath => (tuple.hash, tuple.dimensions, filePath)))
            .OnSuccess(tuple => _logger.Debug("Файл скопирован: {FilePath}", tuple.filePath))
            .Then(tuple => Photo.Create(tuple.filePath)
                .OnSuccess(_ => _logger.Debug("Сущность Photo создана: {FilePath}", tuple.filePath))
                .OnFailure(_ => _fileStorage.DeleteFile(tuple.filePath))
                .Transform(photo => (tuple.hash, tuple.dimensions, photo)))
            .Then(tuple =>
            {
                tuple.photo.UpdateHash(tuple.hash);
                tuple.photo.SetDimensions(tuple.dimensions);
                return Result.Success(tuple.photo);
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
                _logger.Information("Импорт фотографии успешно завершен. PhotoId: {PhotoId}", photo.Id))
            .OnFailure(error =>
                _logger.Error("Ошибка импорта: {ErrorCode} - {ErrorMessage}", error.Code, error.Message))
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