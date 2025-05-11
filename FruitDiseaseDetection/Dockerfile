FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project files and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all files and build the project
COPY . ./
RUN dotnet publish -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Update the service URL to use the docker-compose service name
ENV PREDICTION_API_URL="http://model-api:8000"

# Expose the backend port
EXPOSE 80

# Start the application
ENTRYPOINT ["dotnet", "FruitDiseaseDetection.dll"]