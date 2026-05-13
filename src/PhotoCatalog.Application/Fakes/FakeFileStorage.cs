using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Application.Fakes;

/// <summary>
///     Заглушка файлового хранилища для тестирования.
/// </summary>
public class FakeFileStorage : IFileStorage
{
    /// <inheritdoc />
    public Result<string> StoreFile(string sourcePath, string newFileName)
    {
        string fakePath = $"/fake-storage/{newFileName}";
        return Result<string>.Success(fakePath);
    }

    /// <inheritdoc />
    public ResultVoid DeleteFile(string filePath)
    {
        return ResultVoid.Success();
    }

    /// <inheritdoc />
    public Result<bool> FileExists(string filePath)
    {
        return Result<bool>.Success(true);
    }
}