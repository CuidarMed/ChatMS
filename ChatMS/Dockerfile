# =========================
#  Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ChatMS/ChatMS.csproj ChatMS/
COPY Application/Application.csproj Application/
COPY Domain/Domain.csproj Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/

RUN dotnet restore ChatMS/ChatMS.csproj

# Copiar el resto del código
COPY . .

# Compilar
WORKDIR /src/ChatMS
RUN dotnet publish ChatMS.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# =========================
#  Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish ./

EXPOSE 8080
ENTRYPOINT ["dotnet", "ChatMS.dll"]

