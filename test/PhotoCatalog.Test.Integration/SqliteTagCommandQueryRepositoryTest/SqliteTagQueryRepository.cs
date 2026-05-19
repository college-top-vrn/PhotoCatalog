using System;
using System.IO;

using Microsoft.Data.Sqlite;

using NSubstitute;

using PhotoCatalog.Infrastructure.Repositories;

using Serilog;

using Xunit;

namespace PhotoCatalog.Test.Integration.SqliteTagCommandQueryRepositoryTest;

public class SqliteTagQueryRepositoryTests: IDisposable
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
        var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SqliteTagCommandQueryRepositoryTest", "InitSchemaTest.sql");
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
            cmd.CommandText = "INSERT INTO Tags (Id, Name) VALUES (0, 'Лес')";
            cmd.ExecuteNonQuery();
        }
        
        var tag = _repo.GetById(0);

        Assert.NotNull(tag);
        Assert.Equal("Лес", tag.Value!.Name);
    }

    // [Fact]
    // public void GetById_missing_tag_returns_failure()
    // {
    //     var result = _fixture.QueryRepository.GetById(999);
    //
    //     Assert.True(result.IsFailure);
    // }
    //
    // [Fact]
    // public void GetByName_existing_tag_returns_success()
    // {
    //     var result = _fixture.QueryRepository.GetByName("City");
    //
    //     Assert.True(result.IsSuccess);
    //     Assert.Equal(2, result.Value.Id);
    //     Assert.Equal("City", result.Value.Name);
    // }
    //
    // [Fact]
    // public void GetByName_missing_tag_returns_failure()
    // {
    //     var result = _fixture.QueryRepository.GetByName("Unknown");
    //
    //     Assert.True(result.IsFailure);
    // }
    public void Dispose()
    {
        _keepAliveConnection?.Close();
        _keepAliveConnection?.Dispose();
    }
}