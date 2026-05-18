using Dapper;

using Microsoft.Data.Sqlite;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;

using Serilog;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <inheritdoc />
public class SqliteFolderQueryRepository(string connectionString, ILogger logger) : IFolderQueryRepository
{
    /// <inheritdoc />
    public Result<Folder> GetById(int id)
    {
        try
        {
            using SqliteConnection connection = new(connectionString);
            connection.Open();
            const string sql = "SELECT Id, ParentFolderId, Name FROM Folders WHERE Id = @Id";
            Folder? folder = connection.QueryFirstOrDefault<Folder>(sql, new { Id = id });

            return folder is null
                ? Result<Folder>.Failure(InfrastructureErrors.Database.NotFound)
                : Result<Folder>.Success(folder);
        }
        catch (SqliteException ex)
        {
            logger.Error(ex, "Ошибка SQLite при получении папки с Id = {FolderId}.", id);
            return Result<Folder>.Failure(InfrastructureErrors.Database.Sqlite);
        }
    }
}