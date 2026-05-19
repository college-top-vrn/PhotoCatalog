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
public class SqliteFolderQueryRepository : IFolderQueryRepository
{
    private readonly SqliteUnitOfWork _unitOfWork;
    private readonly ILogger<SqliteUnitOfWork> _logger;

    /// <summary>
    ///     Создание экземпляра.
    /// </summary>
    /// <param name="connectionString">строка соединения.</param>
    /// <param name="logger">логгер.</param>
    public SqliteFolderQueryRepository(string connectionString, ILogger<SqliteUnitOfWork> logger)
    {
        SqliteConnectionStringBuilder builder = new() { DataSource = connectionString, Mode = SqliteOpenMode.ReadOnly };

        _unitOfWork = new SqliteUnitOfWork(builder.ToString(), logger);
        _logger = logger;
    }

    /// <inheritdoc />
    public Result<Folder> GetById(int id)
    {
        try
        {
            _unitOfWork.BeginTransaction();

            var foundFolder = _unitOfWork.Connection!
                .QueryFirstOrDefault<Folder>(
                    """
                    SELECT Id, ParentFolderId, Name
                    FROM Folders
                    WHERE Id = @Id
                    """,
                    new { Id = id });

            return foundFolder
                .ToResult(InfrastructureErrors.Database.NotFound)
                .Finally(
                    success: _ =>
                    {
                        _unitOfWork.Commit();
                        _unitOfWork.Dispose();
                        return Result<Folder>.Success(foundFolder);
                    },
                    failure: _ =>
                    {
                        _unitOfWork.Rollback();
                        _unitOfWork.Dispose();
                        return Result<Folder>.Failure(InfrastructureErrors.Database.NotFound);
                    });
        }
        catch (SqliteException)
        {
            _logger.LogError("Ошибка SQLite при получении папки с Id = {FolderId}", id);
            return Result<Folder>.Failure(InfrastructureErrors.Database.Sqlite);
        }
    }
}