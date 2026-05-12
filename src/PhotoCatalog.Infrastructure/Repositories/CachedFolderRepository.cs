using Microsoft.Extensions.Caching.Hybrid;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <summary>
///     Декоратор репозитория папок, добавляющий кэширование операций чтения с помощью <see cref="HybridCache" />.
///     Оборачивает реальный репозиторий (<see cref="SqliteFolderRepository" />), перехватывая запросы на чтение
///     и инвалидируя кэш при успешных операциях изменения (Add, Update, Delete).
/// </summary>
public class CachedFolderRepository(IFolderRepository innerRepository, HybridCache cache) : IFolderRepository
{
    private readonly HybridCache _cache = cache;
    private readonly IFolderRepository _innerRepository = innerRepository;

    public Result<Folder> GetById(int id)
    {
        throw new NotImplementedException();
    }

    public Result<IEnumerable<Folder>> GetAllFolders()
    {
        throw new NotImplementedException();
    }

    public ResultVoid Add(Folder folder)
    {
        throw new NotImplementedException();
    }

    public ResultVoid Update(Folder folder)
    {
        throw new NotImplementedException();
    }

    public ResultVoid Delete(int id)
    {
        throw new NotImplementedException();
    }
}