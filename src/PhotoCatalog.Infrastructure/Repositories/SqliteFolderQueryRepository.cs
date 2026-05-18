using System;

using Dapper;

using Microsoft.Data.Sqlite;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

using Serilog;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <inheritdoc />
public class SqliteFolderQueryRepository : IFolderQueryRepository
{
    /// <summary>
    ///     Создание экземпляра.
    /// </summary>
    /// <param name="connectionString">строка соединения.</param>
    /// <param name="logger">логгер.</param>
    public SqliteFolderQueryRepository(string connectionString, ILogger logger)
    {
        SqliteConnectionStringBuilder builder = new() { DataSource = connectionString, Mode = SqliteOpenMode.ReadOnly };

        _connectionString = builder.ToString();
        _logger = logger;
    }

    private readonly string _connectionString;

    private readonly ILogger _logger;

    /// <inheritdoc />
    public Result<Folder> GetById(int id)
    {
        try
        {
            using SqliteConnection connection = new(_connectionString);

            connection.Open();

            const string sql = """
                               SELECT Id, ParentFolderId, Name
                               FROM Folders
                               WHERE Id = @Id
                               """;

            Folder? folder = connection.QueryFirstOrDefault<Folder>(sql, new { Id = id });

            return folder is null
                ? Result<Folder>.Failure(InfrastructureErrors.Database.NotFound)
                : Result<Folder>.Success(folder);
        }
        catch (SqliteException exception)
        {
            _logger.Error(exception, "Ошибка SQLite при получении папки с Id = {FolderId}.", id);
            return Result<Folder>.Failure(InfrastructureErrors.Database.Sqlite);
        }
    }
}