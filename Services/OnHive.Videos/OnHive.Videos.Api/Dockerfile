#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1801

ENV VideosApiSettings__VideosAdminPermission=videos_admin

ENV DocsSettings__ServiceUrl=http://onhive-videos-api:1801
ENV ProjectInfo__ProjectName='Video API'
ENV ProjectInfo__ProjectDescription='OnHive Video API'
ENV EnvironmentSettings__AdditionalConfigs__0=onhive-videos-api


EXPOSE 1801

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Videos.Api/OnHive.Videos.Api.csproj", "OnHive.Videos.Api/"]
COPY ["OnHive.Videos.Domain/OnHive.Videos.Domain.csproj", "OnHive.Videos.Domain/"]
COPY ["OnHive.Videos.Repositories/OnHive.Videos.Repositories.csproj", "OnHive.Videos.Repositories/"]
COPY ["OnHive.Videos.Services/OnHive.Videos.Services.csproj", "OnHive.Videos.Services/"]
COPY ["OnHive.Videos.Tests/OnHive.Videos.Tests.csproj", "OnHive.Videos.Tests/"]

RUN dotnet restore "OnHive.Videos.Api/OnHive.Videos.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Videos.Api"
RUN dotnet build "OnHive.Videos.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Videos.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Videos.Api.dll"]
