#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1011

ENV DocsSettings__ServiceUrl=http://ehive-redirect-api:1011
ENV ProjectInfo__ProjectName='Redirects API'
ENV ProjectInfo__ProjectDescription='EHive Redirects API'


EXPOSE 1011

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Redirects.Api/EHive.Redirects.Api.csproj", "EHive.Redirects.Api/"]
COPY ["EHive.Redirects.Domain/EHive.Redirects.Domain.csproj", "EHive.Redirects.Domain/"]
COPY ["EHive.Redirects.Repositories/EHive.Redirects.Repositories.csproj", "EHive.Redirects.Repositories/"]
COPY ["EHive.Redirects.Services/EHive.Redirects.Services.csproj", "EHive.Redirects.Services/"]
COPY ["EHive.Redirects.Tests/EHive.Redirects.Tests.csproj", "EHive.Redirects.Tests/"]

RUN dotnet restore "EHive.Redirects.Api/EHive.Redirects.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Redirects.Api"
RUN dotnet build "EHive.Redirects.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Redirects.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Redirects.Api.dll"]
