FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["InventoryPack.csproj", "./"]
RUN dotnet restore "InventoryPack.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet format "./InventoryPack.csproj" --no-restore --verify-no-changes --severity warn
RUN dotnet build "./InventoryPack.csproj" --no-restore -c $BUILD_CONFIGURATION -o /app/build --warnaserror

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./InventoryPack.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InventoryPack.dll"]
