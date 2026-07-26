# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy repository source files
COPY . .

# Dynamic CSPROJ resolution & publish
RUN CSPROJ=$(find . -name "ScreenWorking.Server.API.csproj" | head -n 1) && \
    echo "Building project: $CSPROJ" && \
    dotnet restore "$CSPROJ" && \
    dotnet publish "$CSPROJ" -c Release -o /app/out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "ScreenWorking.Server.API.dll"]
