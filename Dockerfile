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

# Install PostgreSQL client for database operations
RUN apt-get update && apt-get install -y postgresql-client

# Create startup script with PostgreSQL initialization
RUN echo '#!/bin/bash \n\
echo "Starting application with database initialization..." \n\
# Extract connection information from DATABASE_URL \n\
if [[ $DATABASE_URL == postgres://* || $DATABASE_URL == postgresql://* ]]; then \n\
  # Parse the URL into components \n\
  userpass=$(echo $DATABASE_URL | cut -d@ -f1 | cut -d/ -f3-) \n\
  userpass=${userpass//:/ } # Split into username and password \n\
  user=$(echo $userpass | cut -d" " -f1) \n\
  pass=$(echo $userpass | cut -d" " -f2) \n\
  hostport=$(echo $DATABASE_URL | cut -d@ -f2 | cut -d/ -f1) \n\
  hostport=${hostport//:/ } # Split into host and port \n\
  host=$(echo $hostport | cut -d" " -f1) \n\
  port=$(echo $hostport | cut -d" " -f2) \n\
  db=$(echo $DATABASE_URL | cut -d/ -f4) \n\
  \n\
  echo "Waiting for PostgreSQL to become available..." \n\
  # Wait for the database to be ready \n\
  for i in {1..30}; do \n\
    pg_isready -h $host -p $port -U $user && break \n\
    echo "PostgreSQL not ready yet (attempt $i/30). Waiting..." \n\
    sleep 2 \n\
  done \n\
  \n\
  echo "Running PostgreSQL setup script..." \n\
  # Run our custom migration script \n\
  PGPASSWORD=$pass psql -h $host -p $port -U $user -d $db -f /app/Migrations/PostgresCompatibilityScript.sql || echo "Warning: Migration script returned non-zero exit code" \n\
  \n\
else \n\
  echo "DATABASE_URL not in PostgreSQL format, skipping database initialization" \n\
fi \n\
\n\
echo "Starting application..." \n\
dotnet QuanVitLonManager.dll \n\
' > /app/startup.sh && chmod +x /app/startup.sh

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the startup script
ENTRYPOINT ["/bin/bash", "/app/startup.sh"] 