# --- Этап сборки ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем файл решения и проектный файл
COPY ["*.sln", "./"]
COPY ["MobileServiceSite/MobileServiceSite.csproj", "MobileServiceSite/"]

RUN dotnet restore

# Копируем весь остальной код
COPY . .
WORKDIR "/src/MobileServiceSite"
RUN dotnet publish -c Release -o /app/publish

# --- Этап рантайма ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MobileServiceSite.dll"]