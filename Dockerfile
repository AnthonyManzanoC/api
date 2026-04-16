# 1. Usar la imagen oficial de .NET 9 para compilar
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 2. Copiar los archivos de configuración de los proyectos
COPY ["LamillaEscudero.Api/LamillaEscudero.Api.csproj", "LamillaEscudero.Api/"]
COPY ["LamillaEscudero.Application/LamillaEscudero.Application.csproj", "LamillaEscudero.Application/"]
COPY ["LamillaEscudero.Domain/LamillaEscudero.Domain.csproj", "LamillaEscudero.Domain/"]
COPY ["LamillaEscudero.Infrastructure/LamillaEscudero.Infrastructure.csproj", "LamillaEscudero.Infrastructure/"]

# 3. Restaurar las dependencias
RUN dotnet restore "LamillaEscudero.Api/LamillaEscudero.Api.csproj"

# 4. Copiar el resto del código y compilar
COPY . .
WORKDIR "/src/LamillaEscudero.Api"
RUN dotnet publish "LamillaEscudero.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 5. Configurar el servidor final que correrá tu API
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LamillaEscudero.Api.dll"]