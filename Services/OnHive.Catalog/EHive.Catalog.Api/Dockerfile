#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1302

ENV DocsSettings__ServiceUrl=http://eHive-catalog-api:1302
ENV ProjectInfo__ProjectName='Catalog API'
ENV ProjectInfo__ProjectDescription='EHive Catalog API'

ENV EnvironmentSettings__AdditionalConfigs__0=ehive-catalog-api


EXPOSE 1302

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Catalog.Api/EHive.Catalog.Api.csproj", "EHive.Catalog.Api/"]
COPY ["EHive.Catalog.Domain/EHive.Catalog.Domain.csproj", "EHive.Catalog.Domain/"]
COPY ["EHive.Catalog.Repositories/EHive.Catalog.Repositories.csproj", "EHive.Catalog.Repositories/"]
COPY ["EHive.Catalog.Services/EHive.Catalog.Services.csproj", "EHive.Catalog.Services/"]
COPY ["EHive.Catalog.Tests/EHive.Catalog.Tests.csproj", "EHive.Catalog.Tests/"]

RUN dotnet restore "EHive.Catalog.Api/EHive.Catalog.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Catalog.Api"
RUN dotnet build "EHive.Catalog.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Catalog.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Catalog.Api.dll"]
