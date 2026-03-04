#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1005

ENV SearchApiSettings__SearchAdminPermission=search_admin

ENV DocsSettings__ServiceUrl=http://onhive-search-api:1005
ENV ProjectInfo__ProjectName='SearchResult API'
ENV ProjectInfo__ProjectDescription='OnHive SearchResult API'

ENV EnvironmentSettings__AdditionalConfigs__0=onhive-search-api

EXPOSE 1005

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Search.Api/OnHive.Search.Api.csproj", "OnHive.Search.Api/"]
COPY ["OnHive.Search.Domain/OnHive.Search.Domain.csproj", "OnHive.Search.Domain/"]
COPY ["OnHive.Search.Repositories/OnHive.Search.Repositories.csproj", "OnHive.Search.Repositories/"]
COPY ["OnHive.Search.Services/OnHive.Search.Services.csproj", "OnHive.Search.Services/"]
COPY ["OnHive.Search.Tests/OnHive.Search.Tests.csproj", "OnHive.Search.Tests/"]

RUN dotnet restore "OnHive.Search.Api/OnHive.Search.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Search.Api"
RUN dotnet build "OnHive.Search.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Search.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Search.Api.dll"]
