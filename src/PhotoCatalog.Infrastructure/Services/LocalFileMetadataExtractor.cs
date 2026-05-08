using System;
using System.IO;
using System.Security.Cryptography;

using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;
using PhotoCatalog.Infrastructure.Errors;

using Serilog;

using SixLabors.ImageSharp;

namespace PhotoCatalog.Infrastructure.Services;

/// <summary>
///     Локальная реализация контракта извлечения файловых метаданных.
/// </summary>
/// <remarks>
///     Сервис не содержит бизнес-логики и не загружает весь файл в оперативную память.
///     Для вычисления хэша используется потоковое чтение,
///     для габаритов изображения используется чтение заголовков через ImageSharp.
/// </remarks>
public sealed class LocalFileMetadataExtractor : IFileMetadataExtractor
{
    private readonly ILogger _logger;

    /// <summary>
    ///     Инициализирует сервис извлечения метаданных файлов.
    /// </summary>
    /// <param name="logger">
    ///     Логгер для фиксации ошибок чтения, блокировок и поврежденных файлов.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Выбрасывается, если логгер не передан.
    /// </exception>
    public LocalFileMetadataExtractor(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Вычисляет SHA-256 хэш файла через потоковое чтение.
    /// </summary>
    /// <param name="filePath">
    ///     Путь к файлу, для которого нужно вычислить контрольную сумму.
    /// </param>
    /// <returns>
    ///     Успешный результат с хэшем в шестнадцатеричном формате
    ///     или провальный результат с инфраструктурной ошибкой.
    /// </returns>
    public Result<string> CalculateHash(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            var hashBytes = SHA256.HashData(stream);
            var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();

            return Result<string>.Success(hash);
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.Error(
                exception,
                "Отказано в доступе при вычислении хэша файла. FilePath={FilePath}",
                filePath);

            return Result<string>.Failure(InfrastructureErrors.FileStorage.AccessDenied);
        }
        catch (IOException exception)
        {
            var mappedError = MapIoError(exception);

            _logger.Error(
                exception,
                "Ошибка ввода-вывода при вычислении хэша файла. FilePath={FilePath}",
                filePath);

            return Result<string>.Failure(mappedError);
        }
    }

    /// <summary>
    ///     Извлекает ширину и высоту изображения в пикселях.
    /// </summary>
    /// <param name="filePath">
    ///     Путь к изображению.
    /// </param>
    /// <returns>
    ///     Успешный результат с доменным объектом <see cref="Dimensions" />
    ///     или провальный результат с инфраструктурной ошибкой.
    /// </returns>
    public Result<Dimensions> GetDimensions(string filePath)
    {
        try
        {
            var info = Image.Identify(filePath);
            if (info is null)
            {
                _logger.Error(
                    "Не удалось определить формат или метаданные изображения. FilePath={FilePath}",
                    filePath);

                return Result<Dimensions>.Failure(InfrastructureErrors.FileStorage.IOError);
            }

            var dimensionsResult = Dimensions.Create(info.Width, info.Height);
            return dimensionsResult.IsFailure
                ? Result<Dimensions>.Failure(dimensionsResult.Error)
                : Result<Dimensions>.Success(dimensionsResult.Value!);
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.Error(
                exception,
                "Отказано в доступе при чтении габаритов изображения. FilePath={FilePath}",
                filePath);

            return Result<Dimensions>.Failure(InfrastructureErrors.FileStorage.AccessDenied);
        }
        catch (IOException exception)
        {
            var mappedError = MapIoError(exception);

            _logger.Error(
                exception,
                "Ошибка ввода-вывода при чтении габаритов изображения. FilePath={FilePath}",
                filePath);

            return Result<Dimensions>.Failure(mappedError);
        }
        catch (UnknownImageFormatException exception)
        {
            _logger.Error(
                exception,
                "Файл не является поддерживаемым изображением или поврежден. FilePath={FilePath}",
                filePath);

            return Result<Dimensions>.Failure(InfrastructureErrors.FileStorage.IOError);
        }
    }

    private static Error MapIoError(IOException exception)
    {
        return IsDiskFull(exception)
            ? InfrastructureErrors.FileStorage.DiskFull
            : InfrastructureErrors.FileStorage.IOError;
    }

    private static bool IsDiskFull(IOException exception)
    {
        const int ErrorDiskFull = 0x70;
        const int ErrorHandleDiskFull = 0x27;

        var win32ErrorCode = exception.HResult & 0xFFFF;
        return win32ErrorCode is ErrorDiskFull or ErrorHandleDiskFull;
    }
}
