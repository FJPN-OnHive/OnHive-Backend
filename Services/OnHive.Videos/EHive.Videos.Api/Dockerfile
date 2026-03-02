#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1801

ENV VideosApiSettings__VideosAdminPermission=videos_admin

ENV DocsSettings__ServiceUrl=http://ehive-videos-api:1801
ENV ProjectInfo__ProjectName='Video API'
ENV ProjectInfo__ProjectDescription='EHive Video API'
ENV EnvironmentSettings__AdditionalConfigs__0=ehive-videos-api


EXPOSE 1801

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Videos.Api/EHive.Videos.Api.csproj", "EHive.Videos.Api/"]
COPY ["EHive.Videos.Domain/EHive.Videos.Domain.csproj", "EHive.Videos.Domain/"]
COPY ["EHive.Videos.Repositories/EHive.Videos.Repositories.csproj", "EHive.Videos.Repositories/"]
COPY ["EHive.Videos.Services/EHive.Videos.Services.csproj", "EHive.Videos.Services/"]
COPY ["EHive.Videos.Tests/EHive.Videos.Tests.csproj", "EHive.Videos.Tests/"]

RUN dotnet restore "EHive.Videos.Api/EHive.Videos.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Videos.Api"
RUN dotnet build "EHive.Videos.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Videos.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Videos.Api.dll"]
