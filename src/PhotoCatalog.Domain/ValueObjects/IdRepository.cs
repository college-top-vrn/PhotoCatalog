using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.ValueObjects;


/// <summary>
///     ValueObject, представляющий собой репозиторий идентификаторов.
/// </summary>
public sealed class IdRepository
{
    private readonly List<Guid> _idRepository;

    /// <summary>
    ///     Иммутабельный список идентификаторов.
    /// </summary>
    public IImmutableList<Guid> Ids => _idRepository.ToImmutableList();
    
    private IdRepository(List<Guid> idRepository) => _idRepository = idRepository;
    
    /// <summary>
    ///     Создаёт репозиторий идентификаторов.
    /// </summary>
    /// <param name="ids">список идентификаторов.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех с экземпляром репозитория.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static Result<IdRepository> Create(List<Guid> ids)
    {
        IdRepository idRepository = new(ids);

        return Result.Success(idRepository);
    }
    
    /// <summary>
    ///     Добавляет идентификатор в список.
    /// </summary>
    /// <param name="id">идентификатор.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.IdRepository.DuplicatedId"/>, если данный тег уже привязан.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid Add(Guid id)
    {
        if (_idRepository.Contains(id))
        {
            return ResultVoid.Failure(DomainErrors.IdRepository.DuplicatedId);
        }
        
        _idRepository.Add(id);
        return ResultVoid.Success();
    }

    /// <summary>
    ///     Удаляет идентификатор из списка.
    /// </summary>
    /// <param name="id">идентификатор.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Успех;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Ошибка <see cref="DomainErrors.IdRepository.IdNotFound"/>, если тег не найден.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public ResultVoid Remove(Guid id)
    {
        return _idRepository.Remove(id)
            ? ResultVoid.Success()
            : ResultVoid.Failure(DomainErrors.IdRepository.IdNotFound);
    }
}