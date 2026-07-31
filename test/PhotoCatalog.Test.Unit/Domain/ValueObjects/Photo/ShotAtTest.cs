using System;

using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects.Photo;

using Xunit;

namespace PhotoCatalog.Test.Unit.Domain.ValueObjects.Photo;

public class ShotAtTest
{
    [Fact]
    public void Create_CreatingShotAtWithCorrectValue_ReturnsSuccessWithShotAt()
    {
        DateOnly date = new(2026, 7, 31);
        TimeOnly time = new(10, 20, 30);
        TimeSpan timeSpan = TimeSpan.FromHours(3);
        DateTimeOffset expectedDateTimeOffset = new(date, time, timeSpan);

        Result<ShotAt> result = ShotAt.Create(expectedDateTimeOffset.ToString());

        DateTimeOffset actualDateTimeOffset = result.Value!.Value;

        Assert.True(result.IsSuccess);
        Assert.Equal(actualDateTimeOffset, expectedDateTimeOffset);
    }

    [Fact]
    public void Create_CreatingShotAtWithIncorrectValue_ReturnsFailureWithError()
    {
        const string dateTimeOffset = "Test";

        Result<ShotAt> result = ShotAt.Create(dateTimeOffset);

        Assert.True(result.IsFailure);
        Assert.Equal(result.ResultError, DomainErrors.ShotAt.IsInvalid);
    }
}