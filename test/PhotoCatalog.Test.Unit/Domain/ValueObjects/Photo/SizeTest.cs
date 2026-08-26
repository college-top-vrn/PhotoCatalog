// using System;
//
// using PhotoCatalog.Domain.Primitives;
// using PhotoCatalog.Domain.ValueObjects.Photo;
//
// using Xunit;
//
// namespace PhotoCatalog.Test.Unit.Domain.ValueObjects.Photo;
//
// public class SizeTest
// {
//     [Fact]
//     public void Create_CreatingSizeWithCorrectValue_ReturnsSuccessWithSize()
//     {
//         const Int64 expectedSize = 100;
//
//         Result<Size> result = Size.Create(100);
//
//         Int64 actualSize = result.Value!.Value;
//         
//         Assert.True(result.IsSuccess);
//         Assert.Equal(expectedSize, actualSize);
//     }
// }