FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY ["API/VehicleProfitTracker.API.csproj", "API/"]

# Restore dependencies
RUN dotnet restore "API/VehicleProfitTracker.API.csproj"

# Copy entire source
COPY . .

# Publish
WORKDIR "/src/API"
RUN dotnet publish "VehicleProfitTracker.API.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Set environment
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD dotnet --version || exit 1

# Expose and run
EXPOSE 8080
ENTRYPOINT ["dotnet", "VehicleProfitTracker.API.dll"]
