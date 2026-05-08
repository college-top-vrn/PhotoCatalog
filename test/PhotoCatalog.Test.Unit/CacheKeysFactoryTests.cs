using PhotoCatalog.Application.Caching;

using Xunit;

namespace PhotoCatalog.Test.Unit;

public class CacheKeysFactoryTests
{
    [Fact]
    public void GetFolderAlbumsKey_WithValidId_ReturnsExpectedFormat()
    {
        const int folderId = 42;
        string key = CacheKeysFactory.GetFolderAlbumsKey(folderId);
        Assert.Equal("key:folder:42:albums-key", key);
    }

    [Fact]
    public void GetFoldersTreeKey_ReturnsConstantValue()
    {
        string key = CacheKeysFactory.GetFoldersTreeKey();
        Assert.Equal("key:folders-tree-key", key);
    }

    [Fact]
    public void GetFolderTag_WithValidId_ReturnsExpectedFormat()
    {
        const int folderId = 10;
        string tag = CacheKeysFactory.GetFolderTag(folderId);
        Assert.Equal("tag:folder:10:folder-tag", tag);
    }

    [Fact]
    public void GetFoldersTreeTag_ReturnsConstantValue()
    {
        string tag = CacheKeysFactory.GetFoldersTreeTag();
        Assert.Equal("tag:folders-tree-tag", tag);
    }

    [Fact]
    public void KeyAndTag_ForSameFolder_ShouldBeDifferent()
    {
        const int folderId = 5;
        string key = CacheKeysFactory.GetFolderAlbumsKey(folderId);
        string tag = CacheKeysFactory.GetFolderTag(folderId);
        Assert.NotEqual(key, tag);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void GetFolderAlbumsKey_WithEdgeValues_ContainsIdInString(int folderId)
    {
        string key = CacheKeysFactory.GetFolderAlbumsKey(folderId);
        Assert.Contains(folderId.ToString(), key);
        Assert.StartsWith("key:folder:", key);
    }
}