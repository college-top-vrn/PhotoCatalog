// Infrastructure/Services/LocalFileStorage.cs

using System;
using System.IO;

using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

using Serilog;

namespace PhotoCatalog.Infrastructure.Services;

/// <summary>
/// Реализация контракта IFileStorage, обеспечивающая физическую работу с файлами на локальном жестком диске.
/// Класс изолирует ядро приложения от системных исключений файловой системы, преобразуя их в предсказуемые
/// объекты Result с подробным логированием каждого сбоя.
/// </summary>
public sealed class LocalFileStorage : IFileStorage
{
    private readonly ILogger _logger;
    private readonly string _baseStoragePath;

    /// <summary>
    /// Инициализирует новый экземпляр сервиса файлового хранилища.
    /// </summary>
    /// <param name="logger">Сервис логирования Serilog для записи системных событий и ошибок.</param>
    /// <param name="baseStoragePath">Базовый путь к корневой директории хранилища. Все операции выполняются относительно этого пути.</param>
    public LocalFileStorage(ILogger logger, string baseStoragePath)
    {
        _logger = logger;
        _baseStoragePath = baseStoragePath;

        // Убеждаемся, что базовая директория существует
        if (!Directory.Exists(_baseStoragePath))
        {
            Directory.CreateDirectory(_baseStoragePath);
            _logger.Information("Создана базовая директория хранилища: {BaseStoragePath}", _baseStoragePath);
        }
    }

    /// <summary>
    /// Сохраняет файл в хранилище, копируя его из указанного исходного расположения.
    /// </summary>
    /// <param name="sourcePath">Абсолютный путь к исходному файлу, который необходимо скопировать.</param>
    /// <param name="newFileName">Желаемое имя файла в целевом хранилище.</param>
    /// <returns>
    /// В случае успеха возвращает Result с абсолютным путем к сохраненному файлу.
    /// При возникновении ошибки возвращает Result с соответствующей инфраструктурной ошибкой.
    /// </returns>
    public Result<string> StoreFile(string sourcePath, string newFileName)
    {
        try
        {
            _logger.Debug("Начало операции сохранения файла: {@Parameters}", new { sourcePath, newFileName });

            // Валидация входных параметров
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                _logger.Warning("Попытка сохранения с пустым исходным путем");
                return Result<string>.Failure(InfrastructureErrors.FileStorage.InvalidPath);
            }

            if (string.IsNullOrWhiteSpace(newFileName))
            {
                _logger.Warning("Попытка сохранения с пустым именем файла");
                return Result<string>.Failure(InfrastructureErrors.FileStorage.InvalidPath);
            }

            // Проверка существования исходного файла
            if (!File.Exists(sourcePath))
            {
                _logger.Warning("Исходный файл не существует: {SourcePath}", sourcePath);
                return Result<string>.Failure(InfrastructureErrors.FileStorage.IOError);
            }

            var destinationPath = NormalizeAndValidatePath(newFileName);
            if (destinationPath.IsFailure)
            {
                return Result<string>.Failure(destinationPath.Error); // TODO Сделать ошибку в InfrastructureErrors
            }

            var targetFullPath = Path.Combine(_baseStoragePath, destinationPath.Value!);
            var targetDirectory = Path.GetDirectoryName(targetFullPath);

            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
                _logger.Debug("Создана директория: {DirectoryPath}", targetDirectory);
            }

            // Копирование файла (overwrite = false для предотвращения случайной перезаписи)
            File.Copy(sourcePath, targetFullPath, overwrite: false);

            _logger.Information("Файл успешно сохранен: {TargetPath}", targetFullPath);
            return Result<string>.Success(targetFullPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Error(ex, "Отказано в доступе при сохранении файла. SourcePath={SourcePath}, NewFileName={NewFileName}",
                sourcePath, newFileName);
            return Result<string>.Failure(InfrastructureErrors.FileStorage.AccessDenied);
        }
        catch (IOException ex) when (ex.Message.Contains("not enough space") || ex.Message.Contains("disk full") || ex.HResult == -2147024784)
        {
            _logger.Error(ex, "Недостаточно места на диске при сохранении файла. SourcePath={SourcePath}", sourcePath);
            return Result<string>.Failure(InfrastructureErrors.FileStorage.DiskFull);
        }
        catch (IOException ex)
        {
            _logger.Error(ex, "Ошибка ввода-вывода при сохранении файла. SourcePath={SourcePath}, Message={ErrorMessage}",
                sourcePath, ex.Message);
            return Result<string>.Failure(InfrastructureErrors.FileStorage.IOError);
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Непредвиденная ошибка при сохранении файла. SourcePath={SourcePath}", sourcePath);
            return Result<string>.Failure(InfrastructureErrors.FileStorage.IOError);
        }
    }

    /// <summary>
    /// Удаляет файл из файловой системы по указанному пути.
    /// Операция идемпотентна: если файл уже отсутствует, метод завершается успешно.
    /// </summary>
    /// <param name="filePath">Путь к удаляемому файлу (абсолютный или относительный).</param>
    /// <returns>
    /// В случае успеха возвращает успешный ResultVoid.
    /// При возникновении ошибки возвращает ResultVoid с соответствующей инфраструктурной ошибкой.
    /// </returns>
    public ResultVoid DeleteFile(string filePath)
    {
        try
        {
            _logger.Debug("Начало операции удаления файла: {FilePath}", filePath);

            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.Debug("Путь к файлу пуст - операция удаления пропущена (идемпотентность)");
                return ResultVoid.Success();
            }

            if (!File.Exists(filePath))
            {
                _logger.Information("Файл уже отсутствует, удаление не требуется: {FilePath}", filePath);
                return ResultVoid.Success();
            }

            File.Delete(filePath);
            _logger.Information("Файл успешно удален: {FilePath}", filePath);
            return ResultVoid.Success();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Error(ex, "Отказано в доступе при удалении файла: {FilePath}", filePath);
            return ResultVoid.Failure(InfrastructureErrors.FileStorage.AccessDenied);
        }
        catch (IOException ex)
        {
            _logger.Error(ex, "Ошибка ввода-вывода при удалении файла: {FilePath}, Message={ErrorMessage}",
                filePath, ex.Message);
            return ResultVoid.Failure(InfrastructureErrors.FileStorage.IOError);
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Непредвиденная ошибка при удалении файла: {FilePath}", filePath);
            return ResultVoid.Failure(InfrastructureErrors.FileStorage.IOError);
        }
    }

    /// <summary>
    /// Проверяет существование файла в файловой системе.
    /// Метод никогда не возвращает провальный Result, только успешный Result с булевым значением.
    /// </summary>
    /// <param name="filePath">Путь к проверяемому файлу.</param>
    /// <returns>Всегда возвращает успешный Result, содержащий true, если файл существует, иначе false.</returns>
    public Result<bool> FileExists(string filePath)
    {

        try
        {

            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.Debug("Путь к файлу пуст, возвращаем false");
                return Result<bool>.Success(false);
            }

            var exists = File.Exists(filePath);
            _logger.Debug("Проверка существования файла: {FilePath}, Exists={Exists}", filePath, exists);
            return Result<bool>.Success(exists);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Исключение при проверке существования файла: {FilePath}. Возвращаем false.", filePath);
            return Result<bool>.Success(false);
        }
    }

    /// <summary>
    /// Нормализует и валидирует имя файла, преобразуя его в безопасный относительный путь.
    /// </summary>
    /// <param name="fileName">Имя файла для нормализации.</param>
    /// <returns>Результат с нормализованным путем или ошибкой валидации.</returns>
    private Result<string> NormalizeAndValidatePath(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return Result<string>.Failure(InfrastructureErrors.FileStorage.InvalidPath);
            }

            // Проверка на недопустимые символы в имени файла
            var invalidChars = Path.GetInvalidFileNameChars();
            if (fileName.IndexOfAny(invalidChars) >= 0)
            {
                _logger.Warning("Имя файла содержит недопустимые символы: {FileName}", fileName);
                return Result<string>.Failure(InfrastructureErrors.FileStorage.InvalidPath);
            }

            // Предотвращение попыток выхода за пределы базовой директории
            var normalizedPath = fileName.Replace('\\', '/').TrimStart('/');

            // Запрещаем использование ".." для навигации вверх
            if (normalizedPath.Contains(".."))
            {
                _logger.Warning("Попытка выхода за пределы базовой директории: {FileName}", fileName);
                return Result<string>.Failure(InfrastructureErrors.FileStorage.InvalidPath);
            }

            return Result<string>.Success(normalizedPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка нормализации пути: {FileName}", fileName);
            return Result<string>.Failure(InfrastructureErrors.FileStorage.InvalidPath);
        }
    }
}