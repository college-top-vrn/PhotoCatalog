// using System;
//
// using PhotoCatalog.Domain.Primitives;
// using PhotoCatalog.Domain.ValueObjects.Photo;
//
// using Xunit;
//
// namespace PhotoCatalog.Test.Unit.Domain.ValueObjects.Photo;
//
// public class MetadataBuilderTest
// {
//     [Fact]
//     public void Build_BuildingMetadataWithValues_ReturnsMetadataWithSuccess()
//     {
//         Metadata.Builder builder = new();
//
//         const string expectedMake = "Sony";
//         const string expectedModel = "ILCE-7M3";
//         const string expectedLensModel = "FE 24-70mm F2.8 GM";
//         const double expectedFocalLength = 50.0;
//         const double expectedAperture = 2.8;
//         const string expectedExposureTime = "1/500";
//         const double expectedIso = 100.0;
//         const bool expectedHasFlashfire = false;
//         const double expectedLatitude = 59.934280;
//         const double expectedLongitude = 30.335098;
//         const double expectedAltitude = 15.4;
//         const Orientation expectedOrientation = Orientation.Horizontal;
//         const string expectedColorSpace = "sRGB";
//         DateTimeOffset expectedShotAt = new(
//             2026, 7, 15,
//             14, 30, 0,
//             TimeSpan.FromHours(3)
//         );
//         const bool expectedHasHdr = false;
//         const bool expectedIsPanorama = false;
//         const string expectedArtist = "Ivan Ivanov";
//
//         Result<Metadata> result = builder
//             .SetMake(expectedMake)
//             .SetModel(expectedModel)
//             .SetLensModel(expectedLensModel)
//             .SetFocalLength(expectedFocalLength)
//             .SetAperture(expectedAperture)
//             .SetExposureTime(expectedExposureTime)
//             .SetIso(expectedIso)
//             .SetHasFlashfire(expectedHasFlashfire)
//             .SetLatitude(expectedLatitude)
//             .SetLongitude(expectedLongitude)
//             .SetAltitude(expectedAltitude)
//             .SetOrientation(expectedOrientation)
//             .SetColorSpace(expectedColorSpace)
//             .SetShotAt(expectedShotAt)
//             .SetHasHdr(expectedHasHdr)
//             .SetIsPanorama(expectedIsPanorama)
//             .SetArtist(expectedArtist)
//             .Build();
//
//         Metadata metadata = result.Value!;
//
//         Assert.True(result.IsSuccess);
//         Assert.Equal(expectedMake, metadata.Make);
//         Assert.Equal(expectedModel, metadata.Model);
//         Assert.Equal(expectedLensModel, metadata.LensModel);
//         Assert.Equal(expectedFocalLength, metadata.FocalLength);
//         Assert.Equal(expectedAperture, metadata.Aperture);
//         Assert.Equal(expectedExposureTime, metadata.ExposureTime);
//         Assert.Equal(expectedIso, metadata.Iso);
//         Assert.Equal(expectedHasFlashfire, metadata.HasFlashfire);
//         Assert.Equal(expectedLatitude, metadata.Latitude);
//         Assert.Equal(expectedLongitude, metadata.Longitude);
//         Assert.Equal(expectedAltitude, metadata.Altitude);
//         Assert.Equal(expectedOrientation, metadata.Orientation);
//         Assert.Equal(expectedColorSpace, metadata.ColorSpace);
//         Assert.Equal(expectedShotAt, metadata.ShotAt);
//         Assert.Equal(expectedHasHdr, metadata.HasHdr);
//         Assert.Equal(expectedIsPanorama, metadata.IsPanorama);
//         Assert.Equal(expectedArtist, metadata.Artist);
//     }
//
//     [Fact]
//     public void Build_BuildingMetadataWithoutValues_ReturnsMetadata()
//     {
//         Metadata.Builder builder = new();
//
//         Metadata metadata = builder
//             .Build()
//             .Value!;
//
//         Assert.Null(metadata.Make);
//         Assert.Null(metadata.Model);
//         Assert.Null(metadata.LensModel);
//         Assert.Null(metadata.FocalLength);
//         Assert.Null(metadata.Aperture);
//         Assert.Null(metadata.ExposureTime);
//         Assert.Null(metadata.Iso);
//         Assert.Null(metadata.HasFlashfire);
//         Assert.Null(metadata.Latitude);
//         Assert.Null(metadata.Longitude);
//         Assert.Null(metadata.Altitude);
//         Assert.Null(metadata.Orientation);
//         Assert.Null(metadata.ColorSpace);
//         Assert.Null(metadata.ShotAt);
//         Assert.Null(metadata.HasHdr);
//         Assert.Null(metadata.IsPanorama);
//         Assert.Null(metadata.Artist);
//     }
//
//     [Fact]
//     public void Reset_ResettingMetadataToNewExample()
//     {
//         Metadata.Builder builder = new();
//
//         builder
//             .SetMake("Sony")
//             .SetModel("ILCE-7M3")
//             .SetLensModel("FE 24-70mm F2.8 GM")
//             .SetFocalLength(50.0)
//             .SetAperture(2.8)
//             .SetExposureTime("1/500")
//             .SetIso(100.0)
//             .SetHasFlashfire(false)
//             .SetLatitude(59.934280)
//             .SetLongitude(30.335098)
//             .SetAltitude(15.4)
//             .SetOrientation(Orientation.Horizontal)
//             .SetColorSpace("sRGB")
//             .SetShotAt(new DateTimeOffset(
//                 2026, 7, 15,
//                 14, 30, 0,
//                 TimeSpan.FromHours(3))
//             )
//             .SetHasHdr(false)
//             .SetIsPanorama(false)
//             .SetArtist("Ivan Ivanov");
//
//         builder.Reset();
//
//         var metadata = builder.Build().Value!;
//
//         Assert.Null(metadata.Make);
//         Assert.Null(metadata.Model);
//         Assert.Null(metadata.LensModel);
//         Assert.Null(metadata.FocalLength);
//         Assert.Null(metadata.Aperture);
//         Assert.Null(metadata.ExposureTime);
//         Assert.Null(metadata.Iso);
//         Assert.Null(metadata.HasFlashfire);
//         Assert.Null(metadata.Latitude);
//         Assert.Null(metadata.Longitude);
//         Assert.Null(metadata.Altitude);
//         Assert.Null(metadata.Orientation);
//         Assert.Null(metadata.ColorSpace);
//         Assert.Null(metadata.ShotAt);
//         Assert.Null(metadata.HasHdr);
//         Assert.Null(metadata.IsPanorama);
//         Assert.Null(metadata.Artist);
//     }
// }