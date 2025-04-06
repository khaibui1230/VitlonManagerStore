# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["QuanVitLonManager.csproj", "."]
RUN dotnet restore "./QuanVitLonManager.csproj"
COPY . .
RUN dotnet build "QuanVitLonManager.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "QuanVitLonManager.csproj" -c Release -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "QuanVitLonManager.dll"] 