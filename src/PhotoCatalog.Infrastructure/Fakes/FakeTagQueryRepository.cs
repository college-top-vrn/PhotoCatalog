using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <summary>
///     Репозиторий тегов, имитирующий БД, и хранящий данные в оперативной памяти.
///     Только для получения.
/// </summary>
public class FakeTagQueryRepository : ITagQueryRepository
{
    /// <summary>
    ///     Словарь тегов.
    /// </summary>
    private readonly IReadOnlyDictionary<int, Tag> _tags = new ConcurrentDictionary<int, Tag>();

    /// <inheritdoc />
    public Result<Tag> GetById(int id)
    {
        foreach (KeyValuePair<int, Tag> pair in _tags)
        {
            if (pair.Key == id)
            {
                return Result.Success(pair.Value);
            }
        }

        return Result.Failure<Tag>(InfrastructureErrors.Database.NotFound);
    }

    /// <inheritdoc />
    public Result<IEnumerable<Tag>> GetAll()
    {
        List<Tag> result = [];
        result.AddRange(_tags.Select(pair => pair.Value));

        return result.Count == 0
            ? Result.Failure<IEnumerable<Tag>>(InfrastructureErrors.Database.NotFound)
            : Result.Success<IEnumerable<Tag>>(result);
    }

    /// <inheritdoc />
    public Result<Tag> GetByName(string name)
    {
        foreach (Tag pair in _tags.Values)
        {
            if (pair.Name == name)
            {
                return Result.Success(pair);
            }
        }

        return Result.Failure<Tag>(InfrastructureErrors.Database.NotFound);
    }
}