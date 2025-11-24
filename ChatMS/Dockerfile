# ===== STAGE 1: build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos solo los csproj de todos los proyectos para el restore óptimo
COPY Application/*.csproj Application/
COPY Infrastructure/*.csproj Infrastructure/
COPY Domain/*.csproj Domain/
COPY ChatMS/*.csproj ChatMS/

# Restauramos desde la solución real
COPY *.sln ./
RUN dotnet restore

# Ahora copiamos TODO el código
COPY . .

# Publicamos el microservicio
WORKDIR /src/ChatMS
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false


# ===== STAGE 2: runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ChatMS.dll"]
