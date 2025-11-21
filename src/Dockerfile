FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src


COPY src/Tbilink-BE.WebApi/*.csproj ./src/Tbilink-BE.WebApi/
COPY src/Tbilink-BE.Application/*.csproj ./src/Tbilink-BE.Application/
COPY src/Tbilink-BE.Domain/*.csproj ./src/Tbilink-BE.Domain/
COPY src/Tbilink-BE.Infrastructure/*.csproj ./src/Tbilink-BE.Infrastructure/

RUN dotnet restore ./src/Tbilink-BE.WebApi/Tbilink-BE.WebApi.csproj

COPY . .
RUN dotnet publish ./src/Tbilink-BE.WebApi/Tbilink-BE.WebApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Tbilink-BE.WebApi.dll"]