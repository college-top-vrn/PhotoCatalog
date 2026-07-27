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

/// <inheritdoc cref="IFolderQueryRepository" />
public class SqliteFolderQueryRepository : IFolderQueryRepository, IDisposable
{
    private readonly ILogger _logger;
    private readonly SqliteUnitOfWork _unitOfWork;

    /// <summary>
    ///     Создание экземпляра.
    /// </summary>
    /// <param name="connectionString">строка соединения.</param>
    /// <param name="logger">логгер.</param>
    public SqliteFolderQueryRepository(string connectionString, ILogger logger)
    {
        SqliteConnectionStringBuilder builder = new() { DataSource = connectionString, Mode = SqliteOpenMode.ReadOnly };

        _unitOfWork = new SqliteUnitOfWork(builder.ToString(), logger);
        _logger = logger;
    }

    // TODO: Заменить вызовом GC.SuppressFinalize(object)
    /// <inheritdoc />
    public void Dispose()
    {
        _unitOfWork.Dispose();
    }

    /// <inheritdoc />
    public Result<Folder> GetById(int id)
    {
        try
        {
            _unitOfWork.BeginTransaction();

            if (_unitOfWork.Connection == null)
            {
                return Result.Failure<Folder>(InfrastructureErrors.Database.NotFound);
            }

            Folder? foundFolder = _unitOfWork.Connection
                .QueryFirstOrDefault<Folder>(
                    """
                    SELECT Id, ParentFolderId, Name
                    FROM Folders
                    WHERE Id = @Id
                    """,
                    new { Id = id }
                );

            if (foundFolder == null)
            {
                return Result.Failure<Folder>(InfrastructureErrors.Database.NotFound);
            }

            return foundFolder
                .ToResult(InfrastructureErrors.Database.NotFound)
                .Finally(
                    _ =>
                    {
                        _unitOfWork.Commit();
                        return Result.Success(foundFolder);
                    },
                    _ =>
                    {
                        _unitOfWork.Rollback();
                        return Result.Failure<Folder>(InfrastructureErrors.Database.NotFound);
                    });
        }
        catch (SqliteException)
        {
            _logger.Error("Ошибка SQLite при получении папки с Id = {FolderId}", id);
            _unitOfWork.Rollback();
            return Result.Failure<Folder>(InfrastructureErrors.Database.Postgres);
        }
    }
}