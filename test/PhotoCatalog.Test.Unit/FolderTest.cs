using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit;

public class FolderTest
{
    [Fact]
    public void CreateFunctionMustCreateFolderIfParameterNameIsNotEmpty()
    {
        const string name = "Test";
        const int id = 1;
        Result<Folder> folder = Folder.Create(id, name);

        Assert.Equal(id, folder.Value!.Id);
        Assert.Equal(name, folder.Value.Name);
    }

    [Fact]
    public void CreateFunctionDontCreateFolderIfParameterNameIsEmpty()
    {
        const int id = 1;
        Result<Folder> folder = Folder.Create(id, string.Empty);

        Assert.True(folder.IsFailure);
    }

    [Fact]
    public void RenameFunctionRenameFolderNameIfParameterNameIsNotEmpty()
    {
        const string newName = "Test2";
        const string name = "Test";
        const int id = 1;
        Result<Folder> folder = Folder.Create(id, name);

        folder.Value!.Rename(newName);

        Assert.Equal(newName, folder.Value.Name);
    }

    [Fact]
    public void RenameFunctionDontRenameFolderNameIfParameterNameIsEmpty()
    {
        const string newName = "";
        const string name = "Test";
        const int id = 1;
        Result<Folder> folder = Folder.Create(id, name);

        ResultVoid actualError = folder.Value!.Rename(newName);
        Error expectedError = DomainErrors.Folder.EmptyName;

        Assert.Equal(actualError.IsFailure, ResultVoid.Failure(expectedError).IsFailure);
    }

    [Fact]
    public void MoveToFunctionMoveToGivenFolderIfFolderIdIsNotEqualToThisId()
    {
        const string name = "Test";
        const int id = 1;
        Result<Folder> folder = Folder.Create(id, name);

        const string secondName = "Test2";
        const int secondId = 2;
        Result<Folder> folder2 = Folder.Create(secondId, secondName);

        ResultVoid result = folder.Value!.MoveTo(folder2.Value!);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void MoveToFunctionDontMoveToGivenFolderIfFolderIdIsEqualToThisId()
    {
        const string name = "Test";
        const int id = 1;
        Result<Folder> folder = Folder.Create(id, name);

        ResultVoid result = folder.Value!.MoveTo(folder.Value!);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void MoveToRootFunctionMoveToRootSuccessfully()
    {
        const string name = "Test";
        const int id = 1;
        Result<Folder> folder = Folder.Create(id, name);

        ResultVoid exception = folder.Value!.MoveToRoot();

        Assert.True(exception.IsSuccess);
    }
}