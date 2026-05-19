using System;

using Dapper;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;
using PhotoCatalog.Infrastructure.UnitOfWork;

using ILogger = Serilog.ILogger;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <inheritdoc />
public class SqliteFolderQueryRepository : IFolderQueryRepository
{
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

    private readonly SqliteUnitOfWork _unitOfWork;
    private readonly ILogger<SqliteUnitOfWork> _logger;

    /// <inheritdoc />
    public Result<Folder> GetById(int id)
    {
        _unitOfWork.BeginTransaction();

        var result = _unitOfWork.Connection!
            .QueryFirstOrDefault<Folder>(
                """
                SELECT Id, ParentFolderId, Name
                FROM Folders
                WHERE Id = @Id
                """,
                new { Id = id })
            .ToResult()
            .Ensure(folder => (
                    folder is not null),
                InfrastructureErrors.Database.NotFound)
            .OnSuccess(_ => _unitOfWork.Commit())
            .OnFailure(_ =>
            {
                _logger.LogError("Ошибка SQLite при получении папки с Id = {{FolderId}}.");
                _unitOfWork.Rollback();
            });

        _unitOfWork.Dispose();

        return result!;
    }
}