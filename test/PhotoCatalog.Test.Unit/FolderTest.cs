using PhotoCatalog.Domain.Entities;

using Xunit;

namespace PhotoCatalog.Test.Unit;

public class FolderTest
{
    [Fact]
    public void CreateFunction_MustCreateFolder_IfParameterNameIsNotEmpty()
    {
        const string testName = "Test";
        const int testId = 1;
        var testFolder = Folder.Create(testId, testName);
        
        Assert.Equal(testId, testFolder.Id);
        Assert.Equal(testName, testFolder.Name);
    }
}