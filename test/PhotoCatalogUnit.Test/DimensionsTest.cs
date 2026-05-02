using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;

using Xunit;

namespace PhotoCatalogUnit.Test;

public class DimensionsTest
{
    [Fact]
    public void Create_ValidWidthAndHeight_ReturnsSuccess()
    {
        var actual = Dimensions.Create(100,100);
        
        var expectation  = Dimensions.Create(100,100);
        
        
        Assert.Equal(expectation, actual);
    }

    [Fact]
    public void Create_MinimumValidSizeOne_ReturnsSuccess()
    {
        var actual = Dimensions.Create(1,1);
        
        var expectation  = Dimensions.Create(1,1);
        
        
        Assert.Equal(expectation, actual);
    }

    [Fact]
    public void Create_ZeroValues_ReturnsFailure()
    {
        var actual = Dimensions.Create(0,0);
        Assert.False(actual.IsSuccess);
        Assert.True(actual.IsFailure);
        Assert.Equal(Result<Dimensions>.Failure(new Error("Dimensions.Invalid", "Ширина и высота должны быть больше нуля")), actual.Error);
    }
    
    public void Create_NegativeValues_ReturnsFailure()
    {
        var actual = Dimensions.Create(-1,-1);
        Assert.False(actual.IsSuccess);
        Assert.True(actual.IsFailure);
        Assert.Equal(Result<Dimensions>.Failure(new Error("Dimensions.Invalid", "Ширина и высота должны быть больше нуля")), actual.Error);
        
    }
    
    
}