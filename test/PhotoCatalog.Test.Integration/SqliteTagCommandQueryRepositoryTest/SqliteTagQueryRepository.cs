using System;
using System.Threading.Tasks;

using Dapper;

using Microsoft.Data.Sqlite;

using NSubstitute;

using PhotoCatalog.Infrastructure.Repositories;
using PhotoCatalog.Infrastructure.UnitOfWork;

using Serilog;

using Xunit;

namespace PhotoCatalog.Test.Integration.SqliteTagCommandQueryRepositoryTest;

public class SqliteTagQueryRepositoryTests: IClassFixture<SqliteTagFixture>
{
    private readonly SqliteTagFixture _fixture;

    public SqliteTagQueryRepositoryTests(SqliteTagFixture fixture)
    {
        _fixture = fixture;
    }
    
    
    [Fact]
    public void GetById_existing_tag_returns_success()
    {
        var result = _fixture.QueryRepository.GetById(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Id);
        Assert.Equal("Nature", result.Value.Name);
    }

    [Fact]
    public void GetById_missing_tag_returns_failure()
    {
        var result = _fixture.QueryRepository.GetById(999);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void GetByName_existing_tag_returns_success()
    {
        var result = _fixture.QueryRepository.GetByName("City");

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Id);
        Assert.Equal("City", result.Value.Name);
    }

    [Fact]
    public void GetByName_missing_tag_returns_failure()
    {
        var result = _fixture.QueryRepository.GetByName("Unknown");

        Assert.True(result.IsFailure);
    }
}