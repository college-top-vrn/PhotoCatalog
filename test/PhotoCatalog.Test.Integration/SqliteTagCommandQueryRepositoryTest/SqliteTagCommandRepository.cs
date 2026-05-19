using Dapper;

using PhotoCatalog.Domain.Entities;

using Xunit;

namespace PhotoCatalog.Test.Integration.SqliteTagCommandQueryRepositoryTest;

public class SqliteTagCommandRepositoryTests : IClassFixture<SqliteTagFixture>
{
    private readonly SqliteTagFixture _fixture;

    public SqliteTagCommandRepositoryTests(SqliteTagFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Add_new_unique_tag_returns_success_and_persists()
    {
        var tag = Tag.Create( "Landscape");

        var addResult = _fixture.CommandRepository.Add(tag.Value!);

        Assert.True(addResult.IsSuccess);

        var readResult = _fixture.QueryRepository.GetById(0);
        Assert.True(readResult.IsSuccess);
        Assert.Equal("Landscape", readResult.Value!.Name);
    }

    [Fact]
    public void Add_duplicate_name_returns_failure()
    {
        var tag = Tag.Create( "Nature");

        var addResult = _fixture.CommandRepository.Add(tag.Value!);

        Assert.True(addResult.IsFailure);
    }

    [Fact]
    public void Delete_existing_free_tag_returns_success()
    {
        var deleteResult = _fixture.CommandRepository.Delete(3);

        Assert.True(deleteResult.IsSuccess);

        var readResult = _fixture.QueryRepository.GetById(3);
        Assert.True(readResult.IsFailure);
    }

    [Fact]
    public void Delete_tag_used_by_photo_returns_failure()
    {
        _fixture.Connection.Execute("""
                                        INSERT INTO PhotoTags (PhotoId, TagId) VALUES (10, 1);
                                    """);

        var deleteResult = _fixture.CommandRepository.Delete(1);

        Assert.True(deleteResult.IsFailure);
    }
}