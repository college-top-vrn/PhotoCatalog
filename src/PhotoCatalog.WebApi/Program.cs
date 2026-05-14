using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


const string version = "v1";
const string name = "PhotoCatalog";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();

    app.UseSwaggerUI(swagg =>
    {
        swagg.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{name}");
    });
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/summaries", () =>
{
    if (summaries.Length == 0)
    {
        return Results.NoContent();
    }

    return Results.Ok(summaries);
});

app.MapHealthChecks("/health");
app.Run();