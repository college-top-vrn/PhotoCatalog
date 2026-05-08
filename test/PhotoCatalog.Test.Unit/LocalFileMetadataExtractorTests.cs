using System;
using System.IO;
using System.Security.Cryptography;

using PhotoCatalog.Infrastructure.Errors;
using PhotoCatalog.Infrastructure.Services;

using Serilog;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using Xunit;

namespace PhotoCatalog.Test.Unit;

public class LocalFileMetadataExtractorTests : IDisposable
{
    private readonly string _testRootPath;
    private readonly ILogger _logger;

    public LocalFileMetadataExtractorTests()
    {
        _testRootPath = Path.Combine(Path.GetTempPath(), "PhotoCatalogTests", Guid.NewGuid().ToString("N"));
        _logger = new LoggerConfiguration().CreateLogger();
        Directory.CreateDirectory(_testRootPath);
    }

    [Fact]
    public void CalculateHash_WhenFileExists_ShouldReturnSha256Hash()
    {
        var filePath = Path.Combine(_testRootPath, "hash.txt");
        File.WriteAllText(filePath, "hash-content");

        using var stream = File.OpenRead(filePath);
        var expectedHash = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();

        var sut = new LocalFileMetadataExtractor(_logger);

        var result = sut.CalculateHash(filePath);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedHash, result.Value);
    }

    [Fact]
    public void CalculateHash_WhenFileMissing_ShouldReturnIoError()
    {
        var missingPath = Path.Combine(_testRootPath, "missing.txt");
        var sut = new LocalFileMetadataExtractor(_logger);

        var result = sut.CalculateHash(missingPath);

        Assert.True(result.IsFailure);
        Assert.Equal(InfrastructureErrors.FileStorage.IOError.Code, result.Error.Code);
    }

    [Fact]
    public void GetDimensions_WhenImageIsValid_ShouldReturnDimensions()
    {
        var imagePath = Path.Combine(_testRootPath, "valid.png");
        using (var image = new Image<Rgba32>(13, 21))
        {
            image.SaveAsPng(imagePath);
        }

        var sut = new LocalFileMetadataExtractor(_logger);
        var result = sut.GetDimensions(imagePath);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(13, result.Value!.Width);
        Assert.Equal(21, result.Value.Height);
    }

    [Fact]
    public void GetDimensions_WhenFileIsNotImage_ShouldReturnIoError()
    {
        var filePath = Path.Combine(_testRootPath, "not-image.txt");
        File.WriteAllText(filePath, "plain-text");

        var sut = new LocalFileMetadataExtractor(_logger);
        var result = sut.GetDimensions(filePath);

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
