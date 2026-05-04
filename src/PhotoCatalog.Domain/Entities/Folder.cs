using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Entities;

/// <summary>
///     Представляет папку.
/// </summary>
/// /// <remarks>
///     ВНИМАНИЕ: Не создавайте объект через конструктор по умолчанию.
/// </remarks>
public class Folder
{
    private Folder() { }

    /// <summary>
    ///     Идентификатор.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    ///     Идентификатор родителя.
    /// </summary>
    public int? ParentFolderId { get; private set; }

    /// <summary>
    ///     Название.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    ///     Фабричный метод по созданию экземпляров класса Folder.
    /// </summary>
    /// <param name="id">идентификатор.</param>
    /// <param name="name">название.</param>
    /// <returns>Возвращает экземпляр.</returns>
    public static Result<Folder> Create(int id, string name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? Result<Folder>.Failure(DomainErrors.Folder.EmptyName)
            : new Folder { Id = id, Name = name };
    }

    /// <summary>
    ///     Переименовать экземпляр.
    /// </summary>
    /// <param name="newName">новое название.</param>
    /// <returns>Возвращает результат выполнения.</returns>
    public ResultVoid Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return ResultVoid.Failure(DomainErrors.Folder.EmptyName);

        Name = newName;

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Перемещает папку к родительской папке.
    /// </summary>
    /// <param name="parentFolder">родительская папка.</param>
    /// <returns>Возвращает результата выполнения.</returns>
    public ResultVoid MoveTo(Folder parentFolder)
    {
        if (parentFolder.Id == Id) return ResultVoid.Failure(DomainErrors.Folder.CannotMoveToSelf);

        ParentFolderId = parentFolder.Id;

        return ResultVoid.Success();
    }

    /// <summary>
    ///     Перемещает папкку в корень.
    /// </summary>
    /// <returns>Возвращает результата выполнения.</returns>
    public ResultVoid MoveToRoot()
    {
        ParentFolderId = null;

        return ResultVoid.Success();
    }
}