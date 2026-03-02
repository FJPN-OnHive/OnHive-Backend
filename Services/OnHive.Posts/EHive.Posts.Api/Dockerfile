#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1901

ENV PostsApiSettings__PostsAdminPermission=posts_admin

ENV DocsSettings__ServiceUrl=http://ehive-posts-api:1901
ENV ProjectInfo__ProjectName='Posts API'
ENV ProjectInfo__ProjectDescription='EHive Posts API'

ENV EnvironmentSettings__AdditionalConfigs__0=ehive-posts-api

EXPOSE 1901

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Posts.Api/EHive.Posts.Api.csproj", "EHive.Posts.Api/"]
COPY ["EHive.Posts.Domain/EHive.Posts.Domain.csproj", "EHive.Posts.Domain/"]
COPY ["EHive.Posts.Repositories/EHive.Posts.Repositories.csproj", "EHive.Posts.Repositories/"]
COPY ["EHive.Posts.Services/EHive.Posts.Services.csproj", "EHive.Posts.Services/"]
COPY ["EHive.Posts.Tests/EHive.Posts.Tests.csproj", "EHive.Posts.Tests/"]

RUN dotnet restore "EHive.Posts.Api/EHive.Posts.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Posts.Api"
RUN dotnet build "EHive.Posts.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Posts.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Posts.Api.dll"]
