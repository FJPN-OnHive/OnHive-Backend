#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:1003

ENV EnvironmentSettings__AdditionalConfigs__0=ehive-emails-api
ENV DocsSettings__ServiceUrl=http://ehive-emails-api:1003
ENV ProjectInfo__ProjectName='Emails Api'
ENV ProjectInfo__ProjectDescription='OnHive Emails Api'

EXPOSE 1003

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Emails.Api/EHive.Emails.Api.csproj", "EHive.Emails.Api/"]
COPY ["EHive.Emails.Domain/EHive.Emails.Domain.csproj", "EHive.Emails.Domain/"]
COPY ["EHive.Emails.Repositories/EHive.Emails.Repositories.csproj", "EHive.Emails.Repositories/"]
COPY ["EHive.Emails.Services/EHive.Emails.Services.csproj", "EHive.Emails.Services/"]
COPY ["EHive.Emails.Tests/EHive.Emails.Tests.csproj", "EHive.Emails.Tests/"]

RUN dotnet restore "EHive.Emails.Api/EHive.Emails.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Emails.Api"
RUN dotnet build "EHive.Emails.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Emails.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Emails.Api.dll"]
