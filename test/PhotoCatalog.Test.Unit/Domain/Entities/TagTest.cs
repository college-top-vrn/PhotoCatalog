using System;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.ValueObjects;

using Xunit;

namespace PhotoCatalog.Test.Unit.Domain.Entities;

public class TagTest
{
    [Fact]
    public void Create_CreatingTagWithCorrectValues_ReturnsTagWithSuccess()
    {
        Name expectedName = Name.Create("Test").Value!;
        ColorHex expectedColorHex = ColorHex.Create("Test").Value!;

        Tag tag = Tag.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            expectedName,
            expectedColorHex
        ).Value!;

        Assert.Equal(tag.Name, expectedName);
        Assert.Equal(tag.ColorHex, expectedColorHex);
    }

    [Fact]
    public void DeepCopy_DeeplyCopyingOriginalTag_ReturnsTagCopy()
    {
        Name name = Name.Create("Test").Value!;
        ColorHex colorHex = ColorHex.Create("Test").Value!;

        Tag original = Tag.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            name,
            colorHex
        ).Value!;

        Tag copy = original.DeepCopy();

        Assert.Equal(original.Id, copy.Id);
        Assert.Equal(original.UserId, copy.UserId);
        Assert.Equal(original.Name, copy.Name);
        Assert.Equal(original.ColorHex, copy.ColorHex);
    }

    [Fact]
    public void DeepCopy_DeeplyCopiedTagChangesNotAffectingOriginalTag()
    {
        Name originalName = Name.Create("Original").Value!;
        ColorHex originalColorHex = ColorHex.Create("Original").Value!;

        Tag original = Tag.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            originalName,
            originalColorHex
        ).Value!;

        Tag copy = original.DeepCopy();

        copy.Name = Name.Create("Copy").Value!;
        copy.ColorHex = ColorHex.Create("Copy").Value!;

        Assert.NotEqual(original.Name, copy.Name);
        Assert.NotEqual(original.ColorHex, copy.ColorHex);
    }
}