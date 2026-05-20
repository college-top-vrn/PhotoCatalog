using System.Collections.Concurrent;
using System.Collections.Generic;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <inheritdoc />
public class FakeFolderQueryRepository(ConcurrentDictionary<int, Folder> folders) : IFolderQueryRepository
{
    /// <inheritdoc />
    public Result<Folder> GetById(int id)
    {
        foreach (KeyValuePair<int, Folder> pair in folders)
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