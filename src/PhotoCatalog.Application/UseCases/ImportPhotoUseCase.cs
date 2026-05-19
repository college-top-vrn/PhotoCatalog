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
///     Дополнительно генерирует миниатюру изображения (провал не отменяет операцию).
/// </summary>
public class ImportPhotoUseCase
{
    private readonly IFileStorage _fileStorage;
    private readonly IFileMetadataExtractor _metadataExtractor;
    private readonly IPhotoCommandRepository _photoRepository;
    private readonly IThumbnailService _thumbnailService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    ///     Инициализирует новый экземпляр класса <see cref="ImportPhotoUseCase" />.
    /// </summary>
    /// <param name="fileStorage">Сервис для работы с файловой системой.</param>
    /// <param name="metadataExtractor">Сервис для извлечения метаданных файла.</param>
    /// <param name="photoCommandRepository">Репозиторий для работы с сущностями Photo.</param>
    /// <param name="unitOfWork">Контракт для управления транзакциями.</param>
    /// <param name="thumbnailService">Сервис для генерации миниатюр изображений.</param>
    public ImportPhotoUseCase(
        IFileStorage fileStorage,
        IFileMetadataExtractor metadataExtractor,
        IPhotoCommandRepository photoCommandRepository,
        IUnitOfWork unitOfWork,
        IThumbnailService thumbnailService)
    {
        _fileStorage = fileStorage;
        _metadataExtractor = metadataExtractor;
        _photoRepository = photoCommandRepository;
        _unitOfWork = unitOfWork;
        _thumbnailService = thumbnailService;
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
        Log.Information("Начало импорта фотографии. Путь: {SourcePath}", request.SourcePath);

        return _fileStorage.FileExists(request.SourcePath)
            .ToResult(ApplicationErrors.Files.FileNotFound)
            .OnSuccess(_ => Log.Information("Файл найден: {SourcePath}", request.SourcePath))
            .OnFailure(_ => Log.Warning("Файл не найден: {SourcePath}", request.SourcePath))
            .Then(_ => _metadataExtractor.CalculateHash(request.SourcePath))
            .OnSuccess(hash => Log.Debug("Хэш вычислен: {Hash}", hash))
            .Then(hash => _metadataExtractor.GetDimensions(request.SourcePath)
                .Transform(dimensions => (hash, dimensions)))
            .OnSuccess(tuple => Log.Debug("Размеры получены: {Width}x{Height}", tuple.dimensions.Width, tuple.dimensions.Height))
            .Then(tuple => _fileStorage.StoreFile(request.SourcePath, Path.GetFileName(request.SourcePath))
                .Transform(filePath => (tuple.hash, tuple.dimensions, filePath)))
            .OnSuccess(tuple => Log.Debug("Файл скопирован: {FilePath}", tuple.filePath))
            .Then(tuple => Photo.Create(tuple.filePath)
                .OnSuccess(photo => Log.Debug("Сущность Photo создана: {FilePath}", tuple.filePath))
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
            {
                Log.Information("Импорт фотографии успешно завершен. PhotoId: {PhotoId}", photo.Id);

                // Генерация миниатюры после успешной транзакции
                GenerateThumbnail(photo.RealPath);
            })
            .OnFailure(error =>
                Log.Error("Ошибка импорта: {ErrorCode} - {ErrorMessage}", error.Code, error.Message))
            .Transform(photo => new PhotoResponse(
                photo.Id,
                photo.RealPath,
                photo.FileHash,
                photo.Dimensions.Width,
                photo.Dimensions.Height,
                photo.AddedAt,
                photo.TagIds));
    }

    /// <summary>
    ///     Генерирует миниатюру для успешно импортированной фотографии.
    /// </summary>
    /// <param name="originalFilePath">Путь к оригинальному файлу фотографии.</param>
    /// <remarks>
    ///     Провал генерации миниатюры не откатывает операцию импорта.
    ///     Ошибки логируются как Warning для последующего анализа.
    /// </remarks>
    private void GenerateThumbnail(string originalFilePath)
    {
        var directory = Path.GetDirectoryName(originalFilePath);
        var fileName = Path.GetFileNameWithoutExtension(originalFilePath);
        var extension = Path.GetExtension(originalFilePath);

        var thumbnailDirectory = Path.Combine(directory ?? string.Empty, ".thumbnails");
        var thumbnailPath = Path.Combine(thumbnailDirectory, $"{fileName}_thumb{extension}");

        var result = _thumbnailService.Generate(originalFilePath, thumbnailPath, 400);

        if (result.IsFailure)
        {
            Log.Warning(
                "Не удалось создать миниатюру для файла {OriginalPath}. " +
                "Ошибка: {ErrorCode} - {ErrorMessage}. Фронтенд будет использовать оригинальный файл.",
                originalFilePath,
                result.Error.Code,
                result.Error.Message);
        }
        else
        {
            Log.Debug("Миниатюра успешно создана: {ThumbnailPath}", thumbnailPath);
        }
    }
}