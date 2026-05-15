using System;
using System.IO;
using NSubstitute;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Services;
using PhotoSauce.MagicScaler;
using Serilog;
using Xunit;

namespace PhotoCatalog.Tests.Infrastructure.Services;

public class MagicScalerThumbnailServiceTests : IDisposable
{
    private readonly ILogger _loggerMock;
    private readonly MagicScalerThumbnailService _service;
    private readonly string _outputFolder;

    
    private readonly string _landscapeSourcePath = Path.Combine("MagicScalerThumbnailServiceTests", "landscape_photo.jpg");
    private readonly string _portraitSourcePath = Path.Combine("MagicScalerThumbnailServiceTests", "vertical_smartphone.jpg");

    public MagicScalerThumbnailServiceTests()
    {
        
        _loggerMock = Substitute.For<ILogger>();
        _service = new MagicScalerThumbnailService(_loggerMock);
        
        _outputFolder = Path.Combine(Path.GetTempPath(), "ThumbnailTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_outputFolder);
    }

    [Fact]
    public void Generate_ShouldSuccessfullyResizeImage_WhenSourceFileIsValid()
    {
        
        string targetPath = Path.Combine(_outputFolder, "landscape_thumb.jpg");
        int maxSize = 200;

        
        ResultVoid result = _service.Generate(_landscapeSourcePath, targetPath, maxSize);

        Assert.True(result.IsSuccess);
        Assert.True(File.Exists(targetPath));

        
        var thumbFileInfo = ImageFileInfo.Load(targetPath);

        
        Assert.True(thumbFileInfo.Frames[0].Width <= maxSize);
        Assert.True(thumbFileInfo.Frames[0].Height <= maxSize);
    }

    [Fact]
    public void Generate_ShouldMaintainCorrectOrientation_ForPortraitSmartphonePhotos()
    {
        
        string targetPath = Path.Combine(_outputFolder, "portrait_thumb.jpg");
        int maxSize = 300;

        
        ResultVoid result = _service.Generate(_portraitSourcePath, targetPath, maxSize);

        
        Assert.True(result.IsSuccess);

        
        var thumbFileInfo = ImageFileInfo.Load(targetPath);
        var frame = thumbFileInfo.Frames[0];

        
        Assert.True(frame.Height > frame.Width);
        Assert.Equal(maxSize, frame.Height); 
    }

    [Fact]
    public void Generate_ShouldExecuteWithOptimizedMemoryAllocations()
    {
        
        string targetPath = Path.Combine(_outputFolder, "memory_test_thumb.jpg");
        int maxSize = 150;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        long memoryBefore = GC.GetTotalMemory(true);

    
        ResultVoid result = _service.Generate(_landscapeSourcePath, targetPath, maxSize);

 
        long memoryAfter = GC.GetTotalMemory(false);
        long allocatedMemory = memoryAfter - memoryBefore;

   
        Assert.True(result.IsSuccess);
        
        long maxAllowedAllocationsedBytes = 10 * 1024 * 1024;
        Assert.True(allocatedMemory < maxAllowedAllocationsedBytes, 
            $"Генерация миниатюр превысила порог памяти. Выделено байт: {allocatedMemory}");
    }

    public void Dispose()
    {

        if (Directory.Exists(_outputFolder))
        {
            Directory.Delete(_outputFolder, true);
        }
    }
}