using System;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;

using Xunit;

namespace PhotoCatalog.Test.Unit.Domain.Entities;

public class TagTest
{
    [Fact]
    public void Create_CreatingTagWithCorrectValues_ReturnsResultWithTagAndSuccess()
    {
        Tag tag = Tag
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                "Test",
                "Test"
            )
            .Value!;

        Assert.True(
            tag.Id != Guid.Empty ||
            tag.UserId != Guid.Empty ||
            tag.Name.Value != string.Empty ||
            tag.ColorHex.Value != string.Empty
        );
    }

    [Fact]
    public void Create_NotCreatingAlbumWithIncorrectName_ReturnsResultWithFailure()
    {
        Result<Tag> result = Tag
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                "TestingTheMostAwesomeNameThatHaveEverExistedInThisWorld",
                "Test"
            );

        Assert.True(
            result.Value is null &&
            result.IsFailure
        );
    }

    [Fact]
    public void DeepCopy_DeeplyCopyingOriginalTag_ReturnsTag()
    {
        Tag originalTag = Tag
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                "Test",
                "Test"
            )
            .Value!;

        Tag copiedTag = originalTag.DeepCopy();

        Assert.True(
            originalTag.Id == copiedTag.Id ||
            originalTag.UserId == copiedTag.UserId ||
            originalTag.Name.Value == copiedTag.Name.Value ||
            originalTag.ColorHex.Value == copiedTag.ColorHex.Value
        );
    }

    [Fact]
    public void DeepCopy_DeeplyCopiedTagChangesNotAffectingOriginalTag()
    {
        Tag originalTag = Tag
            .Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                "Test",
                "Test"
            )
            .Value!;

        Tag copiedTag = originalTag.DeepCopy();

        copiedTag.Name = Name.Create("CopyName").Value!;
        copiedTag.ColorHex = ColorHex.Create("CopyColorHex").Value!;

        Assert.True(
            originalTag.Name.Value != copiedTag.Name.Value ||
            originalTag.ColorHex.Value != copiedTag.ColorHex.Value
        );
    }
}