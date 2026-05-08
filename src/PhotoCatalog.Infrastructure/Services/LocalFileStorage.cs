using System;
using System.IO;

using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

using Serilog;

namespace PhotoCatalog.Infrastructure.Services;

/// <summary>
///     Локальная реализация контракта файлового хранилища.
/// </summary>
/// <remarks>
///     Контракт работает только с физическими путями и байтами,
///     не содержит бизнес-логики и не выбрасывает инфраструктурные исключения наружу.
///     Все системные сбои транслируются в предсказуемые объекты <see cref="Result{T}" />
///     и <see cref="ResultVoid" />.
/// </remarks>
public sealed class LocalFileStorage : IFileStorage
{
    private readonly ILogger _logger;
    private readonly string _storageRootPath;

    /// <summary>
    ///     Инициализирует сервис локального хранилища.
    /// </summary>
    /// <param name="logger">
    ///     Логгер для фиксации инфраструктурных сбоев
    ///     (ошибки доступа, блокировки файлов, ошибки ввода-вывода).
    /// </param>
    /// <param name="storageRootPath">
    ///     Базовый путь директории, в которую сохраняются файлы.
    ///     Все операции сохранения выполняются относительно этого пути.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Выбрасывается, если логгер не передан.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Выбрасывается, если базовый путь хранилища пустой.
    /// </exception>
    public LocalFileStorage(ILogger logger, string storageRootPath)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrWhiteSpace(storageRootPath))
        {
            throw new ArgumentException("Базовый путь хранилища не может быть пустым.", nameof(storageRootPath));
        }

        _storageRootPath = Path.GetFullPath(storageRootPath);
    }

    /// <summary>
    ///     Копирует исходный файл в локальное хранилище с новым именем.
    /// </summary>
    /// <param name="sourcePath">
    ///     Абсолютный путь к исходному файлу.
    /// </param>
    /// <param name="newFileName">
    ///     Целевое имя файла внутри хранилища.
    /// </param>
    /// <returns>
    ///     Успешный результат с итоговым абсолютным путем,
    ///     либо инфраструктурную ошибку доступа/диска/ввода-вывода.
    /// </returns>
    public Result<string> StoreFile(string sourcePath, string newFileName)
    {
        try
        {
            var safeFileName = Path.GetFileName(newFileName);
            var destinationPath = Path.Combine(_storageRootPath, safeFileName);

            Directory.CreateDirectory(_storageRootPath);
            File.Copy(sourcePath, destinationPath, overwrite: true);

            return Result<string>.Success(destinationPath);
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.Error(
                exception,
                "Отказано в доступе при сохранении файла. SourcePath={SourcePath}, NewFileName={NewFileName}, StorageRootPath={StorageRootPath}",
                sourcePath,
                newFileName,
                _storageRootPath);

            return Result<string>.Failure(InfrastructureErrors.FileStorage.AccessDenied);
        }
        catch (IOException exception)
        {
            var mappedError = MapIoError(exception);

            _logger.Error(
                exception,
                "Ошибка ввода-вывода при сохранении файла. SourcePath={SourcePath}, NewFileName={NewFileName}, StorageRootPath={StorageRootPath}",
                sourcePath,
                newFileName,
                _storageRootPath);

            return Result<string>.Failure(mappedError);
        }
    }

    /// <summary>
    ///     Удаляет физический файл с диска.
    /// </summary>
    /// <param name="filePath">
    ///     Путь к файлу. Допускается как абсолютный путь,
    ///     так и путь относительно базовой директории хранилища.
    /// </param>
    /// <returns>
    ///     Успех, если файл удален или уже отсутствует;
    ///     иначе провальный результат с инфраструктурной ошибкой.
    /// </returns>
    public ResultVoid DeleteFile(string filePath)
    {
        var resolvedPath = ResolvePath(filePath);

        try
        {
            if (!File.Exists(resolvedPath))
            {
                return ResultVoid.Success();
            }

            File.Delete(resolvedPath);
            return ResultVoid.Success();
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.Error(
                exception,
                "Отказано в доступе при удалении файла. FilePath={FilePath}, ResolvedPath={ResolvedPath}",
                filePath,
                resolvedPath);

            return ResultVoid.Failure(InfrastructureErrors.FileStorage.AccessDenied);
        }
        catch (IOException exception)
        {
            var mappedError = MapIoError(exception);

            _logger.Error(
                exception,
                "Ошибка ввода-вывода при удалении файла. FilePath={FilePath}, ResolvedPath={ResolvedPath}",
                filePath,
                resolvedPath);

            return ResultVoid.Failure(mappedError);
        }
    }

    /// <summary>
    ///     Проверяет существование файла на диске.
    /// </summary>
    /// <param name="filePath">
    ///     Путь к файлу. Допускается как абсолютный путь,
    ///     так и путь относительно базовой директории хранилища.
    /// </param>
    /// <returns>
    ///     Всегда успешный <see cref="Result{T}" /> со значением true/false.
    ///     Исключения файловой системы не пробрасываются и не превращаются в failure.
    /// </returns>
    public Result<bool> FileExists(string filePath)
    {
        var resolvedPath = ResolvePath(filePath);

        try
        {
            return Result<bool>.Success(File.Exists(resolvedPath));
        }
        catch (Exception exception)
        {
            _logger.Error(
                exception,
                "Непредвиденная ошибка проверки существования файла. FilePath={FilePath}, ResolvedPath={ResolvedPath}",
                filePath,
                resolvedPath);

            return Result<bool>.Success(false);
        }
    }

    private string ResolvePath(string filePath)
    {
        return Path.IsPathRooted(filePath)
            ? filePath
            : Path.Combine(_storageRootPath, filePath);
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