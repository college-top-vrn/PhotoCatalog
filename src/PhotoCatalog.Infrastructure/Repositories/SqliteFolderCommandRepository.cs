using System;

using Dapper;

using Microsoft.Data.Sqlite;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;
using PhotoCatalog.Infrastructure.UnitOfWork;

using Serilog;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <inheritdoc cref="IFolderCommandRepository" />
public class SqliteFolderCommandRepository : IFolderCommandRepository, IDisposable
{
    private readonly ILogger _logger;
    private readonly SqliteUnitOfWork _unitOfWork;

    private SqliteFolderCommandRepository(string connectionString, ILogger logger)
    {
        _unitOfWork = new SqliteUnitOfWork(connectionString, logger);
        _logger = logger;
    }

    // TODO: Заменить вызовом GC.SuppressFinalize(object)
    /// <inheritdoc />
    public void Dispose()
    {
        _unitOfWork.Dispose();
    }

    /// <inheritdoc />
    public ResultVoid Add(Folder folder)
    {
        try
        {
            _unitOfWork.BeginTransaction();

            if (_unitOfWork.Connection != null)
            {
                return _unitOfWork.Connection
                    .Execute(
                        """
                        INSERT INTO Folders (Id, ParentFolderId, Name)
                        VALUES (@Id, @ParentFolderId, @Name)
                        """,
                        new { folder.Id, folder.ParentFolderId, folder.Name })
                    .ToResult()
                    .Finally(
                        _ =>
                        {
                            _unitOfWork.Commit();
                            _logger.Information("Папка с Id = {FolderId} успешно добавлена", folder.Id);
                            return ResultVoid.Success();
                        },
                        _ =>
                        {
                            _unitOfWork.Rollback();
                            return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
                        });
            }

            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (SqliteException)
        {
            _logger.Error("Ошибка SQLite при добавлении папки с Id = {FolderId}", folder.Id);
            _unitOfWork.Rollback();
            return ResultVoid.Failure(InfrastructureErrors.Database.Postgres);
        }
    }

    /// <inheritdoc />
    public ResultVoid Update(Folder folder)
    {
        try
        {
            _unitOfWork.BeginTransaction();

            if (_unitOfWork.Connection != null)
            {
                return _unitOfWork.Connection
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
                        _ =>
                        {
                            _unitOfWork.Commit();
                            _logger.Information("Папка с Id = {FolderId} успешно обновлена", folder.Id);
                            return ResultVoid.Success();
                        },
                        _ =>
                        {
                            _unitOfWork.Rollback();
                            return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
                        });
            }

            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (SqliteException)
        {
            _logger.Error("Ошибка SQLite при обновлении папки с Id = {FolderId}", folder.Id);
            _unitOfWork.Rollback();
            return ResultVoid.Failure(InfrastructureErrors.Database.Postgres);
        }
    }

    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        try
        {
            _unitOfWork.BeginTransaction();

            if (_unitOfWork.Connection != null)
            {
                return _unitOfWork.Connection
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
                        _ =>
                        {
                            _unitOfWork.Commit();
                            _logger.Information("Папка с Id = {FolderId} успешно удалена", id);
                            return ResultVoid.Success();
                        },
                        _ =>
                        {
                            _unitOfWork.Rollback();
                            return ResultVoid.Failure(InfrastructureErrors.Database.NotFound);
                        });
            }

            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (SqliteException)
        {
            _logger.Error("Ошибка SQLite при удалении папки с Id = {FolderId}", id);
            _unitOfWork.Rollback();
            return ResultVoid.Failure(InfrastructureErrors.Database.Postgres);
        }
    }
}