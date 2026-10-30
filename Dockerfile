# Etapa 1: Compilación y publicación del backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar archivos de proyectos para restaurar dependencias
COPY SGCAE.sln ./
COPY src/SGCAE.Domain/SGCAE.Domain.csproj src/SGCAE.Domain/
COPY src/SGCAE.Application/SGCAE.Application.csproj src/SGCAE.Application/
COPY src/SGCAE.Infrastructure/SGCAE.Infrastructure.csproj src/SGCAE.Infrastructure/
COPY src/SGCAE.WebApi/SGCAE.WebApi.csproj src/SGCAE.WebApi/
COPY tests/SGCAE.UnitTests/SGCAE.UnitTests.csproj tests/SGCAE.UnitTests/

RUN dotnet restore SGCAE.sln

# Copiar el resto del código fuente
COPY src/ src/
COPY tests/ tests/

# Compilar y publicar la Web API
WORKDIR /app/src/SGCAE.WebApi
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Imagen final de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SGCAE.WebApi.dll"]

