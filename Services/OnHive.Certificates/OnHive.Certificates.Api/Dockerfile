#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1203

ENV CertificatesApiSettings__CertificatesAdminPermission=certificates_admin
ENV DocsSettings__ServiceUrl=http://onhive-certificates-api:1203
ENV ProjectInfo__ProjectName='Certificates API'
ENV ProjectInfo__ProjectDescription='OnHive certificates API'
ENV EnvironmentSettings__AdditionalConfigs__0=onhive-certificates-api

EXPOSE 1203

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Certificates.Api/OnHive.Certificates.Api.csproj", "OnHive.Certificates.Api/"]
COPY ["OnHive.Certificates.Domain/OnHive.Certificates.Domain.csproj", "OnHive.Certificates.Domain/"]
COPY ["OnHive.Certificates.Repositories/OnHive.Certificates.Repositories.csproj", "OnHive.Certificates.Repositories/"]
COPY ["OnHive.Certificates.Services/OnHive.Certificates.Services.csproj", "OnHive.Certificates.Services/"]
COPY ["OnHive.Certificates.Tests/OnHive.Certificates.Tests.csproj", "OnHive.Certificates.Tests/"]

RUN dotnet restore "OnHive.Certificates.Api/OnHive.Certificates.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Certificates.Api"
RUN dotnet build "OnHive.Certificates.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Certificates.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Certificates.Api.dll"]
