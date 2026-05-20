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
    private readonly SqliteConnection _keepAliveConnection;
    private readonly SqliteTagCommandRepository _repoCommand;
    private readonly SqliteTagQueryRepository _repoQuery;

    public SqliteTagCommandRepositoryTests()
    {
        ILogger logger = Substitute.For<ILogger>();

        string connectionString = $"DataSource=file:memdb_{Guid.NewGuid()}?mode=memory&cache=shared";

        _keepAliveConnection = new SqliteConnection(connectionString);
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

        _repoCommand = new SqliteTagCommandRepository(connectionString, logger);
        _repoQuery = new SqliteTagQueryRepository(connectionString, logger);
    }

    [Fact]
    public void AddNewUniqueTagReturnsSuccessAndPersists()
    {
        Result<Tag> tag = Tag.Create("лес");
        ResultVoid result = _repoCommand.Add(tag.Value!);
        Result<Tag> tagNew = _repoQuery.GetByName("лес");


        Assert.True(result.IsSuccess);
        Assert.True(tagNew.IsSuccess);
        Assert.Equal("лес", tagNew.Value!.Name);
    }

    [Fact]
    public void AddDuplicateNameReturnsFailure()
    {
        Result<Tag> tag1 = Tag.Create("горы");
        Result<Tag> tag2 = Tag.Create("горы");


        ResultVoid firstResult = _repoCommand.Add(tag1.Value!);
        Assert.True(firstResult.IsSuccess);

        ResultVoid secondResult = _repoCommand.Add(tag2.Value!);

        Assert.True(secondResult.IsFailure);
    }

    [Fact]
    public void DeleteExistingFreeTagReturnsSuccess()
    {
        _repoCommand.Add(Tag.Create("лес").Value!);

        int tagId = _repoQuery.GetByName("лес").Value!.Id;

        ResultVoid deleteResult = _repoCommand.Delete(tagId);

        Assert.True(deleteResult.IsSuccess);

        Result<Tag> result = _repoQuery.GetById(tagId);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void DeleteTagUsedByPhotoReturnsFailure()
    {
        const int invalidId = 9999;
        ResultVoid deleteResult = _repoCommand.Delete(invalidId);


        Assert.True(deleteResult.IsFailure);
    }

    [Fact]
    public void Update_existing_free_tag_returns_success_and_persists()
    {
        var tagOld = Tag.Create("лес");
        _repoCommand.Add(tagOld.Value!);


        var tagNew = Tag.Create("поляна");

        var result = _repoCommand.Update(tagNew.Value!);

        Assert.True(result.IsSuccess);

        Assert.Equal("поляна", tagNew.Value!.Name);
    }

    [Fact]
    public void Update_tag_returns_failure()
    {
        var tagNew = Tag.Create("поляна");

        var result = _repoCommand.Update(tagNew.Value!);
        Assert.True(result.IsFailure);
    }

    // TODO: Заменить вызовом GC.SuppressFinalize(object)
    public void Dispose()
    {
        _keepAliveConnection.Close();
        _keepAliveConnection.Dispose();
    }
}