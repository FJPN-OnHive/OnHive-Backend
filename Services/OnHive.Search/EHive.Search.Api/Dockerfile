#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1005

ENV SearchApiSettings__SearchAdminPermission=search_admin

ENV DocsSettings__ServiceUrl=http://ehive-search-api:1005
ENV ProjectInfo__ProjectName='SearchResult API'
ENV ProjectInfo__ProjectDescription='EHive SearchResult API'

ENV EnvironmentSettings__AdditionalConfigs__0=ehive-search-api

EXPOSE 1005

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Search.Api/EHive.Search.Api.csproj", "EHive.Search.Api/"]
COPY ["EHive.Search.Domain/EHive.Search.Domain.csproj", "EHive.Search.Domain/"]
COPY ["EHive.Search.Repositories/EHive.Search.Repositories.csproj", "EHive.Search.Repositories/"]
COPY ["EHive.Search.Services/EHive.Search.Services.csproj", "EHive.Search.Services/"]
COPY ["EHive.Search.Tests/EHive.Search.Tests.csproj", "EHive.Search.Tests/"]

RUN dotnet restore "EHive.Search.Api/EHive.Search.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Search.Api"
RUN dotnet build "EHive.Search.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Search.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Search.Api.dll"]
