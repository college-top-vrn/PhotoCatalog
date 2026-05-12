using System.Collections.Generic;
using System.Linq;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <summary>
///     Фальшивый репозиторий папок для врменной работы с frontend
/// </summary>
public class FakeFolderRepository : IFolderRepository
{
    private readonly Dictionary<int, Folder> _storage = new();


    /// <inheritdoc/>
    public Result<Folder> GetById(int id)
    {
        if (_storage.TryGetValue(id, out var photo))
        {
            return Result<Folder>.Success(photo.DeepCopy());
        }

        return Result<Folder>.Failure(new Error("FakeFolder.NotFound", "Папка не найдена"));
    }

    /// <inheritdoc />
    public Result<IEnumerable<Folder>> GetAllFolders()
    {
        var copies = _storage.Values.Select(f => f.DeepCopy()).ToList();
        return Result<IEnumerable<Folder>>.Success(copies);
    }

    /// <inheritdoc />
    public ResultVoid Add(Folder folder)
    {
        var folderCopy = folder.DeepCopy();
        if (_storage.TryAdd(folder.Id, folderCopy))
        {
            return ResultVoid.Success();
        }

        return ResultVoid.Failure(new Error("FakeFolder.FailedToAdd", "Не удалось добавить"));
    }


    /// <inheritdoc />
    public ResultVoid Update(Folder folder)
    {
        if (_storage.ContainsKey(folder.Id))
        {
            var folderCopy = folder.DeepCopy();
            _storage[folder.Id] = folderCopy;
            return ResultVoid.Success();
        }

        return ResultVoid.Failure(new Error("FakeFolder.FailedUpdate", "Не удалось обновить "));
    }

    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        if (_storage.ContainsKey(id))
        {
            _storage.Remove(id);
            return ResultVoid.Success();
        }

        return ResultVoid.Failure(new Error("FakeFolder.FailedRemove", "Не удалось удалить"));
    }
}