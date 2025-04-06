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
\n\
# Function to extract host and port from a connection string \n\
parse_postgres_url() { \n\
  # If the URL starts with postgres:// or postgresql://, try to parse it \n\
  if [[ $DATABASE_URL == postgres://* || $DATABASE_URL == postgresql://* ]]; then \n\
    # Extract credentials (username:password) \n\
    userpass=$(echo $DATABASE_URL | sed -E "s/^(postgresql|postgres):\/\///g" | cut -d@ -f1) \n\
    user=$(echo $userpass | cut -d: -f1) \n\
    pass=$(echo $userpass | cut -d: -f2) \n\
    \n\
    # Extract host, port and database name \n\
    hostportdb=$(echo $DATABASE_URL | sed -E "s/^(postgresql|postgres):\/\/[^@]+@//g") \n\
    # Handle different URL formats (with or without port) \n\
    if [[ $hostportdb == *":"*"/"* ]]; then \n\
      # Format is host:port/dbname \n\
      host=$(echo $hostportdb | cut -d: -f1) \n\
      port=$(echo $hostportdb | cut -d: -f2 | cut -d/ -f1) \n\
      db=$(echo $hostportdb | cut -d/ -f2) \n\
    else \n\
      # Format is host/dbname (default port 5432) \n\
      host=$(echo $hostportdb | cut -d/ -f1) \n\
      port="5432" \n\
      db=$(echo $hostportdb | cut -d/ -f2) \n\
    fi \n\
    \n\
    echo "Host: $host, Port: $port, DB: $db, User: $user" \n\
    return 0 \n\
  else \n\
    echo "DATABASE_URL not in PostgreSQL format" \n\
    return 1 \n\
  fi \n\
} \n\
\n\
# Main script starts here \n\
if [[ -n $DATABASE_URL ]]; then \n\
  echo "DATABASE_URL found, attempting to parse" \n\
  parse_postgres_url \n\
  if [[ $? -eq 0 ]]; then \n\
    echo "Waiting for PostgreSQL to become available..." \n\
    # Wait for the database \n\
    for i in {1..30}; do \n\
      pg_isready -h $host -p $port -U $user && break \n\
      echo "PostgreSQL not ready yet (attempt $i/30). Waiting..." \n\
      sleep 2 \n\
    done \n\
    \n\
    echo "Running PostgreSQL setup script..." \n\
    cat /app/Migrations/PostgresCompatibilityScript.sql \n\
    PGPASSWORD=$pass psql -h $host -p $port -U $user -d $db -v ON_ERROR_STOP=0 -f /app/Migrations/PostgresCompatibilityScript.sql \n\
    echo "SQL script execution completed with status: $?" \n\
  else \n\
    echo "Could not parse DATABASE_URL properly, skipping database initialization" \n\
  fi \n\
else \n\
  echo "No DATABASE_URL found, skipping database initialization" \n\
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