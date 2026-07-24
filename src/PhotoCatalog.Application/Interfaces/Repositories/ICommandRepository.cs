using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Application.Interfaces.Repositories;

public interface ICommandRepository<in T>
{
    ResultVoid Add(T data);

    ResultVoid Update(T data);

    ResultVoid Delete(T data);
}