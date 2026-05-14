using System;
using System.IO;
using System.Security.Cryptography;

using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;
using PhotoCatalog.Infrastructure.Errors;

using Serilog;

namespace PhotoCatalog.Infrastructure.Services;

/// <summary>
///     Реализует контракт извлечения метаданных из файлов.
///     Предоставляет операции вычисления SHA-256-хэша и определения габаритов изображения.
///     Методы работают потоково и не загружают весь файл в оперативную память.
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
    /// <exception cref="UnauthorizedAccessException">
    ///     Может возникнуть, если у процесса отсутствуют права на чтение файла.
    /// </exception>
    public Result<string> CalculateHash(string filePath)
    {
        try
        {
            _logger.Debug("Начало вычисления хэша SHA-256 для файла: {FilePath}", filePath);

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192);
            using var sha256 = SHA256.Create();

            var hashBytes = sha256.ComputeHash(stream);
            var hashString = Convert.ToHexString(hashBytes).ToLowerInvariant();

            _logger.Debug("Хэш успешно вычислен для файла: {FilePath}, Hash={Hash}", filePath, hashString);

            return Result<string>.Success(hashString);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Error(ex, "Отказано в доступе при чтении файла для вычисления хэша: {FilePath}", filePath);
            return Result<string>.Failure(InfrastructureErrors.FileStorage.AccessDenied);
        }
        catch (IOException ex) when (ex.Message.Contains("used by another process", StringComparison.OrdinalIgnoreCase))
        {
            _logger.Error(ex, "Файл заблокирован другим процессом при вычислении хэша: {FilePath}", filePath);
            return Result<string>.Failure(InfrastructureErrors.MetadataExtractor.FileLocked);
        }
        catch (IOException ex)
        {
            _logger.Error(ex, "Ошибка ввода-вывода при вычислении хэша файла: {FilePath}, Message={ErrorMessage}",
                filePath, ex.Message);
            return Result<string>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Непредвиденная ошибка при вычислении хэша файла: {FilePath}", filePath);
            return Result<string>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);
        }
    }

    /// <summary>
    ///     Извлекает габариты изображения: ширину и высоту в пикселях.
    ///     Поддерживаются форматы PNG и JPEG.
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

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096);
            var result = TryReadDimensions(stream);

            if (result.IsSuccess)
            {
                _logger.Debug(
                    "Габариты успешно извлечены: {FilePath}, Width={Width}, Height={Height}",
                    filePath,
                    result.Value!.Width,
                    result.Value.Height);
            }
            else
            {
                _logger.Error(
                    "Ошибка извлечения габаритов изображения: {FilePath}, Code={ErrorCode}",
                    filePath,
                    result.Error.Code);
            }

            return result;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Error(ex, "Отказано в доступе при чтении изображения: {FilePath}", filePath);
            return Result<Dimensions>.Failure(InfrastructureErrors.FileStorage.AccessDenied);
        }
        catch (IOException ex) when (ex.Message.Contains("used by another process", StringComparison.OrdinalIgnoreCase))
        {
            _logger.Error(ex, "Файл изображения заблокирован другим процессом: {FilePath}", filePath);
            return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.FileLocked);
        }
        catch (IOException ex)
        {
            _logger.Error(ex, "Ошибка ввода-вывода при чтении изображения: {FilePath}, Message={ErrorMessage}",
                filePath, ex.Message);
            return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Непредвиденная ошибка при извлечении габаритов изображения: {FilePath}", filePath);
            return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);
        }
    }

    /// <summary>
    ///     Определяет формат файла и направляет выполнение в специализированный парсер размеров.
    /// </summary>
    /// <param name="stream">
    ///     Поток, содержащий бинарные данные файла изображения.
    /// </param>
    /// <returns>
    ///     Успешный результат с габаритами изображения,
    ///     либо провальный результат, если формат не распознан или файл поврежден.
    /// </returns>
    private Result<Dimensions> TryReadDimensions(Stream stream)
    {
        var header = new byte[8];

        if (stream.Read(header, 0, header.Length) != header.Length)
            return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);

        if (IsPng(header))
            return ReadPng(stream);

        if (IsJpeg(header))
            return ReadJpeg(stream);

        return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.NotAnImage);
    }

    /// <summary>
    ///     Проверяет, соответствует ли заголовок сигнатуре PNG.
    /// </summary>
    /// <param name="header">
    ///     Первые байты файла.
    /// </param>
    /// <returns>
    ///     <c>true</c>, если заголовок соответствует PNG; иначе <c>false</c>.
    /// </returns>
    private static bool IsPng(byte[] header) =>
        header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
        header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;

    /// <summary>
    ///     Проверяет, соответствует ли заголовок сигнатуре JPEG.
    /// </summary>
    /// <param name="header">
    ///     Первые байты файла.
    /// </param>
    /// <returns>
    ///     <c>true</c>, если заголовок соответствует JPEG; иначе <c>false</c>.
    /// </returns>
    private static bool IsJpeg(byte[] header) =>
        header[0] == 0xFF && header[1] == 0xD8;

    /// <summary>
    ///     Извлекает габариты изображения из PNG-файла.
    ///     Размеры хранятся в чанке <c>IHDR</c>.
    /// </summary>
    /// <param name="stream">
    ///     Поток, содержащий PNG-файл.
    /// </param>
    /// <returns>
    ///     Успешный результат с шириной и высотой изображения,
    ///     либо провальный результат, если файл поврежден.
    /// </returns>
    private Result<Dimensions> ReadPng(Stream stream)
    {
        var lengthBytes = new byte[4];
        var typeBytes = new byte[4];
        var data = new byte[8];

        if (stream.Read(lengthBytes, 0, 4) != 4)
            return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);

        if (stream.Read(typeBytes, 0, 4) != 4)
            return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);

        if (System.Text.Encoding.ASCII.GetString(typeBytes) != "IHDR")
            return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);

        if (stream.Read(data, 0, 8) != 8)
            return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);

        int width = ReadInt32BigEndian(data, 0);
        int height = ReadInt32BigEndian(data, 4);

        return Dimensions.Create(width, height);
    }

    /// <summary>
    ///     Извлекает габариты изображения из JPEG-файла.
    ///     Метод ищет сегмент, содержащий данные о ширине и высоте.
    /// </summary>
    /// <param name="stream">
    ///     Поток, содержащий JPEG-файл.
    /// </param>
    /// <returns>
    ///     Успешный результат с шириной и высотой изображения,
    ///     либо провальный результат, если файл поврежден.
    /// </returns>
    private Result<Dimensions> ReadJpeg(Stream stream)
    {
        while (true)
        {
            int prefix = stream.ReadByte();
            if (prefix == -1)
                return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);

            if (prefix != 0xFF)
                continue;

            int marker = stream.ReadByte();
            if (marker == -1)
                return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);

            while (marker == 0xFF)
            {
                marker = stream.ReadByte();
                if (marker == -1)
                    return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);
            }

            if (marker is 0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC5 or 0xC6 or 0xC7 or 0xC9 or 0xCA or 0xCB or 0xCD or 0xCE
                or 0xCF)
            {
                var size = new byte[7];
                if (stream.Read(size, 0, size.Length) != size.Length)
                    return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);

                int height = (size[3] << 8) | size[4];
                int width = (size[5] << 8) | size[6];

                return Dimensions.Create(width, height);
            }

            var lengthBytes = new byte[2];
            if (stream.Read(lengthBytes, 0, 2) != 2)
                return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);

            int segmentLength = (lengthBytes[0] << 8) | lengthBytes[1];
            if (segmentLength < 2)
                return Result<Dimensions>.Failure(InfrastructureErrors.MetadataExtractor.FileCorrupted);

            stream.Seek(segmentLength - 2, SeekOrigin.Current);
        }
    }

    /// <summary>
    ///     Преобразует 4 байта в целое число в формате big-endian.
    /// </summary>
    /// <param name="buffer">
    ///     Массив байтов, содержащий число.
    /// </param>
    /// <param name="offset">
    ///     Смещение, с которого начинается число.
    /// </param>
    /// <returns>
    ///     Целое число, прочитанное из массива байтов.
    /// </returns>
    private static int ReadInt32BigEndian(byte[] buffer, int offset) =>
        (buffer[offset] << 24) | (buffer[offset + 1] << 16) | (buffer[offset + 2] << 8) | buffer[offset + 3];
}