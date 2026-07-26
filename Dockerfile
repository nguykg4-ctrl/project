# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy CSProj and restore dependencies
COPY server/src/ScreenWorking.Server.API/*.csproj ./server/src/ScreenWorking.Server.API/
RUN dotnet restore ./server/src/ScreenWorking.Server.API/ScreenWorking.Server.API.csproj

# Copy source code and publish
COPY server/src/ScreenWorking.Server.API/ ./server/src/ScreenWorking.Server.API/
RUN dotnet publish ./server/src/ScreenWorking.Server.API/ScreenWorking.Server.API.csproj -c Release -o /app/out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "ScreenWorking.Server.API.dll"]
