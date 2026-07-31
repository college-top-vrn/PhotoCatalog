using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects.Photo;

using Xunit;

namespace PhotoCatalog.Test.Unit.Domain.ValueObjects.Photo;

public class StorageKeyTest
{
    [Fact]
    public void Create_CreatingStorageKeyWithCorrectValue_ReturnsSuccessWithStorageKey()
    {
        const string expectedStorageKey = "Test";

        Result<StorageKey> result = StorageKey.Create(expectedStorageKey);

        string actualStorageKey = result.Value!.Value;

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedStorageKey, actualStorageKey);
    }

    [Fact]
    public void Create_CreatingStorageKeyWithEmptyValue_ReturnsFailureWithError()
    {
        string expectedStorageKey = string.Empty;

        Result<StorageKey> result = StorageKey.Create(expectedStorageKey);

        Assert.True(result.IsFailure);
        Assert.Equal(result.ResultError, DomainErrors.StorageKey.IsEmpty);
    }
}