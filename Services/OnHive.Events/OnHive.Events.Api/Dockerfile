#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1010

ENV EventsApiSettings__EventsAdminPermission=events_admin

ENV EnvironmentSettings__AdditionalConfigs__0=onhive-events-api

ENV DocsSettings__ServiceUrl=http://onhive-events-api:1010
ENV ProjectInfo__ProjectName='EventRegister API'
ENV ProjectInfo__ProjectDescription='OnHive EventRegister API'


EXPOSE 1010

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Events.Api/OnHive.Events.Api.csproj", "OnHive.Events.Api/"]
COPY ["OnHive.Events.Domain/OnHive.Events.Domain.csproj", "OnHive.Events.Domain/"]
COPY ["OnHive.Events.Repositories/OnHive.Events.Repositories.csproj", "OnHive.Events.Repositories/"]
COPY ["OnHive.Events.Services/OnHive.Events.Services.csproj", "OnHive.Events.Services/"]
COPY ["OnHive.Events.Tests/OnHive.Events.Tests.csproj", "OnHive.Events.Tests/"]

RUN dotnet restore "OnHive.Events.Api/OnHive.Events.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Events.Api"
RUN dotnet build "OnHive.Events.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Events.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Events.Api.dll"]
