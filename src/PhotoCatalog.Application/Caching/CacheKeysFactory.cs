namespace PhotoCatalog.Application.Caching;

/// <summary>
///     Централизованная фабрика строковых идентификаторов
///     для кэширования.
///     Разделяет генерацию Key и Tag.
/// </summary>
public static class CacheKeysFactory
{
    private const string KeyPrefix = "key";
    private const string TagPrefix = "tag";
    private const string FolderInfix = "folder";
    private const string AlbumsSuffix = "albums-key";
    private const string TreeKeySuffix = "folders-tree-key";
    private const string FolderTagSuffix = "folder-tag";
    private const string TreeTagSuffix = "folders-tree-tag";

    /// <summary>
    ///     Ключ для списка альбомов конкретной папки.
    /// </summary>
    /// <param name="folderId">Идентификатор папки.</param>
    /// <returns>Уникальный ключ вида <c>key:folder:{folderId}:albums-key</c>.</returns>
    public static string GetFolderAlbumsKey(int folderId)
    {
        return $"{KeyPrefix}:{FolderInfix}:{folderId}:{AlbumsSuffix}";
    }

    /// <summary>
    ///     Ключ для полного дерева папок.
    /// </summary>
    /// <returns>Ключ вида <c>key:folders-tree-key</c>.</returns>
    public static string GetFoldersTreeKey()
    {
        return $"{KeyPrefix}:{TreeKeySuffix}";
    }

    /// <summary>
    ///     Тег, связанный с конкретной папкой.
    ///     Сброс кэша по этому тегу
    ///     инвалидирует все данные, относящиеся к папке.
    /// </summary>
    /// <param name="folderId">Идентификатор папки.</param>
    /// <returns>Тег вида <c>tag:folder:{folderId}:folder-tag</c>.</returns>
    public static string GetFolderTag(int folderId)
    {
        return $"{TagPrefix}:{FolderInfix}:{folderId}:{FolderTagSuffix}";
    }

    /// <summary>
    ///     Тег для всего дерева папок.
    ///     Сброс по этому тегу вызывает
    ///     полную перестройку дерева при следующем запросе.
    /// </summary>
    /// <returns>Тег вида <c>tag:folders-tree-tag</c>.</returns>
    public static string GetFoldersTreeTag()
    {
        return $"{TagPrefix}:{TreeTagSuffix}";
    }
}