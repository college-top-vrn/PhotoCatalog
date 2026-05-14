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
            if (pair.Key == id)
                return Result<Folder>.Success(pair.Value);

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
    public ResultVoid Add(Folder? folder)
    {
        if (folder is null)
            return ResultVoid.Failure(new Error("FolderRepository.CantAddFolder",
                "Не удалось добавить папку"));

        _lastId += 1;

        _folders.TryAdd(_lastId, folder);

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Добавление папки.
    /// </summary>
    /// <param name="folder">папка.</param>
    /// <param name="id">идентификатор папки.</param>
    /// <returns>
    ///     Возвращает значение успешного выполнения.
    ///     В противном случая вернётся отрицательный результат.
    /// </returns>
    public ResultVoid Add(Folder? folder, int id)
    {
        if (folder is null)
            return ResultVoid.Failure(new Error("FolderRepository.FolderIsNull",
                "Папка является null"));

        if (_folders.TryAdd(id, folder).ToResult().IsFailure)
            return ResultVoid
                .Failure(new Error("FolderRepository.FolderWithSameIdAlreadyExist",
                    "Папка с похожим идентификатором уже существует"));

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
    public ResultVoid Update(Folder? folder)
    {
        if (folder is null)
            return ResultVoid.Failure(new Error("FolderRepository.FolderIsNull",
                "Папка является null"));

        var deleteResult = Delete(folder.Id);

        if (deleteResult.IsFailure) return ResultVoid.Failure(deleteResult.Error);

        Add(folder, folder.Id);

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
        var searchResult = GetById(id);

        if (searchResult.IsFailure)
            return ResultVoid.Failure(new Error("FolderRepository.CantDeleteFolder",
                "Не удалось удалить папку"));

        _folders.Remove(id, out _);

        return ResultVoid.Success();
    }
}