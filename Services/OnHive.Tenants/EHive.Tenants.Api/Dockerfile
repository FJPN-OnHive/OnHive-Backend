#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:1001

ENV DocsSettings__ServiceUrl=http://ehive-tenants-api:1001
ENV ProjectInfo__ProjectName='Tenants'
ENV ProjectInfo__ProjectDescription='OnHive Tenants Api'

EXPOSE 1001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Tenants.Api/EHive.Tenants.Api.csproj", "EHive.Tenants.Api/"]
COPY ["EHive.Tenants.Domain/EHive.Tenants.Domain.csproj", "EHive.Tenants.Domain/"]
COPY ["EHive.Tenants.Repositories/EHive.Tenants.Repositories.csproj", "EHive.Tenants.Repositories/"]
COPY ["EHive.Tenants.Services/EHive.Tenants.Services.csproj", "EHive.Tenants.Services/"]
COPY ["EHive.Tenants.Tests/EHive.Tenants.Tests.csproj", "EHive.Tenants.Tests/"]

RUN dotnet restore "EHive.Tenants.Api/EHive.Tenants.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Tenants.Api"
RUN dotnet build "EHive.Tenants.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Tenants.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Tenants.Api.dll"]
