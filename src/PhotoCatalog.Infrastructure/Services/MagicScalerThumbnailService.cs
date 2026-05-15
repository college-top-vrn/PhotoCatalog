using System;

using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

using PhotoSauce.MagicScaler;

namespace PhotoCatalog.Infrastructure.Services;

using Serilog;
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
            var settings = new ProcessImageSettings
            {
                Width = maxSize,
                Height = maxSize,
                ResizeMode = CropScaleMode.Max
            };

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