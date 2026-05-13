using System.Reflection;

using Microsoft.Extensions.Caching.Hybrid;

using PhotoCatalog.Application.Caching;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

using Serilog;

namespace PhotoCatalog.Infrastructure.Repositories;


/// <summary>
///     Декоратор репозитория папок, добавляющий кэширование операций чтения с помощью <see cref="HybridCache" />.
///     Оборачивает реальный репозиторий (<see cref="SqliteFolderRepository" />), перехватывая запросы на чтение
///     и инвалидируя кэш при успешных операциях изменения (Add, Update, Delete).
///     Ошибки, возвращённые внутренним репозиторием, не попадают в кэш благодаря выбрасыванию <see cref="CacheBypassException"/>.
/// </summary>
/// <param name="innerRepository">Оригинальный репозиторий, выполняющий реальные запросы к базе данных.</param>
/// <param name="cache">Сервис гибридного кэширования.</param>
/// <param name="logger">Логгер для записи событий работы декоратора.</param>
public class CachedFolderRepository(IFolderRepository innerRepository, HybridCache cache, ILogger logger) : IFolderRepository
{
    /// <inheritdoc />
    /// <remarks>
    ///     Данные папки кэшируются с ключом <see cref="CacheKeysFactory.GetFolderKey"/> и тэгами
    ///     <see cref="CacheKeysFactory.GetFolderTag"/> и <see cref="CacheKeysFactory.GetFoldersTreeTag"/>.
    ///     При сбое базы данных кэш не обновляется.
    /// </remarks>
    public Result<Folder> GetById(int id)
    {
        try
        {
            Folder? cachedFolder = cache.GetOrCreateAsync<Folder?>(
                CacheKeysFactory.GetFolderKey(id),
                _ => GetFolderByIdValueTask(id),
                options: null,
                tags: [CacheKeysFactory.GetFolderTag(id), CacheKeysFactory.GetFoldersTreeTag()]
            ).AsTask().GetAwaiter().GetResult();
            
            return Result<Folder>.Success(cachedFolder!);
        }
        catch (CacheBypassException ex)
        {
            logger.Warning(ex, "Не удалось получить папку с Id={FolderId} из внутреннего репозитория – результат не кэширован", id);
            return Result<Folder>.Failure(InfrastructureErrors.Database.Sqlite);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Непредвиденная ошибка при получении папки с Id={FolderId} из кэша", id);
            return Result<Folder>.Failure(InfrastructureErrors.Cache.UnknownError);
        }
    }

    /// <inheritdoc />
    public Result<IEnumerable<Folder>> GetAllFolders()
    {
        try
        {
            IEnumerable<Folder>? folders = cache.GetOrCreateAsync<IEnumerable<Folder>?>(
                CacheKeysFactory.GetFoldersTreeKey(),
                _ => GetAllFoldersValueTask(),
                options: null,
                tags: [CacheKeysFactory.GetFoldersTreeTag()]
            ).AsTask().GetAwaiter().GetResult();

            return Result<IEnumerable<Folder>>.Success(folders!);
        }
        catch (CacheBypassException ex)
        {
            logger.Warning(ex, "Не удалось получить список папок из внутреннего репозитория – результат не кэширован");
            return Result<IEnumerable<Folder>>.Failure(InfrastructureErrors.Database.Sqlite);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Непредвиденная ошибка при получении списка папок из кэша");
            return Result<IEnumerable<Folder>>.Failure(InfrastructureErrors.Cache.UnknownError);
        }
    }

    /// <inheritdoc />
    public ResultVoid Add(Folder folder) => UpdateAndInvalidate(() => innerRepository.Add(folder), folder.Id);

    /// <inheritdoc />
    public ResultVoid Update(Folder folder) => UpdateAndInvalidate(() => innerRepository.Update(folder), folder.Id);

    /// <inheritdoc />
    public ResultVoid Delete(int id) => UpdateAndInvalidate(() => innerRepository.Delete(id), id);

    /// <summary>
    ///     Выполняет операцию изменения через внутренний репозиторий и,
    ///     в случае успеха, инвалидирует кэш дерева папок по тегу <see cref="CacheKeysFactory.GetFoldersTreeTag"/>.
    /// </summary>
    /// <param name="operation">Функция, выполняющая операцию изменения (Add, Update, Delete) и возвращающая <see cref="ResultVoid"/>.</param>
    /// <param name="folderId">Идентификатор папки, над которой выполняется операция (для логирования).</param>
    /// <returns>Результат операции, полученный от внутреннего репозитория.</returns>
    private ResultVoid UpdateAndInvalidate(Func<ResultVoid> operation, int folderId)
    {
        try
        {
            ResultVoid result = operation();
            if (result.IsSuccess)
            {
                logger.Information(
                    "Операция {Operation} папки с Id={FolderId} выполнена успешно, инвалидация кэша дерева папок",
                    operation.GetMethodInfo().Name,
                    folderId);
                cache.RemoveByTagAsync(CacheKeysFactory.GetFoldersTreeTag()).AsTask().GetAwaiter().GetResult();
            }
            else
            {
                logger.Warning(
                    "Операция {Operation} папки с Id={FolderId} завершилась ошибкой: {ErrorCode} {ErrorMessage}",
                    operation.GetMethodInfo().Name, folderId, result.Error.Code, result.Error.Message);   
            }
            return result;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Исключение при выполнении операции {Operation} папки с Id={FolderId}",
                operation.GetMethodInfo().Name, folderId);
            return ResultVoid.Failure(InfrastructureErrors.Database.Sqlite);
        }
    }

    /// <summary>
    ///     Синхронно получает папку и оборачивает результат в ValueTask.
    /// </summary>
    private ValueTask<Folder?> GetFolderByIdValueTask(int id)
    {
        Result<Folder> result = innerRepository.GetById(id);
        if (!result.IsFailure)
        {
            return new ValueTask<Folder?>(result.Value);
        }

        logger.Warning("Папка с Id={FolderId} не получена: {ErrorCode} {ErrorMessage}",
            id, result.Error.Code, result.Error.Message);
        throw new CacheBypassException($"Папка с Id={id} не найдена или ошибка БД.");
    }

    /// <summary>
    ///     Синхронно получает все папки и оборачивает результат в ValueTask.
    /// </summary>
    private ValueTask<IEnumerable<Folder>?> GetAllFoldersValueTask()
    {
        Result<IEnumerable<Folder>> result = innerRepository.GetAllFolders();
        if (!result.IsFailure)
        {
            return new ValueTask<IEnumerable<Folder>?>(result.Value);
        }

        logger.Warning("Не удалось получить список всех папок: {ErrorCode} {ErrorMessage}",
            result.Error.Code, result.Error.Message);
        throw new CacheBypassException("Не удалось получить список всех папок.");
    }
}