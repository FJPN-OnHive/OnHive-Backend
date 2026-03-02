#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

ENV EnvironmentSettings__EnvironmentType=1
ENV ASPNETCORE_URLS=http://+:1000
ENV SystemParametersApiSettings__SystemParametersAdminPermission=systemParameters_admin
ENV DocsSettings__ServiceUrl=http://ehive-systemparameters-api:1000
ENV ProjectInfo__ProjectName='System Parameters'
ENV ProjectInfo__ProjectDescription='OnHive system parameters'

EXPOSE 1000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.SystemParameters.Api/EHive.SystemParameters.Api.csproj", "EHive.SystemParameters.Api/"]
COPY ["EHive.SystemParameters.Domain/EHive.SystemParameters.Domain.csproj", "EHive.SystemParameters.Domain/"]
COPY ["EHive.SystemParameters.Repositories/EHive.SystemParameters.Repositories.csproj", "EHive.SystemParameters.Repositories/"]
COPY ["EHive.SystemParameters.Services/EHive.SystemParameters.Services.csproj", "EHive.SystemParameters.Services/"]
COPY ["EHive.SystemParameters.Tests/EHive.SystemParameters.Tests.csproj", "EHive.SystemParameters.Tests/"]

RUN dotnet restore "EHive.SystemParameters.Api/EHive.SystemParameters.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.SystemParameters.Api"
RUN dotnet build "EHive.SystemParameters.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.SystemParameters.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.SystemParameters.Api.dll"]
