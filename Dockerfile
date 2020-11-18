#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src

COPY Source/Service/Service.csproj Source/Service/
RUN dotnet restore Source/Service/Service.csproj 

COPY . .
WORKDIR /src/Source/Service
RUN dotnet build Service.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Service.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Service.dll"]