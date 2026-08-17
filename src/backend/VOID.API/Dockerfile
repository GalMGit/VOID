FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/backend/VOID.API/VOID.API.csproj", "src/backend/VOID.API/"]
COPY ["src/shared/VOID.Shared.Contracts/VOID.Shared.Contracts.csproj", "src/shared/VOID.Shared.Contracts/"]
COPY ["src/backend/VOID.Application/VOID.Application.csproj", "src/backend/VOID.Application/"]
COPY ["src/backend/VOID.Domain/VOID.Domain.csproj", "src/backend/VOID.Domain/"]
COPY ["src/backend/VOID.Infrastructure/VOID.Infrastructure.csproj", "src/backend/VOID.Infrastructure/"]
COPY ["src/backend/VOID.Persistence/VOID.Persistence.csproj", "src/backend/VOID.Persistence/"]
RUN dotnet restore "src/backend/VOID.API/VOID.API.csproj"
COPY . .
WORKDIR "/src/src/backend/VOID.API"
RUN dotnet build "./VOID.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./VOID.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VOID.API.dll"]
