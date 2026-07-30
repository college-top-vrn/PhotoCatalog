using System;

using PhotoCatalog.Domain.ValueObjects.Photo;

using Xunit;

namespace PhotoCatalog.Test.Unit.Domain.ValueObjects.Photo;

public class MetadataBuilder
{
    [Fact]
    public void Build_BuildMetadataWithValues_ReturnsMetadata()
    {
        Metadata.Builder builder = new();

        var metadata = builder.SetMake("Sony")
            .SetModel("ILCE-7M3")
            .SetLensModel("FE 24-70mm F2.8 GM")
            .SetFocalLength(50.0)
            .SetAperture(2.8)
            .SetExposureTime("1/500")
            .SetIso(100.0)
            .SetHasFlashfire(false)
            .SetLatitude(59.934280)
            .SetLongitude(30.335098)
            .SetAltitude(15.4)
            .SetOrientation(Orientation.Horizontal)
            .SetColorSpace("sRGB")
            .SetShotAt(new
                DateTimeOffset(
                    2026,
                    7,
                    15,
                    14,
                    30,
                    0,
                    TimeSpan.FromHours(3))
            )
            .SetHasHdr(false)
            .SetIsPanorama(false)
            .SetArtist("Ivan Ivanov")
            .Build()
            .Value;

        Assert.False((metadata.Make is null ||
                      metadata.Model is null ||
                      metadata.LensModel is null ||
                      metadata.FocalLength is null ||
                      metadata.Aperture is null ||
                      metadata.ExposureTime is null ||
                      metadata.Iso is null ||
                      metadata.HasFlashfire is null ||
                      metadata.Latitude is null ||
                      metadata.Longitude is null ||
                      metadata.Altitude is null ||
                      metadata.Orientation is null ||
                      metadata.ColorSpace is null ||
                      metadata.ShotAt is null));
    }

    [Fact]
    public void Build_BuildMetadataWithoutValues_ReturnsMetadata()
    {
        Metadata.Builder builder = new();

        var metadata = builder
            .Build()
            .Value;

        Assert.True((metadata.Make is null ||
                     metadata.Model is null ||
                     metadata.LensModel is null ||
                     metadata.FocalLength is null ||
                     metadata.Aperture is null ||
                     metadata.ExposureTime is null ||
                     metadata.Iso is null ||
                     metadata.HasFlashfire is null ||
                     metadata.Latitude is null ||
                     metadata.Longitude is null ||
                     metadata.Altitude is null ||
                     metadata.Orientation is null ||
                     metadata.ColorSpace is null ||
                     metadata.ShotAt is null));
    }

    [Fact]
    public void Reset_ResetsMetadataToNewExample()
    {
        Metadata.Builder builder = new();

        builder.SetMake("Sony")
            .SetModel("ILCE-7M3")
            .SetLensModel("FE 24-70mm F2.8 GM")
            .SetFocalLength(50.0)
            .SetAperture(2.8)
            .SetExposureTime("1/500")
            .SetIso(100.0)
            .SetHasFlashfire(false)
            .SetLatitude(59.934280)
            .SetLongitude(30.335098)
            .SetAltitude(15.4)
            .SetOrientation(Orientation.Horizontal)
            .SetColorSpace("sRGB")
            .SetShotAt(new
                DateTimeOffset(
                    2026,
                    7,
                    15,
                    14,
                    30,
                    0,
                    TimeSpan.FromHours(3))
            )
            .SetHasHdr(false)
            .SetIsPanorama(false)
            .SetArtist("Ivan Ivanov");

        builder.Reset();

        var metadata = builder.Build().Value;

        Assert.True((metadata.Make is null ||
                     metadata.Model is null ||
                     metadata.LensModel is null ||
                     metadata.FocalLength is null ||
                     metadata.Aperture is null ||
                     metadata.ExposureTime is null ||
                     metadata.Iso is null ||
                     metadata.HasFlashfire is null ||
                     metadata.Latitude is null ||
                     metadata.Longitude is null ||
                     metadata.Altitude is null ||
                     metadata.Orientation is null ||
                     metadata.ColorSpace is null ||
                     metadata.ShotAt is null));
    }
}