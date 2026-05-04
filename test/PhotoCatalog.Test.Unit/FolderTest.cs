using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit;

public class FolderTest
{
    [Fact]
    public void CreateFunction_MustCreateFolder_IfParameterNameIsNotEmpty()
    {
        const string name = "Test";
        const int id = 1;
        var folder = Folder.Create(id, name);

        Assert.Equal(id, folder.Value!.Id);
        Assert.Equal(name, folder.Value.Name);
    }

    [Fact]
    public void CreateFunction_DontCreateFolder_IfParameterNameIsEmpty()
    {
        const string name = "";
        const int id = 1;
        var folder = Folder.Create(id, name);

        var expectedError = DomainErrors.Folder.EmptyName;

        Assert.Equal(folder.Error.Message, expectedError.Message);
    }

    [Fact]
    public void RenameFunction_RenameFolderName_IfParameterNameIsNotEmpty()
    {
        const string newName = "Test2";
        const string name = "Test";
        const int id = 1;
        var folder = Folder.Create(id, name);

        folder.Value!.Rename(newName);

        Assert.Equal(newName, folder.Value.Name);
    }

    [Fact]
    public void RenameFunction_DontRenameFolderName_IfParameterNameIsEmpty()
    {
        const string newName = "";
        const string name = "Test";
        const int id = 1;
        var folder = Folder.Create(id, name);

        var actualError = folder.Value!.Rename(newName);
        var expectedError = DomainErrors.Folder.EmptyName;

        Assert.Equal(actualError.IsFailure, ResultVoid.Failure(expectedError).IsFailure);
    }

    [Fact]
    public void MoveToFunction_MoveToGivenFolder_IfFolderIdIsNotEqualToThisId()
    {
        const string name = "Test";
        const int id = 1;
        var folder = Folder.Create(id, name);

        const string secondName = "Test2";
        const int secondId = 2;
        var folder2 = Folder.Create(secondId, secondName);

        var result = folder.Value!.MoveTo(folder2.Value!);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void MoveToFunction_DontMoveTOGivenFolder_IfFolderIdIsEqualToThisId()
    {
        const string name = "Test";
        const int id = 1;
        var folder = Folder.Create(id, name);

        var result = folder.Value!.MoveTo(folder.Value!);

        Assert.True(result.IsFailure);
    }
}