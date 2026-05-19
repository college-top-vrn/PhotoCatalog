using System;
using System.IO;

using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

using PhotoSauce.MagicScaler;

using Serilog;

namespace PhotoCatalog.Infrastructure.Services;

/// <summary>
///     Реализация сервиса генерации миниатюр с использованием PhotoSauce.MagicScaler.
/// </summary>
/// <remarks>
///     MagicScaler — высокопроизводительная библиотека для обработки изображений,
///     оптимизированная для создания миниатюр и ресайза в веб-приложениях.
/// </remarks>
public class ThumbnailService : IThumbnailService
{
    private static readonly Error ImageProcessingError = new(
        "Thumbnail.ImageProcessingError",
        "Не удалось обработать изображение при создании миниатюры");

    private static readonly Error FileAccessError = new(
        "Thumbnail.FileAccessError",
        "Не удалось прочитать исходный файл или записать миниатюру");

    /// <inheritdoc />
    public ResultVoid Generate(string sourcePath, string targetPath, int maxSize)
    {
        try
        {
            if (!File.Exists(sourcePath))
            {
                return ResultVoid.Failure(new Error(
                    "Thumbnail.SourceNotFound",
                    $"Исходный файл не найден: {sourcePath}"));
            }

            // Создаём директорию для миниатюры, если её нет
            var directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Настройки ресайза для MagicScaler
            var settings = new ProcessImageSettings
            {
                Width = maxSize,
                Height = maxSize,
                ResizeMode = CropScaleMode.Crop
            };

            // Генерируем миниатюру
            MagicImageProcessor.ProcessImage(sourcePath, targetPath, settings);

            Log.Debug("Миниатюра создана: {TargetPath} (размер: {MaxSize}px)", targetPath, maxSize);

            return ResultVoid.Success();
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Error(ex, "Нет доступа к файлу: {SourcePath}", sourcePath);
            return ResultVoid.Failure(FileAccessError);
        }
        catch (IOException ex)
        {
            Log.Error(ex, "Ошибка ввода-вывода при работе с файлом: {SourcePath}", sourcePath);
            return ResultVoid.Failure(FileAccessError);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Необработанная ошибка при создании миниатюры: {SourcePath}", sourcePath);
            return ResultVoid.Failure(ImageProcessingError);
        }
    }
}