#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:1002

ENV ProjectInfo__ProjectName=Users
ENV ProjectInfo__ProjectDescription='Users and Login service'

ENV UsersApiSettings__UserAdminPermission=users_admin
ENV DocsSettings__ServiceUrl=http://onhive-users-api:1002
ENV EnvironmentSettings__AdditionalConfigs__0=onhive-users-api
      
EXPOSE 1002

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Users.Api/OnHive.Users.Api.csproj", "OnHive.Users.Api/"]
COPY ["OnHive.Users.Domain/OnHive.Users.Domain.csproj", "OnHive.Users.Domain/"]
COPY ["OnHive.Users.Repositories/OnHive.Users.Repositories.csproj", "OnHive.Users.Repositories/"]
COPY ["OnHive.Users.Services/OnHive.Users.Services.csproj", "OnHive.Users.Services/"]
COPY ["OnHive.Users.Tests/OnHive.Users.Tests.csproj", "OnHive.Users.Tests/"]

RUN dotnet restore "OnHive.Users.Api/OnHive.Users.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Users.Api"
RUN dotnet build "OnHive.Users.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Users.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Users.Api.dll"]
