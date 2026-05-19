using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using NSubstitute;
using Serilog;
using Xunit;
using PhotoCatalog.Infrastructure.Repositories;
using PhotoCatalog.Infrastructure.UnitOfWork;

public sealed class SqliteTagFixture : IAsyncLifetime
{
    public SqliteConnection Connection { get; private set; } = null!;
    public SqliteUnitOfWork UnitOfWork { get; private set; } = null!;
    public SqliteTagQueryRepository QueryRepository { get; private set; } = null!;
    public SqliteTagCommandRepository CommandRepository { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var connString = $"DataSource=file:test{Guid.NewGuid():N}?mode=memory&cache=shared";
        Connection = new SqliteConnection(connString);
        await Connection.OpenAsync();
        await Connection.ExecuteAsync("PRAGMA foreign_keys = ON;");

        await Connection.ExecuteAsync("""
            CREATE TABLE Tags (
                Id INTEGER NOT NULL,
                Name TEXT NOT NULL UNIQUE,
                PRIMARY KEY(Id)
            );

            CREATE TABLE Photos (
                Id INTEGER NOT NULL,
                RealPath TEXT NOT NULL UNIQUE,
                FileHash TEXT,
                Dimensions TEXT,
                AddedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY(Id)
            );

            CREATE TABLE PhotoTags (
                PhotoId INTEGER NOT NULL,
                TagId INTEGER NOT NULL,
                PRIMARY KEY(PhotoId, TagId),
                FOREIGN KEY(PhotoId) REFERENCES Photos(Id) ON UPDATE NO ACTION ON DELETE CASCADE,
                FOREIGN KEY(TagId) REFERENCES Tags(Id) ON UPDATE NO ACTION ON DELETE CASCADE
            );
        """);

        await Connection.ExecuteAsync("""
            INSERT INTO Tags (Id, Name) VALUES
            (1, 'Nature'),
            (2, 'City'),
            (3, 'Night');
        """);

        await Connection.ExecuteAsync("""
            INSERT INTO Photos (Id, RealPath, FileHash, Dimensions, AddedAt)
            VALUES (10, '/img/a.jpg', 'hash-1', '1920x1080', CURRENT_TIMESTAMP);
        """);

        var serilogLogger = Substitute.For<ILogger>();
        var msLogger = Substitute.For<Microsoft.Extensions.Logging.ILogger<SqliteUnitOfWork>>();

        UnitOfWork = new SqliteUnitOfWork(Connection.ToString(), msLogger);
        QueryRepository = new SqliteTagQueryRepository(Connection.ToString(), serilogLogger);
        CommandRepository = new SqliteTagCommandRepository(UnitOfWork.ToString()!, serilogLogger);
    }

    public async Task DisposeAsync()
    {
        UnitOfWork.Dispose();
        await Connection.DisposeAsync();
    }
}