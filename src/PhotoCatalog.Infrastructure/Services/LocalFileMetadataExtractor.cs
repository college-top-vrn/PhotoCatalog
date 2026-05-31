using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Gif;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;

using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;
using PhotoCatalog.Infrastructure.Errors;

using Serilog;

using Directory = MetadataExtractor.Directory;

namespace PhotoCatalog.Infrastructure.Services;

/// <summary>
///     Реализует контракт извлечения метаданных из файлов.
///     Предоставляет операции вычисления SHA-256-хэша и определения габаритов изображения.
///     Методы работают потоково и не загружают весь файл в оперативную память.
///     Для извлечения размеров изображения используется библиотека MetadataExtractor,
///     которая читает только заголовки файлов без декодирования матрицы пикселей.
/// </summary>
public sealed class LocalFileMetadataExtractor : IFileMetadataExtractor
{
    private readonly ILogger _logger;

    /// <summary>
    ///     Инициализирует новый экземпляр <see cref="LocalFileMetadataExtractor" />.
    /// </summary>
    /// <param name="logger">
    ///     Контракт логирования Serilog, используемый для записи диагностических сообщений,
    ///     предупреждений и ошибок.
    /// </param>
    public LocalFileMetadataExtractor(ILogger logger)
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
            _logger.Debug("Начало вычисления хэша SHA-256 для файла: {FilePath}", filePath);

            using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192);
            using SHA256 sha256 = SHA256.Create();

            byte[] hashBytes = sha256.ComputeHash(stream);
            string hashString = Convert.ToHexString(hashBytes).ToLowerInvariant();

            _logger.Debug("Хэш успешно вычислен для файла: {FilePath}, Hash={Hash}", filePath, hashString);

            return Result.Success(hashString);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Error(ex, "Отказано в доступе при чтении файла для вычисления хэша: {FilePath}", filePath);
            return Result.Failure<string>(InfrastructureErrors.FileStorage.AccessDenied);
        }
        catch (IOException ex) when (ex.Message.Contains("used by another process", StringComparison.OrdinalIgnoreCase))
        {
            _logger.Error(ex, "Файл заблокирован другим процессом при вычислении хэша: {FilePath}", filePath);
            return Result.Failure<string>(InfrastructureErrors.MetadataExtractor.FileLocked);
        }
        catch (IOException ex)
        {
            _logger.Error(ex, "Ошибка ввода-вывода при вычислении хэша файла: {FilePath}, Message={ErrorMessage}",
                filePath, ex.Message);
            return Result.Failure<string>(InfrastructureErrors.MetadataExtractor.FileCorrupted);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Непредвиденная ошибка при вычислении хэша файла: {FilePath}", filePath);
            return Result.Failure<string>(InfrastructureErrors.MetadataExtractor.FileCorrupted);
        }
    }

    /// <summary>
    ///     Извлекает габариты изображения: ширину и высоту в пикселях.
    ///     Метод использует библиотеку MetadataExtractor, которая читает только заголовки файлов
    ///     без декодирования матрицы пикселей, что обеспечивает высокую производительность.
    ///     Поддерживаются различные форматы изображений: JPEG, PNG, GIF, BMP, TIFF и другие.
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
            _logger.Debug("Начало извлечения габаритов изображения: {FilePath}", filePath);

            // Используем библиотеку MetadataExtractor для чтения метаданных
            // Библиотека работает только с заголовками файлов, не декодирует пиксели
            IReadOnlyList<Directory> directories = ImageMetadataReader.ReadMetadata(filePath);

            int? width = null;
            int? height = null;

            // Обход всех директорий в поисках тегов ширины и высоты
            foreach (Directory directory in directories)
            {
                // Пропускаем директории с ошибками
                if (directory.HasError)
                {
                    _logger.Debug("Директория {DirectoryName} содержит ошибки: {Errors}",
                        directory.Name, string.Join(", ", directory.Errors));
                    continue;
                }

                // Поиск ширины
                width ??= TryGetImageWidth(directory);

                // Поиск высоты
                height ??= TryGetImageHeight(directory);

                // Если оба значения найдены - прекращаем поиск
                if (width.HasValue && height.HasValue)
                {
                    break;
                }
            }

            // Проверка: удалось ли найти оба размера
            if (!width.HasValue || !height.HasValue)
            {
                _logger.Error(
                    "Не удалось найти размеры изображения в файле: {FilePath}. WidthFound={WidthFound}, HeightFound={HeightFound}",
                    filePath,
                    width.HasValue,
                    height.HasValue);

                return Result.Failure<Dimensions>(InfrastructureErrors.MetadataExtractor.MetadataNotFound);
            }

            _logger.Debug(
                "Габариты успешно извлечены: {FilePath}, Width={Width}, Height={Height}",
                filePath,
                width.Value,
                height.Value);

            return Dimensions.Create(width.Value, height.Value);
        }
        catch (ImageProcessingException ex)
        {
            // Файл не является изображением или имеет неподдерживаемый формат
            _logger.Error(ex, "Файл не является корректным изображением или формат не поддерживается: {FilePath}",
                filePath);
            return Result.Failure<Dimensions>(InfrastructureErrors.MetadataExtractor.NotAnImage);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Error(ex, "Отказано в доступе при чтении изображения: {FilePath}", filePath);
            return Result.Failure<Dimensions>(InfrastructureErrors.FileStorage.AccessDenied);
        }
        catch (IOException ex) when (ex.Message.Contains("used by another process", StringComparison.OrdinalIgnoreCase))
        {
            _logger.Error(ex, "Файл изображения заблокирован другим процессом: {FilePath}", filePath);
            return Result.Failure<Dimensions>(InfrastructureErrors.MetadataExtractor.FileLocked);
        }
        catch (IOException ex)
        {
            _logger.Error(ex, "Ошибка ввода-вывода при чтении изображения: {FilePath}, Message={ErrorMessage}",
                filePath, ex.Message);
            return Result.Failure<Dimensions>(InfrastructureErrors.MetadataExtractor.FileCorrupted);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Непредвиденная ошибка при извлечении габаритов изображения: {FilePath}", filePath);
            return Result.Failure<Dimensions>(InfrastructureErrors.MetadataExtractor.FileCorrupted);
        }
    }

    /// <summary>
    ///     Пытается извлечь ширину изображения из директории метаданных.
    ///     Проверяет различные типы директорий и их теги.
    /// </summary>
    /// <param name="directory">
    ///     Директория с метаданными изображения.
    /// </param>
    /// <returns>
    ///     Значение ширины, если найдено; иначе <c>null</c>.
    /// </returns>
    private static int? TryGetImageWidth(Directory directory)
    {
        switch (directory)
        {
            // JPEG директория
            case JpegDirectory jpegDir when jpegDir.TryGetInt32(JpegDirectory.TagImageWidth, out int width):
                return width;
            // PNG директория
            case PngDirectory pngDir when pngDir.TryGetInt32(PngDirectory.TagImageWidth, out int width):
                return width;
            // GIF директория
            case GifHeaderDirectory gifDir when gifDir.TryGetInt32(GifHeaderDirectory.TagImageWidth, out int width):
                return width;
            // Exif IFD0 директория (содержит базовые теги изображения)
            case ExifIfd0Directory exifDir when exifDir.TryGetInt32(ExifDirectoryBase.TagImageWidth, out int width):
                return width;
        }

        // Общий поиск по тегам, которые могут содержать ширину
        int[] widthTags = [0xA002, 0x0100, 0x0112];
        foreach (int tag in widthTags)
        {
            if (directory.TryGetInt32(tag, out int width))
            {
                return width;
            }
        }

        return null;
    }

    /// <summary>
    ///     Пытается извлечь высоту изображения из директории метаданных.
    ///     Проверяет различные типы директорий и их теги.
    /// </summary>
    /// <param name="directory">
    ///     Директория с метаданными изображения.
    /// </param>
    /// <returns>
    ///     Значение высоты, если найдено; иначе <c>null</c>.
    /// </returns>
    private static int? TryGetImageHeight(Directory directory)
    {
        switch (directory)
        {
            // JPEG директория
            case JpegDirectory jpegDir when jpegDir.TryGetInt32(JpegDirectory.TagImageHeight, out int height):
                return height;
            // PNG директория
            case PngDirectory pngDir when pngDir.TryGetInt32(PngDirectory.TagImageHeight, out int height):
                return height;
            // GIF директория
            case GifHeaderDirectory gifDir when gifDir.TryGetInt32(GifHeaderDirectory.TagImageHeight, out int height):
                return height;
            // Exif IFD0 директория (содержит базовые теги изображения)
            case ExifIfd0Directory exifDir when exifDir.TryGetInt32(ExifDirectoryBase.TagImageHeight, out int height):
                return height;
        }

        // Общий поиск по тегам, которые могут содержать высоту
        int[] heightTags = [0xA003, 0x0101, 0x0117];
        foreach (int tag in heightTags)
        {
            if (directory.TryGetInt32(tag, out int height))
            {
                return height;
            }
        }

        return null;
    }
}