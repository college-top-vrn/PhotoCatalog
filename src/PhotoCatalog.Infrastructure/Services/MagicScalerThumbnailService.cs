using System;

using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

using PhotoSauce.MagicScaler;

using Serilog;

namespace PhotoCatalog.Infrastructure.Services;

/// <summary>
///     Сервис для генерации миниатюр изображений с использованием <see cref="MagicImageProcessor" />.
/// </summary>
/// <remarks>
///     Сервис автоматически сохраняет пропорции изображения, применяет режим ресайза
///     <see cref="CropScaleMode.Max" /> и использует JPEG как выходной формат.
/// </remarks>
public class MagicScalerThumbnailService : IThumbnailService
{
    private readonly ILogger _logger;

    /// <summary>
    ///     Инициализирует новый экземпляр <see cref="MagicScalerThumbnailService" />.
    /// </summary>
    /// <param name="logger">Логгер для записи ошибок и служебных сообщений.</param>
    public MagicScalerThumbnailService(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public ResultVoid Generate(string sourcePath, string targetPath, int maxSize)
    {
        try
        {
            _logger.Information(
                "Начинается генерация миниатюры. Исходный файл: {SourcePath}, результат: {TargetPath}, размер: {MaxSize}",
                sourcePath, targetPath, maxSize);
            ProcessImageSettings settings = new() { Width = maxSize, Height = maxSize, ResizeMode = CropScaleMode.Max };

            settings.TrySetEncoderFormat(ImageMimeTypes.Jpeg);

            MagicImageProcessor.ProcessImage(sourcePath, targetPath, settings);
            return ResultVoid.Success();
        }
        catch (Exception ex)
        {
            _logger.Error(
                ex,
                "Не удалось создать миниатюру. Исходный файл: {SourcePath}, результат: {TargetPath}, размер: {MaxSize}",
                sourcePath, targetPath, maxSize);
            return ResultVoid.Failure(InfrastructureErrors.FileStorage.ThumbnailGenerationFailed);
        }
    }
}