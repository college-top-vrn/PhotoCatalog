using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <inheritdoc />
public class FakeFolderQueryRepository : IFolderQueryRepository
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
    public Result<Folder> GetById(int id)
    {
        foreach (KeyValuePair<int, Folder> pair in _folders)
        {
            if (pair.Key == id)
            {
                return Result<Folder>.Success(pair.Value);
            }
        }

        return Result<Folder>.Failure(new Error("FolderRepository.FolderNotFound",
            "Не удалось найти папку по идентификатору"));
    }
}