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

    public Result<IEnumerable<Folder>> GetAllFolders()
    {
        if (_folders.IsEmpty)
        {
            return Result<IEnumerable<Folder>>
                .Failure(new Error("FolderRepository.FoldersNotFound", "Не удалось получить все папки"));
        }

        IEnumerable<Folder> folders = _folders.Values.AsEnumerable();

        return Result<IEnumerable<Folder>>.Success(folders);
    }
}