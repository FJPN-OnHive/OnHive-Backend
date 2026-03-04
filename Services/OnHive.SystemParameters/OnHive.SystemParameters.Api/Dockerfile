#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

ENV EnvironmentSettings__EnvironmentType=1
ENV ASPNETCORE_URLS=http://+:1000
ENV SystemParametersApiSettings__SystemParametersAdminPermission=systemParameters_admin
ENV DocsSettings__ServiceUrl=http://onhive-systemparameters-api:1000
ENV ProjectInfo__ProjectName='System Parameters'
ENV ProjectInfo__ProjectDescription='OnHive system parameters'

EXPOSE 1000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.SystemParameters.Api/OnHive.SystemParameters.Api.csproj", "OnHive.SystemParameters.Api/"]
COPY ["OnHive.SystemParameters.Domain/OnHive.SystemParameters.Domain.csproj", "OnHive.SystemParameters.Domain/"]
COPY ["OnHive.SystemParameters.Repositories/OnHive.SystemParameters.Repositories.csproj", "OnHive.SystemParameters.Repositories/"]
COPY ["OnHive.SystemParameters.Services/OnHive.SystemParameters.Services.csproj", "OnHive.SystemParameters.Services/"]
COPY ["OnHive.SystemParameters.Tests/OnHive.SystemParameters.Tests.csproj", "OnHive.SystemParameters.Tests/"]

RUN dotnet restore "OnHive.SystemParameters.Api/OnHive.SystemParameters.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.SystemParameters.Api"
RUN dotnet build "OnHive.SystemParameters.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.SystemParameters.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.SystemParameters.Api.dll"]
