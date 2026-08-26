// using PhotoCatalog.Domain.Primitives;
// using PhotoCatalog.Domain.ValueObjects;
//
// using Xunit;
//
// namespace PhotoCatalog.Test.Unit.Domain.ValueObjects;
//
// public class NameTest
// {
//     [Fact]
//     public void Create_CreatingNameWithCorrectValue_ReturnsSuccessWithName()
//     {
//         Result<Name> resultName = Name.Create("Test");
//
//         Assert.True(
//             resultName.Value!.Value != string.Empty &&
//             resultName.IsSuccess
//         );
//     }
//
//     [Fact]
//     public void Create_CreatingNameWithEmptyValue_ReturnsFailureWithError()
//     {
//         Result<Name> resultName = Name.Create(string.Empty);
//
//         Assert.True(
//             resultName.IsFailure &&
//             resultName.ResultError == DomainErrors.Name.IsEmpty
//         );
//     }
//
//     [Fact]
//     public void Create_CreatingNameWithTooLongValue_ReturnsFailureWithError()
//     {
//         Result<Name> resultName = Name.Create("TestingTheMostAwesomeNameThatHaveEverExistedInThisWorld");
//
//         Assert.True(
//             resultName.IsFailure &&
//             resultName.ResultError == DomainErrors.Name.IsTooLong
//         );
//     }
// }