using System;
using System.IO;

using PhotoCatalog.Infrastructure.Errors;
using PhotoCatalog.Infrastructure.Services;

using Serilog;

using Xunit;

namespace PhotoCatalog.Test.Unit;

public class LocalFileStorageTests : IDisposable
{
    private readonly string _testRootPath;
    private readonly ILogger _logger;

    public LocalFileStorageTests()
    {
        _testRootPath = Path.Combine(Path.GetTempPath(), "PhotoCatalogTests", Guid.NewGuid().ToString("N"));
        _logger = new LoggerConfiguration().CreateLogger();
    }

    [Fact]
    public void StoreFile_WhenSourceExists_ShouldReturnSuccessAndCopyFile()
    {
        Directory.CreateDirectory(_testRootPath);
        var sourcePath = Path.Combine(_testRootPath, "source.txt");
        File.WriteAllText(sourcePath, "payload");

        var storagePath = Path.Combine(_testRootPath, "storage");
        var sut = new LocalFileStorage(_logger, storagePath);

        var result = sut.StoreFile(sourcePath, "target.txt");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(File.Exists(result.Value));
        Assert.Equal("payload", File.ReadAllText(result.Value!));
    }

    [Fact]
    public void DeleteFile_WhenFileMissing_ShouldReturnSuccess()
    {
        var storagePath = Path.Combine(_testRootPath, "storage");
        var sut = new LocalFileStorage(_logger, storagePath);
        var missingPath = Path.Combine(storagePath, "missing.txt");

        var result = sut.DeleteFile(missingPath);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void DeleteFile_WhenFileExists_ShouldDeleteAndReturnSuccess()
    {
        var storagePath = Path.Combine(_testRootPath, "storage");
        Directory.CreateDirectory(storagePath);

        var filePath = Path.Combine(storagePath, "to-delete.txt");
        File.WriteAllText(filePath, "delete-me");

        var sut = new LocalFileStorage(_logger, storagePath);

        var result = sut.DeleteFile(filePath);

        Assert.True(result.IsSuccess);
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void FileExists_WhenFileExists_ShouldReturnTrue()
    {
        var storagePath = Path.Combine(_testRootPath, "storage");
        Directory.CreateDirectory(storagePath);

        var filePath = Path.Combine(storagePath, "exists.txt");
        File.WriteAllText(filePath, "1");

        var sut = new LocalFileStorage(_logger, storagePath);

        var result = sut.FileExists(filePath);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public void StoreFile_WhenSourceMissing_ShouldReturnIoError()
    {
        var storagePath = Path.Combine(_testRootPath, "storage");
        var sut = new LocalFileStorage(_logger, storagePath);

        var result = sut.StoreFile(Path.Combine(_testRootPath, "missing.txt"), "target.txt");

        Assert.True(result.IsFailure);
        Assert.Equal(InfrastructureErrors.FileStorage.IOError.Code, result.Error.Code);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testRootPath))
        {
            Directory.Delete(_testRootPath, recursive: true);
        }
    }
}
