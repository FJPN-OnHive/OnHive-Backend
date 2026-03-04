#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1901

ENV PostsApiSettings__PostsAdminPermission=posts_admin

ENV DocsSettings__ServiceUrl=http://onhive-posts-api:1901
ENV ProjectInfo__ProjectName='Posts API'
ENV ProjectInfo__ProjectDescription='OnHive Posts API'

ENV EnvironmentSettings__AdditionalConfigs__0=onhive-posts-api

EXPOSE 1901

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Posts.Api/OnHive.Posts.Api.csproj", "OnHive.Posts.Api/"]
COPY ["OnHive.Posts.Domain/OnHive.Posts.Domain.csproj", "OnHive.Posts.Domain/"]
COPY ["OnHive.Posts.Repositories/OnHive.Posts.Repositories.csproj", "OnHive.Posts.Repositories/"]
COPY ["OnHive.Posts.Services/OnHive.Posts.Services.csproj", "OnHive.Posts.Services/"]
COPY ["OnHive.Posts.Tests/OnHive.Posts.Tests.csproj", "OnHive.Posts.Tests/"]

RUN dotnet restore "OnHive.Posts.Api/OnHive.Posts.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Posts.Api"
RUN dotnet build "OnHive.Posts.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Posts.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Posts.Api.dll"]
