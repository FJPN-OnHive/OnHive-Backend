#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:1002

ENV ProjectInfo__ProjectName=Users
ENV ProjectInfo__ProjectDescription='Users and Login service'

ENV UsersApiSettings__UserAdminPermission=users_admin
ENV DocsSettings__ServiceUrl=http://ehive-users-api:1002
ENV EnvironmentSettings__AdditionalConfigs__0=ehive-users-api
      
EXPOSE 1002

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Users.Api/EHive.Users.Api.csproj", "EHive.Users.Api/"]
COPY ["EHive.Users.Domain/EHive.Users.Domain.csproj", "EHive.Users.Domain/"]
COPY ["EHive.Users.Repositories/EHive.Users.Repositories.csproj", "EHive.Users.Repositories/"]
COPY ["EHive.Users.Services/EHive.Users.Services.csproj", "EHive.Users.Services/"]
COPY ["EHive.Users.Tests/EHive.Users.Tests.csproj", "EHive.Users.Tests/"]

RUN dotnet restore "EHive.Users.Api/EHive.Users.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Users.Api"
RUN dotnet build "EHive.Users.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Users.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Users.Api.dll"]
