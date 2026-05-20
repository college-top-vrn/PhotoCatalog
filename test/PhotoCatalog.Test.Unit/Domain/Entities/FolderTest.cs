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
    public void CreateFunctionMustCreateFolderIfParameterNameIsNotEmpty()
    {
        const string name = "Test";
        const int id = 1;
        Result<Folder> folder = Folder.Create(id, name);

        Assert.Equal(id, folder.Value!.Id);
        Assert.Equal(name, folder.Value.Name);
    }

    /// <summary>
    ///     Проверяет, что папка не создается, если переданное имя пустое.
    /// </summary>
    [Fact]
    public void CreateFunctionDontCreateFolderIfParameterNameIsEmpty()
    {
        const int id = 1;
        Result<Folder> folder = Folder.Create(id, string.Empty);

        Assert.True(folder.IsFailure);
    }

    /// <summary>
    ///     Проверяет, что имя папки успешно изменяется, если переданное новое имя не пустое.
    /// </summary>
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

    /// <summary>
    ///     Проверяет, что имя папки не изменяется, если переданное новое имя пустое.
    /// </summary>
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

    /// <summary>
    ///     Проверяет, что папка успешно перемещается в другую папку, если идентификатор целевой папки не совпадает с идентификатором перемещаемой папки.
    /// </summary>
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

    /// <summary>
    ///     Проверяет, что папка не перемещается в саму себя, если идентификаторы совпадают.
    /// </summary>
    [Fact]
    public void MoveToFunctionDontMoveToGivenFolderIfFolderIdIsEqualToThisId()
    {
        const string name = "Test";
        const int id = 1;
        Result<Folder> folder = Folder.Create(id, name);

        ResultVoid result = folder.Value!.MoveTo(folder.Value!);

        Assert.True(result.IsFailure);
    }

    /// <summary>
    ///     Проверяет, что папка успешно перемещается в корень.
    /// </summary>
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