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

/// <inheritdoc />
public class SqliteFolderQueryRepository : IFolderQueryRepository
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

    /// <inheritdoc />
    public Result<Folder> GetById(int id)
    {
        try
        {
            _unitOfWork.BeginTransaction();

            Folder? foundFolder = _unitOfWork.Connection!
                .QueryFirstOrDefault<Folder>(
                    """
                    SELECT Id, ParentFolderId, Name
                    FROM Folders
                    WHERE Id = @Id
                    """,
                    new { Id = id }
                );

            return foundFolder
                .ToResult(InfrastructureErrors.Database.NotFound)
                .Finally(
                    _ =>
                    {
                        _unitOfWork.Commit();
                        _unitOfWork.Dispose();
                        return Result<Folder>.Success(foundFolder);
                    },
                    _ =>
                    {
                        _unitOfWork.Rollback();
                        _unitOfWork.Dispose();
                        return Result<Folder>.Failure(InfrastructureErrors.Database.NotFound);
                    });
        }
        catch (SqliteException)
        {
            _logger.Error("Ошибка SQLite при получении папки с Id = {FolderId}", id);
            _unitOfWork.Rollback();
            _unitOfWork.Dispose();
            return Result<Folder>.Failure(InfrastructureErrors.Database.Sqlite);
        }
    }
}