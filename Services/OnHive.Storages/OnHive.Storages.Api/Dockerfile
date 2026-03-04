#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1800

ENV StoragesApiSettings__StoragesAdminPermission=storages_admin

ENV DocsSettings__ServiceUrl=http://onhive-storages-api:1800
ENV ProjectInfo__ProjectName='Storage API'
ENV ProjectInfo__ProjectDescription='OnHive Storage API'
ENV EnvironmentSettings__AdditionalConfigs__0=onhive-storages-api


EXPOSE 1800

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Storages.Api/OnHive.Storages.Api.csproj", "OnHive.Storages.Api/"]
COPY ["OnHive.Storages.Domain/OnHive.Storages.Domain.csproj", "OnHive.Storages.Domain/"]
COPY ["OnHive.Storages.Repositories/OnHive.Storages.Repositories.csproj", "OnHive.Storages.Repositories/"]
COPY ["OnHive.Storages.Services/OnHive.Storages.Services.csproj", "OnHive.Storages.Services/"]
COPY ["OnHive.Storages.Tests/OnHive.Storages.Tests.csproj", "OnHive.Storages.Tests/"]

RUN dotnet restore "OnHive.Storages.Api/OnHive.Storages.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Storages.Api"
RUN dotnet build "OnHive.Storages.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Storages.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Storages.Api.dll"]
