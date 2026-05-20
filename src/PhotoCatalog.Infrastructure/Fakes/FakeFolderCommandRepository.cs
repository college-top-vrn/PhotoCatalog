using System.Collections.Concurrent;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <inheritdoc />
public class FakeFolderCommandRepository(
    ConcurrentDictionary<int, Folder> folders,
    FakeFolderQueryRepository fakeFolderQueryRepository) : IFolderCommandRepository
{
    /// <summary>
    ///     Идентификатор последнего элемента.
    /// </summary>
    private int _lastId;

    /// <inheritdoc />
    public ResultVoid Add(Folder folder)
    {
        _lastId += 1;

        folders.TryAdd(_lastId, folder);

        return ResultVoid.Success();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public ResultVoid Add(Folder folder, int id)
    {
        if (folders.TryAdd(id, folder).ToResult().IsFailure)
        {
            return ResultVoid
                .Failure(new Error("FolderRepository.FolderWithSameIdAlreadyExist",
                    "Папка с похожим идентификатором уже существует"));
        }

        return ResultVoid.Success();
    }

    /// <inheritdoc />
    public ResultVoid Update(Folder folder)
    {
        ResultVoid deleteResult = Delete(folder.Id);

        if (deleteResult.IsFailure)
        {
            return ResultVoid.Failure(deleteResult.Error);
        }

        Add(folder, folder.Id);

        return ResultVoid.Success();
    }

    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        Result<Folder> searchResult = fakeFolderQueryRepository.GetById(id);

        if (searchResult.IsFailure)
        {
            return ResultVoid.Failure(new Error("FolderRepository.CantDeleteFolder",
                "Не удалось удалить папку"));
        }

        folders.TryRemove(id, out _);

        return ResultVoid.Success();
    }
}