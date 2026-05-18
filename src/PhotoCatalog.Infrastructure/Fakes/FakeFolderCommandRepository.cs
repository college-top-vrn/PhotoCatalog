using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <inheritdoc />
public class FakeFolderCommandRepository : IFolderCommandRepository
{
    /// <summary>
    ///     Словарь папок.
    /// </summary>
    private readonly ConcurrentDictionary<int, Folder> _folders = new();

    /// <summary>
    ///     Идентификатор последнего элемента.
    /// </summary>
    private int _lastId;
    
    /// <inheritdoc />
    public ResultVoid Add(Folder? folder)
    {
        if (folder is null)
        {
            return ResultVoid.Failure(new Error("FolderRepository.CantAddFolder",
                "Не удалось добавить папку"));
        }

        _lastId += 1;

        _folders.TryAdd(_lastId, folder);

        return ResultVoid.Success();
    }

    /// <inheritdoc />
    public ResultVoid Add(Folder? folder, int id)
    {
        if (folder is null)
        {
            return ResultVoid.Failure(new Error("FolderRepository.FolderIsNull",
                "Папка является null"));
        }

        if (_folders.TryAdd(id, folder).ToResult().IsFailure)
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
        if (folder is null)
        {
            return ResultVoid.Failure(new Error("FolderRepository.FolderIsNull",
                "Папка является null"));
        }

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
        Result<Folder> searchResult = GetById(id);

        if (searchResult.IsFailure)
        {
            return ResultVoid.Failure(new Error("FolderRepository.CantDeleteFolder",
                "Не удалось удалить папку"));
        }

        _folders.Remove(id, out _);

        return ResultVoid.Success();
    }
}