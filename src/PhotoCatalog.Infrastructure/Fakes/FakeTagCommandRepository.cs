using System.Collections.Concurrent;
using System.Collections.Generic;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <summary>
///     Репозиторий тегов, имитирующий БД, и хранящий данные в оперативной памяти.
///     Только для изменения.
/// </summary>
public class FakeTagCommandRepository: ITagCommandRepository
{
    /// <summary>
    ///     Словарь тегов.
    /// </summary>
    private readonly ConcurrentDictionary<int, Tag> _tags = new();

    /// <summary>
    ///     Идентификатор последнего элемента.
    /// </summary>
    private int _lastId;


    /// <inheritdoc />
    public ResultVoid Add(Tag tag)
    {
        _lastId += 1;
        
        if (_tags.TryAdd(_lastId, tag))
        {
            return ResultVoid.Success();
        }

        _lastId -= 1;
        return ResultVoid.Failure(InfrastructureErrors.Database.ConstraintViolation);

    }

    /// <inheritdoc />
    public ResultVoid Update(Tag tag)
    {
        if (!_tags.ContainsKey(tag.Id))
        {
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        
        _tags[tag.Id] = tag;
        
        return ResultVoid.Success();
    }

    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        if (!_tags.ContainsKey(id))
        {
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }

        _tags.Remove(id, out _);

        return ResultVoid.Success();
    }
}