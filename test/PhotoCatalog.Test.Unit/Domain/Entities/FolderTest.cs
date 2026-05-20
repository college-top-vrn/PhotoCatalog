using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit.Domain.Entities;

/// <summary>
///     Содержит модульные тесты для проверки работы доменной сущности Folder.
/// </summary>
public class FolderTest
{
    /// <summary>
    ///     Проверяет, что папка успешно создается, если переданное имя не пустое.
    /// </summary>
    [Fact]
    public void CreateFunction_MustCreateFolder_IfParameterNameIsNotEmpty()
    {
        const string name = "Test";
        const int id = 1;
        var folder = Folder.Create(id, name);

        Assert.Equal(id, folder.Value!.Id);
        Assert.Equal(name, folder.Value.Name);
    }

    /// <summary>
    ///     Проверяет, что папка не создается, если переданное имя пустое.
    /// </summary>
    [Fact]
    public void CreateFunction_DontCreateFolder_IfParameterNameIsEmpty()
    {
        const string name = "";
        const int id = 1;
        var folder = Folder.Create(id, string.Empty);

        Assert.True(folder.IsFailure);
    }

    /// <summary>
    ///     Проверяет, что имя папки успешно изменяется, если переданное новое имя не пустое.
    /// </summary>
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

    /// <summary>
    ///     Проверяет, что имя папки не изменяется, если переданное новое имя пустое.
    /// </summary>
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

    /// <summary>
    ///     Проверяет, что папка успешно перемещается в другую папку, если идентификатор целевой папки не совпадает с идентификатором перемещаемой папки.
    /// </summary>
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

    /// <summary>
    ///     Проверяет, что папка не перемещается в саму себя, если идентификаторы совпадают.
    /// </summary>
    [Fact]
    public void MoveToFunction_DontMoveToGivenFolder_IfFolderIdIsEqualToThisId()
    {
        const string name = "Test";
        const int id = 1;
        var folder = Folder.Create(id, name);

        var result = folder.Value!.MoveTo(folder.Value!);

        Assert.True(result.IsFailure);
    }

    /// <summary>
    ///     Проверяет, что папка успешно перемещается в корень.
    /// </summary>
    [Fact]
    public void MoveToRootFunction_MoveToRootSuccessfully()
    {
        const string name = "Test";
        const int id = 1;
        var folder = Folder.Create(id, name);

        var exception = folder.Value!.MoveToRoot();

        Assert.True(exception.IsSuccess);
    }
}