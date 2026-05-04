using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Entities;

public class Folder
{
    private Folder() { }
    public int Id { get; private set; }
    public int? ParentFolderId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public static Result<Folder> Create(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) Result<Folder>.Failure(DomainErrors.Folder.EmptyName);

        return new Folder { Id = id, Name = name };
    }

    public ResultVoid Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return ResultVoid.Failure(DomainErrors.Folder.EmptyName);

        Name = newName;

        return ResultVoid.Success();
    }

    public ResultVoid MoveTo(Folder parentFolder)
    {
        if (parentFolder.Id == Id) return ResultVoid.Failure(DomainErrors.Folder.CannotMoveToSelf);

        ParentFolderId = parentFolder.Id;

        return new ResultVoid();
    }

    public ResultVoid MoveToRoot()
    {
        ParentFolderId = null;

        return ResultVoid.Success();
    }
}