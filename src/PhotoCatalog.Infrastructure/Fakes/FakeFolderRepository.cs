using System.Collections.Concurrent;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <summary>
///     Репозиторий папок, имитирующий БД, и хранящий данные в оперативной памяти.
/// </summary>
public class FakeFolderRepository : IFolderRepository
{
    /// <summary>
    ///     Идентификатор последнего элемента.
    /// </summary>
    private int _lastId;
    
    /// <summary>
    ///     Словарь папок.
    /// </summary>
    private ConcurrentDictionary<int, Folder> _folders = new();

    /// <summary>
    ///     Получение папки по идентификатору.
    /// </summary>
    /// <param name="id">идентификатор папки.</param>
    /// <returns>Папку.</returns>
    public Result<Folder> GetById(int id)
    {
        foreach (var pair in _folders)
        {
            if (pair.Key == id) return pair.Value.ToResult();
        }

        return Result<Folder>.Failure(new Error("FolderRepository.FolderNotFound",
            "Не удалось найти папку по идентификатору"));
    }

    /// <summary>
    ///     Добавление папки.
    /// </summary>
    /// <param name="folder">папка.</param>
    /// <returns>
    ///     Возвращает значение успешного выполнения.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public ResultVoid Add(Folder folder)
    {
        var result = _folders
            .TryAdd(_lastId, folder)
            .ToResult();

        if (result.IsFailure) return ResultVoid.Failure(new Error("FolderRepository.CantAddFolder",
            "Не удалось добавить папку"));

        _lastId += 1;
        
        return ResultVoid.Success();
    }

    /// <summary>
    ///     Обновление папки.
    /// </summary>
    /// <param name="folder">папка.</param>
    /// <returns>
    ///     Возвращает значение успешного выполнения.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public ResultVoid Update(Folder folder)
    {
        var deleteResult = Delete(folder.Id);
        
        if (deleteResult.IsFailure) return ResultVoid.Failure(deleteResult.Error);
        
        var addResult = Add(folder);

        if (addResult.IsFailure) return ResultVoid.Failure(addResult.Error);

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Удаление папки.
    /// </summary>
    /// <param name="id">идентификатор папки.</param>
    /// <returns>
    ///     Возвращает значение успешного выполнения.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public ResultVoid Delete(int id)
    {
        var result = _folders
            .TryRemove(id, out var folder)
            .ToResult();

        if (result.IsFailure)
            return ResultVoid.Failure(new Error("FolderRepository.CantDeleteFolder",
                "Не удалось удалить папку"));

        return ResultVoid.Success();
    }
}