using PhotoCatalog.Application.Caching;

using Xunit;

namespace PhotoCatalog.Test.Unit.Application.Caching;

/// <summary>
///     Содержит модульные тесты для проверки работы CacheKeysFactory.
/// </summary>
public class CacheKeysFactoryTests
{
    /// <summary>
    ///     Проверяет, что метод GetFolderAlbumsKey возвращает ключ в правильном формате для валидного идентификатора.
    /// </summary>
    [Fact]
    public void GetFolderAlbumsKeyWithValidIdReturnsExpectedFormat()
    {
        const int folderId = 42;
        string key = CacheKeysFactory.GetFolderAlbumsKey(folderId);
        Assert.Equal("key:folder:42:albums-key", key);
    }

    /// <summary>
    ///     Проверяет, что метод GetFoldersTreeKey возвращает константное значение.
    /// </summary>
    [Fact]
    public void GetFoldersTreeKeyReturnsConstantValue()
    {
        string key = CacheKeysFactory.GetFoldersTreeKey();
        Assert.Equal("key:folders-tree-key", key);
    }

    /// <summary>
    ///     Проверяет, что метод GetFolderTag возвращает тег в правильном формате для валидного идентификатора.
    /// </summary>
    [Fact]
    public void GetFolderTagWithValidIdReturnsExpectedFormat()
    {
        const int folderId = 10;
        string tag = CacheKeysFactory.GetFolderTag(folderId);
        Assert.Equal("tag:folder:10:folder-tag", tag);
    }

    /// <summary>
    ///     Проверяет, что метод GetFoldersTreeTag возвращает константное значение.
    /// </summary>
    [Fact]
    public void GetFoldersTreeTagReturnsConstantValue()
    {
        string tag = CacheKeysFactory.GetFoldersTreeTag();
        Assert.Equal("tag:folders-tree-tag", tag);
    }

    /// <summary>
    ///     Проверяет, что ключ и тег для одной и той же папки различаются.
    /// </summary>
    [Fact]
    public void KeyAndTagForSameFolderShouldBeDifferent()
    {
        const int folderId = 5;
        string key = CacheKeysFactory.GetFolderAlbumsKey(folderId);
        string tag = CacheKeysFactory.GetFolderTag(folderId);
        Assert.NotEqual(key, tag);
    }

    /// <summary>
    ///     Проверяет, что метод GetFolderAlbumsKey корректно обрабатывает граничные значения идентификатора.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void GetFolderAlbumsKeyWithEdgeValuesContainsIdInString(int folderId)
    {
        string key = CacheKeysFactory.GetFolderAlbumsKey(folderId);
        Assert.Contains(folderId.ToString(), key);
        Assert.StartsWith("key:folder:", key);
    }
}