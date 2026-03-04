#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1302

ENV DocsSettings__ServiceUrl=http://eHive-catalog-api:1302
ENV ProjectInfo__ProjectName='Catalog API'
ENV ProjectInfo__ProjectDescription='OnHive Catalog API'

ENV EnvironmentSettings__AdditionalConfigs__0=onhive-catalog-api


EXPOSE 1302

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Catalog.Api/OnHive.Catalog.Api.csproj", "OnHive.Catalog.Api/"]
COPY ["OnHive.Catalog.Domain/OnHive.Catalog.Domain.csproj", "OnHive.Catalog.Domain/"]
COPY ["OnHive.Catalog.Repositories/OnHive.Catalog.Repositories.csproj", "OnHive.Catalog.Repositories/"]
COPY ["OnHive.Catalog.Services/OnHive.Catalog.Services.csproj", "OnHive.Catalog.Services/"]
COPY ["OnHive.Catalog.Tests/OnHive.Catalog.Tests.csproj", "OnHive.Catalog.Tests/"]

RUN dotnet restore "OnHive.Catalog.Api/OnHive.Catalog.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Catalog.Api"
RUN dotnet build "OnHive.Catalog.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Catalog.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Catalog.Api.dll"]
