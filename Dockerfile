FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

RUN mkdir -p /app/data && chown -R app:app /app/data

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Packages.props ./

COPY src/PhotoCatalog.Domain/PhotoCatalog.Domain.csproj src/PhotoCatalog.Domain/
COPY src/PhotoCatalog.Application/PhotoCatalog.Application.csproj src/PhotoCatalog.Application/
COPY src/PhotoCatalog.Infrastructure/PhotoCatalog.Infrastructure.csproj src/PhotoCatalog.Infrastructure/
COPY src/PhotoCatalog.WebApi/PhotoCatalog.WebApi.csproj src/PhotoCatalog.WebApi/

RUN dotnet restore src/PhotoCatalog.WebApi/PhotoCatalog.WebApi.csproj

COPY src/ ./src
WORKDIR "/src/src/PhotoCatalog.WebApi"
RUN dotnet build "PhotoCatalog.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhotoCatalog.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/photocatalog.db"
ENTRYPOINT ["dotnet", "PhotoCatalog.WebApi.dll"]