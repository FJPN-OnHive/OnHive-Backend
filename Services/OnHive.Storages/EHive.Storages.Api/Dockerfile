#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1800

ENV StoragesApiSettings__StoragesAdminPermission=storages_admin

ENV DocsSettings__ServiceUrl=http://ehive-storages-api:1800
ENV ProjectInfo__ProjectName='Storage API'
ENV ProjectInfo__ProjectDescription='EHive Storage API'
ENV EnvironmentSettings__AdditionalConfigs__0=ehive-storages-api


EXPOSE 1800

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Storages.Api/EHive.Storages.Api.csproj", "EHive.Storages.Api/"]
COPY ["EHive.Storages.Domain/EHive.Storages.Domain.csproj", "EHive.Storages.Domain/"]
COPY ["EHive.Storages.Repositories/EHive.Storages.Repositories.csproj", "EHive.Storages.Repositories/"]
COPY ["EHive.Storages.Services/EHive.Storages.Services.csproj", "EHive.Storages.Services/"]
COPY ["EHive.Storages.Tests/EHive.Storages.Tests.csproj", "EHive.Storages.Tests/"]

RUN dotnet restore "EHive.Storages.Api/EHive.Storages.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Storages.Api"
RUN dotnet build "EHive.Storages.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Storages.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Storages.Api.dll"]
