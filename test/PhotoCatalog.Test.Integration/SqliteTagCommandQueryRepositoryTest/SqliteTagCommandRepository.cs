using System;
using System.IO;

using Microsoft.Data.Sqlite;

using NSubstitute;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Repositories;

using Serilog;

using Xunit;

namespace PhotoCatalog.Test.Integration.SqliteTagCommandQueryRepositoryTest;

public class SqliteTagCommandRepositoryTests : IDisposable
{
    private readonly string _connectionString;
    private readonly SqliteConnection _keepAliveConnection;
    private readonly ILogger _logger;
    private readonly SqliteTagCommandRepository _repoCommand;
    private readonly SqliteTagQueryRepository _repoQuery;

    public SqliteTagCommandRepositoryTests()
    {
        _logger = Substitute.For<ILogger>();

        _connectionString = $"DataSource=file:memdb_{Guid.NewGuid()}?mode=memory&cache=shared";

        _keepAliveConnection = new SqliteConnection(_connectionString);
        _keepAliveConnection.Open();
        string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TabelTest", "InitSchemaTest.sql");
        if (!File.Exists(scriptPath))
        {
            scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InitSchemaTest.sql");
        }

        string script = File.ReadAllText(scriptPath);
        using (SqliteCommand command = _keepAliveConnection.CreateCommand())
        {
            command.CommandText = script;
            command.ExecuteNonQuery();
        }

        _repoCommand = new SqliteTagCommandRepository(_connectionString, _logger);
        _repoQuery = new SqliteTagQueryRepository(_connectionString, _logger);
    }

    public void Dispose()
    {
        _keepAliveConnection.Close();
        _keepAliveConnection.Dispose();
    }

    [Fact]
    public void Add_new_unique_tag_returns_success_and_persists()
    {
        Result<Tag> tag = Tag.Create("лес");
        ResultVoid result = _repoCommand.Add(tag.Value!);
        Result<Tag> tagNew = _repoQuery.GetByName("лес");


        Assert.True(result.IsSuccess);
        Assert.True(tagNew.IsSuccess);
        Assert.Equal("лес", tagNew.Value!.Name);
    }

    [Fact]
    public void Add_duplicate_name_returns_failure()
    {
        Result<Tag> tag1 = Tag.Create("горы");
        Result<Tag> tag2 = Tag.Create("горы");


        ResultVoid firstResult = _repoCommand.Add(tag1.Value!);
        Assert.True(firstResult.IsSuccess);

        ResultVoid secondResult = _repoCommand.Add(tag2.Value!);

        Assert.True(secondResult.IsFailure);
    }

    [Fact]
    public void Delete_existing_free_tag_returns_success()
    {
        _repoCommand.Add(Tag.Create("лес").Value!);

        int tagId = _repoQuery.GetByName("лес").Value!.Id;

        ResultVoid deleteResult = _repoCommand.Delete(tagId);

        Assert.True(deleteResult.IsSuccess);

        Result<Tag> result = _repoQuery.GetById(tagId);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Delete_tag_used_by_photo_returns_failure()
    {
        int invalidId = 9999;
        ResultVoid deleteResult = _repoCommand.Delete(invalidId);


        Assert.True(deleteResult.IsFailure);
    }
}