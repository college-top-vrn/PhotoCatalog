using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Application.Fakes;

/// <summary>
///     Заглушка валидатора иерархии папок для тестирования.
/// </summary>
public class FakeFolderHierarchyValidator : IFolderHierarchyValidator
{
    /// <inheritdoc />
    public ResultVoid CheckForCycles(int sourceFolderId, int targetFolderId)
    {
        return ResultVoid.Success();
    }
}