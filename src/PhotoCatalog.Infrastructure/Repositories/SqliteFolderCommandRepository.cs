using Dapper;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;
using PhotoCatalog.Infrastructure.UnitOfWork;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <inheritdoc />
public class SqliteFolderCommandRepository : IFolderCommandRepository
{
    private readonly SqliteUnitOfWork _unitOfWork;
    private readonly ILogger<SqliteUnitOfWork> _logger;

    SqliteFolderCommandRepository(string connectionString, ILogger<SqliteUnitOfWork> logger)
    {
        _unitOfWork = new SqliteUnitOfWork(connectionString, logger);
        _logger = logger;
    }

    /// <inheritdoc />
    public ResultVoid Add(Folder folder)
    {
        try
        {
            _unitOfWork.BeginTransaction();

            return _unitOfWork.Connection!
                .Execute(
                    """
                    INSERT INTO Folders (Id, ParentFolderId, Name)
                    VALUES (@Id, @ParentFolderId, @Name)
                    """,
                    new { folder.Id, folder.ParentFolderId, folder.Name })
                .ToResult()
                .Finally(
                    success: _ =>
                    {
                        _unitOfWork.Commit();
                        _logger.LogInformation("Папка с Id = {FolderId} успешно добавлена", folder.Id);
                        _unitOfWork.Dispose();
                        return ResultVoid.Success();
                    },
                    failure: _ =>
                    {
                        _unitOfWork.Rollback();
                        _unitOfWork.Dispose();
                        return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
                    });
        }
        catch (SqliteException)
        {
            _logger.LogError("Ошибка SQLite при добавлении папки с Id = {FolderId}", folder.Id);
            _unitOfWork.Rollback();
            _unitOfWork.Dispose();
            return ResultVoid.Failure(InfrastructureErrors.Database.Sqlite);
        }
    }

    /// <inheritdoc />
    public ResultVoid Update(Folder folder)
    {
        try
        {
            _unitOfWork.BeginTransaction();

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
                .Finally(
                    success: _ =>
                    {
                        _unitOfWork.Commit();
                        _logger.LogInformation("Папка с Id = {FolderId} успешно обновлена", folder.Id);
                        _unitOfWork.Dispose();
                        return ResultVoid.Success();
                    },
                    failure: _ =>
                    {
                        _unitOfWork.Rollback();
                        _unitOfWork.Dispose();
                        return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
                    });
        }
        catch (SqliteException)
        {
            _logger.LogError("Ошибка SQLite при обновлении папки с Id = {FolderId}", folder.Id);
            _unitOfWork.Rollback();
            _unitOfWork.Dispose();
            return ResultVoid.Failure(InfrastructureErrors.Database.Sqlite);
        }
    }

    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        try
        {
            _unitOfWork.BeginTransaction();

            return _unitOfWork.Connection!
                .Execute(
                    """
                    DELETE FROM Folders
                    WHERE Id = @Id
                    """,
                    new { Id = id }
                )
                .ToResult()
                .Check(affectedRows => (affectedRows != 0).ToResult())
                .Finally(
                    success: _ =>
                    {
                        _unitOfWork.Commit();
                        _logger.LogInformation("Папка с Id = {FolderId} успешно удалена", id);
                        _unitOfWork.Dispose();
                        return ResultVoid.Success();
                    },
                    failure: _ =>
                    {
                        _unitOfWork.Rollback();
                        _unitOfWork.Dispose();
                        return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
                    });
        }
        catch (SqliteException)
        {
            _logger.LogError("Ошибка SQLite при удалении папки с Id = {FolderId}", id);
            _unitOfWork.Rollback();
            _unitOfWork.Dispose();
            return ResultVoid.Failure(InfrastructureErrors.Database.Sqlite);
        }
    }
}