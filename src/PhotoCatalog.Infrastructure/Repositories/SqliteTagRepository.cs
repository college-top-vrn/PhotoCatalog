using Dapper;

using Microsoft.Data.Sqlite;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Infrastructure.Repositories;

public class SqliteTagRepository : ITagRepository
{
    private readonly string _connectionString;

    public SqliteTagRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Result<Tag> GetById(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var teg = connection.QuerySingleOrDefault<Tag>(
            "SELECT * FROM Tags WHERE Id = @Id",
            new { Id = id });
        if (teg == null)
            return Result<Tag>.Failure(DomainErrors.Tag.EmptyName);

        return Result<Tag>.Success(teg);
    }

    public Result<Tag> GetByName(string name)
    {
        throw new NotImplementedException();
    }

    public ResultVoid Add(Tag tag)
    {
        throw new NotImplementedException();
    }

    public Result<Tag> Delete(int id)
    {
        throw new NotImplementedException();
    }
}