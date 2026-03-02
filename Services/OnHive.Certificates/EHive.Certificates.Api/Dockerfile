#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1203

ENV CertificatesApiSettings__CertificatesAdminPermission=certificates_admin
ENV DocsSettings__ServiceUrl=http://ehive-certificates-api:1203
ENV ProjectInfo__ProjectName='Certificates API'
ENV ProjectInfo__ProjectDescription='OnHive certificates API'
ENV EnvironmentSettings__AdditionalConfigs__0=ehive-certificates-api

EXPOSE 1203

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Certificates.Api/EHive.Certificates.Api.csproj", "EHive.Certificates.Api/"]
COPY ["EHive.Certificates.Domain/EHive.Certificates.Domain.csproj", "EHive.Certificates.Domain/"]
COPY ["EHive.Certificates.Repositories/EHive.Certificates.Repositories.csproj", "EHive.Certificates.Repositories/"]
COPY ["EHive.Certificates.Services/EHive.Certificates.Services.csproj", "EHive.Certificates.Services/"]
COPY ["EHive.Certificates.Tests/EHive.Certificates.Tests.csproj", "EHive.Certificates.Tests/"]

RUN dotnet restore "EHive.Certificates.Api/EHive.Certificates.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Certificates.Api"
RUN dotnet build "EHive.Certificates.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Certificates.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Certificates.Api.dll"]
