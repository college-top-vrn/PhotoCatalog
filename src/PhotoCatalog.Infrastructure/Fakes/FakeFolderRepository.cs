using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <summary>
///     Фальшивый репозиторий для врменной работы с frontend
/// </summary>
public class FakeFolderRepository : IFolderRepository
{
    private readonly Dictionary<int, Folder> _storage;


    /// <inheritdoc/>
    public Result<Folder> GetById(int id)
    {
        if (_storage.TryGetValue(id, out var photo))
        {
            return Result<Folder>.Success(photo);
        }

        return Result<Folder>.Failure(new Error("FakeFolder.NotFound", "Папка не найдена"));
    }

    /// <inheritdoc />
    public Result<IEnumerable<Folder>> GetAllFolders()
    {
        return Result<IEnumerable<Folder>>.Success(_storage.Values.ToList());
    }

    /// <inheritdoc />
    public ResultVoid Add(Folder folder)
    {
        if (_storage.TryAdd(folder.Id, folder))
        {
            return ResultVoid.Success();
        }

        return ResultVoid.Failure(new Error("FakeFolder.FailedToAdd", "Не удалось добавить"));
    }


    /// <inheritdoc />
    public ResultVoid Update(Folder folder)
    {
        var foundElement = _storage.Values.FirstOrDefault(e => folder.Id == e.Id);

        if (foundElement is not null)
        {
            _storage.Remove(foundElement.Id);
            _storage.Add(folder.Id, folder);
            return ResultVoid.Success();
        }

        return ResultVoid.Failure(new Error("FakeFolder.FailedUpdate", "Не удалось обновить "));
    }

    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        var foundElement = _storage.Values.FirstOrDefault(e => e.Id == id);

        if (foundElement is not null)
        {
            _storage.Remove(foundElement.Id);
            return ResultVoid.Success();
        }

        return ResultVoid.Failure(new Error("FakeFolder.FailedRemove", "Не удалось удалить"));
    }
}