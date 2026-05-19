using Dapper;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;
using PhotoCatalog.Infrastructure.UnitOfWork;

using Serilog;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <inheritdoc />
public class SqliteFolderCommandRepository : IFolderCommandRepository
{
    SqliteFolderCommandRepository(string connectionString, ILogger<SqliteUnitOfWork> logger)
    {
        _unitOfWork = new SqliteUnitOfWork(connectionString, logger);
        _logger = logger;
    }

    private readonly SqliteUnitOfWork _unitOfWork;
    private readonly ILogger<SqliteUnitOfWork> _logger;

    /// <inheritdoc />
    public ResultVoid Add(Folder folder)
    {
        return _unitOfWork.Connection!
            .Execute(
                """
                INSERT INTO Folders (Id, ParentFolderId, Name)
                VALUES (@Id, @ParentFolderId, @Name)
                """,
                new { folder.Id, folder.ParentFolderId, folder.Name })
            .ToResult()
            .OnSuccess(_ => _unitOfWork.Commit())
            .OnFailure(_ => _unitOfWork.Rollback());
    }

    /// <inheritdoc />
    public ResultVoid Update(Folder folder)
    {
        return _unitOfWork.Connection!
            .Execute(
                """
                UPDATE Folders
                SET ParentFolderId = @ParentFolderId,
                Name = @Name
                WHERE Id = @Id
                """,
                new { folder.Id, folder.ParentFolderId, folder.Name }
            )
            .ToResult()
            .Check(affectedRows => (affectedRows != 0).ToResult())
            .OnSuccess(_ => _unitOfWork.Commit())
            .OnFailure(_ => _unitOfWork.Rollback());
    }

    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        return _unitOfWork.Connection!
            .Execute("DELETE FROM Folders WHERE Id = @Id",
                new { Id = id })
            .ToResult()
            .Check(affectedRows => (affectedRows != 0).ToResult())
            .OnSuccess(_ => _unitOfWork.Commit())
            .OnFailure(_ => _unitOfWork.Rollback());
    }
}