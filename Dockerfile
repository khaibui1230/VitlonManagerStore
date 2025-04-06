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

# Create startup script
RUN echo '#!/bin/bash \n\
echo "Starting application with database handling..." \n\
echo "Waiting for database to become available..." \n\
sleep 5 \n\
echo "Starting application..." \n\
dotnet QuanVitLonManager.dll \n\
' > /app/startup.sh && chmod +x /app/startup.sh

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the startup script
ENTRYPOINT ["/bin/bash", "/app/startup.sh"] 