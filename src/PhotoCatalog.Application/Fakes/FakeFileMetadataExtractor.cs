using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;

namespace PhotoCatalog.Application.Fakes;

/// <summary>
///     Заглушка извлечения метаданных для тестирования.
/// </summary>
public class FakeFileMetadataExtractor : IFileMetadataExtractor
{
    /// <inheritdoc />
    public Result<string> CalculateHash(string filePath)
    {
        return Result.Success("fake-sha256-hash");
    }

    /// <inheritdoc />
    public Result<Dimensions> GetDimensions(string filePath)
    {
        return Result.Success(Dimensions.Create(1920, 1080).Value!);
    }
}