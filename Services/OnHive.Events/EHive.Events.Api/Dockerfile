#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1010

ENV EventsApiSettings__EventsAdminPermission=events_admin

ENV EnvironmentSettings__AdditionalConfigs__0=ehive-events-api

ENV DocsSettings__ServiceUrl=http://ehive-events-api:1010
ENV ProjectInfo__ProjectName='EventRegister API'
ENV ProjectInfo__ProjectDescription='EHive EventRegister API'


EXPOSE 1010

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Events.Api/EHive.Events.Api.csproj", "EHive.Events.Api/"]
COPY ["EHive.Events.Domain/EHive.Events.Domain.csproj", "EHive.Events.Domain/"]
COPY ["EHive.Events.Repositories/EHive.Events.Repositories.csproj", "EHive.Events.Repositories/"]
COPY ["EHive.Events.Services/EHive.Events.Services.csproj", "EHive.Events.Services/"]
COPY ["EHive.Events.Tests/EHive.Events.Tests.csproj", "EHive.Events.Tests/"]

RUN dotnet restore "EHive.Events.Api/EHive.Events.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Events.Api"
RUN dotnet build "EHive.Events.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Events.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Events.Api.dll"]
