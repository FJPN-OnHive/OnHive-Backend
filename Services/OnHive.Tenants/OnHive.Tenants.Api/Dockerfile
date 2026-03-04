#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:1001

ENV DocsSettings__ServiceUrl=http://onhive-tenants-api:1001
ENV ProjectInfo__ProjectName='Tenants'
ENV ProjectInfo__ProjectDescription='OnHive Tenants Api'

EXPOSE 1001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Tenants.Api/OnHive.Tenants.Api.csproj", "OnHive.Tenants.Api/"]
COPY ["OnHive.Tenants.Domain/OnHive.Tenants.Domain.csproj", "OnHive.Tenants.Domain/"]
COPY ["OnHive.Tenants.Repositories/OnHive.Tenants.Repositories.csproj", "OnHive.Tenants.Repositories/"]
COPY ["OnHive.Tenants.Services/OnHive.Tenants.Services.csproj", "OnHive.Tenants.Services/"]
COPY ["OnHive.Tenants.Tests/OnHive.Tenants.Tests.csproj", "OnHive.Tenants.Tests/"]

RUN dotnet restore "OnHive.Tenants.Api/OnHive.Tenants.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Tenants.Api"
RUN dotnet build "OnHive.Tenants.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Tenants.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Tenants.Api.dll"]
