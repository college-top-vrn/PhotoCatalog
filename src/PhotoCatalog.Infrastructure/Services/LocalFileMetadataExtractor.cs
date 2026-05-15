using System;
using System.IO;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Gif;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;
using Microsoft.Extensions.Logging;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;
using PhotoCatalog.Infrastructure.Errors;

namespace PhotoCatalog.Infrastructure.Services;

/// <summary>
///     Реализует контракт извлечения метаданных из файлов.
///     Предоставляет операции вычисления SHA-256-хэша и определения габаритов изображения.
///     Методы работают потоково и не загружают весь файл в оперативную память.
/// </summary>
public sealed class LocalFileMetadataExtractor : IFileMetadataExtractor
{
    private readonly ILogger<LocalFileMetadataExtractor> _logger;

    /// <summary>
    ///     Инициализирует новый экземпляр <see cref="LocalFileMetadataExtractor" />.
    /// </summary>
    /// <param name="logger">
    ///     Контракт логирования Microsoft.Extensions.Logging, используемый для записи диагностических сообщений,
    ///     предупреждений и ошибок.
    /// </param>
    public LocalFileMetadataExtractor(ILogger<LocalFileMetadataExtractor> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Вычисляет криптографический хэш файла по алгоритму SHA-256.
    ///     Чтение файла выполняется потоково, без загрузки содержимого целиком в память.
    /// </summary>
    /// <param name="filePath">
    ///     Абсолютный путь к файлу, для которого необходимо вычислить хэш.
    /// </param>
    /// <returns>
    ///     Успешный результат с хэш-строкой в шестнадцатеричном формате,
    ///     либо провальный результат с ошибкой инфраструктурного уровня.
    /// </returns>
    public Result<string> CalculateHash(string filePath)
    {
        try
        {
            _logger.LogDebug("Начало вычисления хэша SHA-256 для файла: {FilePath}", filePath);

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192);
            using var sha256 = System.Security.Cryptography.SHA256.Create();

            byte[] hashBytes = sha256.ComputeHash(stream);
            string hashString = Convert.ToHexString(hashBytes).ToLowerInvariant();

            _logger.LogDebug("Хэш успешно вычислен для файла: {FilePath}, Hash={Hash}", filePath, hashString);

            return Result<string>.Success(hashString);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Отказано в доступе при чтении файла для вычисления хэша: {FilePath}", filePath);
            return Result<string>.Failure(InfrastructureErrors.FileStorage.AccessDenied);
        }
        catch (IOException ex) when (ex.Message.Contains("used by another process", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError(ex, "Файл заблокирован другим процессом при вычислении хэша: {FilePath}", filePath);
            return Result<string>.Failure(InfrastructureErrors.MetadataExtractor.FileLocked);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Ошибка ввода-вывода при вычислении хэша файла: {FilePath}, Message={ErrorMessage}",
                filePath, ex.Message);
            return Result<string>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при вычислении хэша файла: {FilePath}", filePath);
            return Result<string>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);
        }
    }

    /// <summary>
    ///     Извлекает габариты изображения: ширину и высоту в пикселях.
    ///     Поддерживаются форматы JPEG, PNG, GIF и другие через библиотеку MetadataExtractor.
    ///     Метод работает только с заголовками файлов (нулевое декодирование матрицы пикселей).
    /// </summary>
    /// <param name="filePath">
    ///     Абсолютный путь к файлу изображения.
    /// </param>
    /// <returns>
    ///     Успешный результат с объектом <see cref="Dimensions" />,
    ///     либо провальный результат с ошибкой инфраструктурного уровня.
    /// </returns>
    public Result<Dimensions> GetDimensions(string filePath)
    {
        try
        {
            _logger.LogDebug("Начало извлечения габаритов изображения: {FilePath}", filePath);

            IReadOnlyList<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(filePath);

            Dimensions? dimensions = ExtractDimensions(directories);

            if (dimensions == null)
            {
                _logger.LogError("Не удалось найти габариты изображения: {FilePath}", filePath);
                return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.DimensionsNotFound);
            }

            _logger.LogDebug(
                "Габариты успешно извлечены: {FilePath}, Width={Width}, Height={Height}",
                filePath,
                dimensions.Value.Width,
                dimensions.Value.Height);

            return Result<Dimensions>.Success(dimensions.Value);
        }
        catch (ImageProcessingException ex)
        {
            _logger.LogError(ex, "Файл не является корректным изображением: {FilePath}", filePath);
            return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.NotAnImage);
        }
        catch (IOException ex) when (ex.Message.Contains("used by another process", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError(ex, "Файл изображения заблокирован другим процессом: {FilePath}", filePath);
            return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.FileLocked);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Ошибка ввода-вывода при чтении изображения: {FilePath}, Message={ErrorMessage}",
                filePath, ex.Message);
            return Result<Dimensions>.Failure(InfrastructureErrors.FileStorage.IOError);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Отказано в доступе при чтении изображения: {FilePath}", filePath);
            return Result<Dimensions>.Failure(InfrastructureErrors.FileStorage.AccessDenied);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при извлечении габаритов изображения: {FilePath}", filePath);
            return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);
        }
    }

    /// <summary>
    ///     Извлекает габариты изображения из коллекции директорий метаданных.
    ///     Обходит директории в поиске тегов ширины и высоты.
    ///     Поддерживает различные форматы: JPEG, PNG, GIF, а также Exif-директории.
    /// </summary>
    /// <param name="directories">
    ///     Коллекция директорий с метаданными, полученная из библиотеки MetadataExtractor.
    /// </param>
    /// <returns>
    ///     Объект <see cref="Dimensions" /> с шириной и высотой,
    ///     либо <c>null</c>, если размеры не найдены.
    /// </returns>
    private static Dimensions? ExtractDimensions(IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        foreach (MetadataExtractor.Directory directory in directories)
        {
            if (!directory.TryGetInt32(MetadataExtractor.Formats.Jpeg.JpegDirectory.TagImageWidth, out int width))
            {
                if (!directory.TryGetInt32(MetadataExtractor.Formats.Png.PngDirectory.TagImageWidth, out width))
                {
                    if (!directory.TryGetInt32(MetadataExtractor.Formats.Gif.GifHeaderDirectory.TagImageWidth, out width))
                    {
                        if (directory is ExifIfd0Directory exifDir)
                        {
                            if (!exifDir.TryGetInt32(ExifIfd0Directory.TagImageWidth, out width))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }

            if (directory.TryGetInt32(MetadataExtractor.Formats.Jpeg.JpegDirectory.TagImageHeight, out int height))
            {
                return Dimensions.Create(width, height).Value;
            }

            if (directory.TryGetInt32(MetadataExtractor.Formats.Png.PngDirectory.TagImageHeight, out height))
            {
                return Dimensions.Create(width, height).Value;
            }

            if (directory.TryGetInt32(MetadataExtractor.Formats.Gif.GifHeaderDirectory.TagImageHeight, out height))
            {
                return Dimensions.Create(width, height).Value;
            }

            if (directory is ExifIfd0Directory exifDirHeight && exifDirHeight.TryGetInt32(ExifIfd0Directory.TagImageHeight, out height))
            {
                return Dimensions.Create(width, height).Value;
            }
        }

        return null;
    }
}