# Build de producción del backend de Corela15 (ASP.NET Core 8).
# Contexto de build: backend/ (este mismo directorio, donde vive Corela15.sln).
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Corela15.Domain/Corela15.Domain.csproj src/Corela15.Domain/
COPY src/Corela15.Application/Corela15.Application.csproj src/Corela15.Application/
COPY src/Corela15.Infrastructure/Corela15.Infrastructure.csproj src/Corela15.Infrastructure/
COPY src/Corela15.Api/Corela15.Api.csproj src/Corela15.Api/
RUN dotnet restore src/Corela15.Api/Corela15.Api.csproj

COPY src/Corela15.Domain/ src/Corela15.Domain/
COPY src/Corela15.Application/ src/Corela15.Application/
COPY src/Corela15.Infrastructure/ src/Corela15.Infrastructure/
COPY src/Corela15.Api/ src/Corela15.Api/
RUN dotnet publish src/Corela15.Api/Corela15.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# El puerto se fija acá (nunca en ASPNETCORE_URLS del .env) -- el reverse
# proxy (frontend/nginx) es el único punto expuesto a internet.
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Corela15.Api.dll"]
