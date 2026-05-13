using System.Collections.Concurrent;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;

public class FakeFolderRepository : IFolderRepository
{
    private int _lastId = 0;
    private readonly ConcurrentDictionary<int, Folder> _data = new();

    public Result<Folder> GetById(int id)
    {
        foreach (var pair in _data)
        {
            if (pair.Key == id) return pair.Value.ToResult();
        }

        return Result<Folder>.Failure(new Error("FolderRepository.FolderNotFound",
            "Не удалось найти папку по идентификатору"));
    }

    public ResultVoid Add(Folder folder)
    {
        var result = _data
            .TryAdd(_lastId, folder)
            .ToResult();

        if (result.IsFailure) return ResultVoid.Failure(new Error("FolderRepository.CantAddFolder",
            "Не удалось добавить папку"));

        _lastId += 1;
        
        return ResultVoid.Success();
    }

    public ResultVoid Update(Folder folder)
    {
        var deleteResult = Delete(folder.Id);
        
        if (deleteResult.IsFailure) return ResultVoid.Failure(deleteResult.Error);
        
        var addResult = Add(folder);

        if (addResult.IsFailure) return ResultVoid.Failure(addResult.Error);

        return ResultVoid.Success();
    }

    public ResultVoid Delete(int id)
    {
        var result = _data
            .TryRemove(id, out var folder)
            .ToResult();

        if (result.IsFailure)
            return ResultVoid.Failure(new Error("FolderRepository.CantDeleteFolder",
                "Не удалось удалить папку"));

        return ResultVoid.Success();
    }
}