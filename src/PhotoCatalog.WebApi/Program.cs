using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PhotoCatalog.Application.DTOs;
using PhotoCatalog.Application.Fakes;
using PhotoCatalog.Application.UseCases;
using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Interfaces.Services;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Extensions;
using PhotoCatalog.Infrastructure.Fakes;

using Serilog;

try
{
    const string version = "v1";
    const string name = "PhotoCatalog";

    Log.Information("Запуск веб-хоста...");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateBootstrapLogger();

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddHealthChecks();

    builder.Services.AddSingleton<IFolderRepository, FakeFolderRepository>();
    builder.Services.AddSingleton<IPhotoCommandRepository, FakePhotoCommandRepository>();
    builder.Services.AddSingleton<ITagQueryRepository, FakeTagQueryRepository>();
    builder.Services.AddSingleton<ITagCommandRepository, FakeTagCommandRepository>();
    builder.Services.AddSingleton<IAlbumQueryRepository, FakeAlbumQueryRepository>();
    builder.Services.AddSingleton<IAlbumCommandRepository, FakeAlbumCommandRepository>();

    builder.Services.AddSingleton<IFileStorage, FakeFileStorage>();
    builder.Services.AddSingleton<IFileMetadataExtractor, FakeFileMetadataExtractor>();
    builder.Services.AddSingleton<IFolderHierarchyValidator, FakeFolderHierarchyValidator>();
    builder.Services.AddSingleton<IUnitOfWork, FakeUnitOfWork>();

    builder.Services.AddTransient<CreateFolderUseCase>();
    builder.Services.AddTransient<DeletePhotoUseCase>();
    builder.Services.AddTransient<AddTagToPhotoUseCase>();
    builder.Services.AddTransient<MoveFolderUseCase>();
    builder.Services.AddTransient<ImportPhotoUseCase>();
    builder.Services.AddTransient<AddPhotoToAlbumUseCase>();

    WebApplication app = builder.Build();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();

        app.UseSwaggerUI(swagg =>
        {
            swagg.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{name}");
        });
    }

    app.MapGet("/test", () => "Hello World!");

    app.MapGroup("/api/tags");

    app.MapGet("/{id}", (int id) =>
    {
        FakeTagQueryRepository repository = new();
        Result<Tag> tag = repository.GetById(id);
        if (tag.IsSuccess)
        {
            return Results.Ok(tag);
        }

        return Results.NotFound();
    });

    app.MapPost("/{name}", (string name) =>
    {
        FakeTagQueryRepository repository = new();
        Result<Tag> tag = repository.GetByName(name);
        if (tag.IsSuccess)
        {
            return Results.Ok(tag);
        }

        return Results.NotFound();
    });

    app.MapDelete("/{id}", (int id) =>
    {
        FakeTagCommandRepository repository = new();
        ResultVoid tag = repository.Delete(id);
        if (tag.IsSuccess)
        {
            return Results.Ok(tag);
        }

        return Results.NotFound();
    });


    app.MapHealthChecks("/health");

    RouteGroupBuilder albumEndpointsGroup = app.MapGroup("/api/albums").WithTags("Альбомы");

    albumEndpointsGroup.MapGet("/{folderId:int}/albums", (int folderId, IAlbumQueryRepository albumQueryRepository) =>
        albumQueryRepository
            .GetByFolderId(folderId)
            .ToHttpResult());

    albumEndpointsGroup.MapPost("/", (AlbumResponse album, IAlbumCommandRepository albumCommandRepository) => albumCommandRepository
        .Add(Album.Create(album.Name, album.Id).Value!)
        .ToHttpResult());
    // TODO исправить код
    // albumEndpointsGroup.MapPost("/{albumId:int}/photos/{photoId:int}",
    //     (int albumId, int photoId, IAlbumQueryRepository albumQueryRepository, IPhotoRepository photoRepository) =>
    //     {
    //         Result<Photo> searchResult = photoRepository.GetById(photoId);
    //
    //         if (searchResult.IsFailure)
    //         {
    //             return searchResult.Error.ToHttpResult();
    //         }
    //
    //         return albumQueryRepository
    //             .AddPhoto(albumId, photoId)
    //             .ToHttpResult();
    //     });
    //
    // albumEndpointsGroup.MapDelete("/{albumId:int}/photos/{photoId:int}",
    //     (int albumId, int photoId, IAlbumQueryRepository albumQueryRepository, IPhotoRepository photoRepository) =>
    //     {
    //         Result<Photo> searchResult = photoRepository.GetById(photoId);
    //
    //         if (searchResult.IsFailure)
    //         {
    //             return searchResult.Error.ToHttpResult();
    //         }
    //
    //         return albumQueryRepository
    //             .DeletePhoto(albumId, photoId)
    //             .ToHttpResult();
    //     });
    //
    // albumEndpointsGroup.MapDelete("/{id:int}",
    //     (int id, IAlbumCommandRepository albumCommandRepository) => albumCommandRepository.Delete(id).ToHttpResult());

    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Необработанное исключение.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}