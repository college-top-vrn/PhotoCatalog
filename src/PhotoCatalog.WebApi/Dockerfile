FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/PhotoCatalog.WebApi/PhotoCatalog.WebApi.csproj", "src/PhotoCatalog.WebApi/"]
COPY ["src/PhotoCatalog.Application/PhotoCatalog.Application.csproj", "src/PhotoCatalog.Application/"]
COPY ["src/PhotoCatalog.Domain/PhotoCatalog.Domain.csproj", "src/PhotoCatalog.Domain/"]
COPY ["src/PhotoCatalog.Infrastructure/PhotoCatalog.Infrastructure.csproj", "src/PhotoCatalog.Infrastructure/"]
RUN dotnet restore "src/PhotoCatalog.WebApi/PhotoCatalog.WebApi.csproj"
COPY . .
WORKDIR "/src/src/PhotoCatalog.WebApi"
RUN dotnet build "./PhotoCatalog.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PhotoCatalog.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhotoCatalog.WebApi.dll"]
