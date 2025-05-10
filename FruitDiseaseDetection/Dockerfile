# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Proje dosyalarını kopyala ve derle
COPY . ./
RUN dotnet publish -c Release -o out

# Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "FruitDiseaseDetection.dll"]
