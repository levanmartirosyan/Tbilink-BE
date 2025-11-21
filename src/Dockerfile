FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Tbilink-BE.WebApi/*.csproj ./Tbilink-BE.WebApi/
COPY Tbilink-BE.Application/*.csproj ./Tbilink-BE.Application/
COPY Tbilink-BE.Domain/*.csproj ./Tbilink-BE.Domain/
COPY Tbilink-BE.Infrastructure/*.csproj ./Tbilink-BE.Infrastructure/

RUN dotnet restore ./Tbilink-BE.WebApi/Tbilink-BE.WebApi.csproj

COPY . .
RUN dotnet publish ./Tbilink-BE.WebApi/Tbilink-BE.WebApi.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "Tbilink-BE.WebApi.dll"]
