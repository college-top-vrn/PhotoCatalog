using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Entities;

public class Folder
{
    private Folder() { }
    public int Id { get; private set; }
    public int? ParentFolderId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public static Folder Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) Result<Folder>.Failure(new Error("Folder.EmptyName", "..."));

        return new Folder { Name = name };
    }

    public ResultVoid Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return ResultVoid.Failure(new Error("Folder.EmptyName", "..."));

        Name = newName;

        return ResultVoid.Success();
    }

    public ResultVoid MoveTo(Folder parentFolder)
    {
        if (parentFolder.Id == Id) return ResultVoid.Failure(new Error("Folder.CannotMoveToSelf", "..."));

        ParentFolderId = parentFolder.Id;

        return new ResultVoid();
    }

    public ResultVoid MoveToRoot()
    {
        ParentFolderId = null;

        return ResultVoid.Success();
    }
}