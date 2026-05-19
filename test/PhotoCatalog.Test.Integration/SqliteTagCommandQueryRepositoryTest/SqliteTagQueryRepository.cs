using System;
using System.IO;

using Microsoft.Data.Sqlite;

using NSubstitute;

using PhotoCatalog.Infrastructure.Repositories;

using Serilog;

using Xunit;

namespace PhotoCatalog.Test.Integration.SqliteTagCommandQueryRepositoryTest;

public class SqliteTagQueryRepositoryTests : IDisposable
{
    private readonly SqliteConnection _keepAliveConnection;
    private readonly string _connectionString;
    private readonly ILogger _logger;
    private readonly SqliteTagQueryRepository _repo;

    public SqliteTagQueryRepositoryTests()
    {
        _logger = Substitute.For<ILogger>();

        _connectionString = $"DataSource=file:memdb_{Guid.NewGuid()}?mode=memory&cache=shared";

        _keepAliveConnection = new SqliteConnection(_connectionString);
        _keepAliveConnection.Open();
        var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TabelTest", "InitSchemaTest.sql");
        if (!File.Exists(scriptPath))
        {
            scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InitSchemaTest.sql");
        }

        var script = File.ReadAllText(scriptPath);
        using (var command = _keepAliveConnection.CreateCommand())
        {
            command.CommandText = script;
            command.ExecuteNonQuery();
        }

        _repo = new SqliteTagQueryRepository(_connectionString, _logger);
    }


    [Fact]
    public void GetById_existing_tag_returns_success()
    {
        using (var cmd = _keepAliveConnection.CreateCommand())
        {
            cmd.CommandText = "INSERT INTO Tags (Id, Name) VALUES (0, 'лес')";
            cmd.ExecuteNonQuery();
        }

        var tag = _repo.GetById(0);

        Assert.NotNull(tag);
        Assert.Equal("лес", tag.Value!.Name);
    }

    [Fact]
    public void GetById_missing_tag_returns_failure()
    {
        var tag = _repo.GetById(0);

        Assert.True(tag.IsFailure);
    }

    [Fact]
    public void GetByName_existing_tag_returns_success()
    {
        using (var cmd = _keepAliveConnection.CreateCommand())
        {
            cmd.CommandText = "INSERT INTO Tags (Id, Name) VALUES (0, 'лес')";
            cmd.ExecuteNonQuery();
        }

        var tag = _repo.GetByName("лес");

        Assert.True(tag.IsSuccess);
        Assert.Equal("лес", tag.Value!.Name);
    }

    [Fact]
    public void GetByName_missing_tag_returns_failure()
    {
        var tag = _repo.GetByName("лес");

        Assert.True(tag.IsFailure);
    }

    public void Dispose()
    {
        _keepAliveConnection.Close();
        _keepAliveConnection.Dispose();
    }
}