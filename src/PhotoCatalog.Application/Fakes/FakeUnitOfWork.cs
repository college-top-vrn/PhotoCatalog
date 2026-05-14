using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Application.Fakes;

/// <summary>
///     Заглушка Unit of Work для тестирования без реальной БД.
/// </summary>
public class FakeUnitOfWork : IUnitOfWork
{
    /// <inheritdoc />
    public void Dispose()
    {
    }

    /// <inheritdoc />
    public ResultVoid BeginTransaction()
    {
        return ResultVoid.Success();
    }

    /// <inheritdoc />
    public ResultVoid Commit()
    {
        return ResultVoid.Success();
    }

    /// <inheritdoc />
    public ResultVoid Rollback()
    {
        return ResultVoid.Success();
    }
}