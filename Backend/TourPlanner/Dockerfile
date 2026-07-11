FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TourPlanner/TourPlanner.csproj", "TourPlanner/"]
COPY ["TourPlanner.API/TourPlanner.API.csproj", "TourPlanner.API/"]
COPY ["TourPlanner.Application/TourPlanner.Application.csproj", "TourPlanner.Application/"]
COPY ["TourPlanner.Application.Contracts/TourPlanner.Application.Contracts.csproj", "TourPlanner.Application.Contracts/"]
COPY ["TourPlanner.Domain/TourPlanner.Domain.csproj", "TourPlanner.Domain/"]
COPY ["TourPlanner.Infrastructure/TourPlanner.Infrastructure.csproj", "TourPlanner.Infrastructure/"]
RUN dotnet restore "TourPlanner/TourPlanner.csproj"
COPY . .
WORKDIR "/src/TourPlanner"
RUN dotnet build "./TourPlanner.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TourPlanner.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Pre-create the storage dir owned by the non-root app user so the first-time named-volume
# mount at /app/storage is seeded from this (writable) directory instead of a root-owned one.
RUN mkdir -p /app/storage
ENTRYPOINT ["dotnet", "TourPlanner.dll"]
