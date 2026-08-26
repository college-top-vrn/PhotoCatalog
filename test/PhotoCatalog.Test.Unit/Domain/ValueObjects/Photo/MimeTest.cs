// using PhotoCatalog.Domain.Primitives;
// using PhotoCatalog.Domain.ValueObjects.Photo;
//
// using Xunit;
//
// namespace PhotoCatalog.Test.Unit.Domain.ValueObjects.Photo;
//
// public class MimeTest
// {
//     [Fact]
//     public void Create_CreatingMimeWithCorrectValue_ReturnsSuccessWithMime()
//     {
//         const string mimeValue = "Test";
//
//         Result<Mime> result = Mime.Create(mimeValue);
//
//         Assert.True(result.IsSuccess);
//         Assert.Equal(mimeValue, result.Value!.Value);
//     }
//
//     [Fact]
//     public void Create_CreatingMimeWithEmptyValue_ReturnsFailureWithError()
//     {
//         string mimeValue = string.Empty;
//
//         Result<Mime> result = Mime.Create(mimeValue);
//
//         Assert.True(result.IsFailure);
//         Assert.Equal(result.ResultError, DomainErrors.Mime.IsEmpty);
//     }
// }