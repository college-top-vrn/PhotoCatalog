using System.Collections.Concurrent;

using PhotoCatalog.Domain.Entities;

namespace PhotoCatalog.Infrastructure.Fakes;

/// <summary>
///     Имитация базы данных.
/// </summary>
public class FakeDatabase
{
    private static ConcurrentDictionary<int, Folder> Folders { get; } = [];
    
    private static ConcurrentDictionary<int, Album> Albums { get; } = [];
    
    private static ConcurrentDictionary<int, Photo> Photos { get; } = [];
    
    private static ConcurrentDictionary<int, Tag> Tags { get; } = [];

    /// <summary>
    ///     Репозиторий папок для чтения данных.
    /// </summary>
    public static FakeFolderQueryRepository FakeFolderQueryRepository { get; } = new(Folders);
    /// <summary>
    ///     Репозиторий папок для изменения данных.
    /// </summary>
    public static FakeFolderCommandRepository FakeFolderCommandRepository { get; } = new(Folders, FakeFolderQueryRepository);
}