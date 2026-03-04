#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:1003

ENV EnvironmentSettings__AdditionalConfigs__0=onhive-emails-api
ENV DocsSettings__ServiceUrl=http://onhive-emails-api:1003
ENV ProjectInfo__ProjectName='Emails Api'
ENV ProjectInfo__ProjectDescription='OnHive Emails Api'

EXPOSE 1003

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Emails.Api/OnHive.Emails.Api.csproj", "OnHive.Emails.Api/"]
COPY ["OnHive.Emails.Domain/OnHive.Emails.Domain.csproj", "OnHive.Emails.Domain/"]
COPY ["OnHive.Emails.Repositories/OnHive.Emails.Repositories.csproj", "OnHive.Emails.Repositories/"]
COPY ["OnHive.Emails.Services/OnHive.Emails.Services.csproj", "OnHive.Emails.Services/"]
COPY ["OnHive.Emails.Tests/OnHive.Emails.Tests.csproj", "OnHive.Emails.Tests/"]

RUN dotnet restore "OnHive.Emails.Api/OnHive.Emails.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Emails.Api"
RUN dotnet build "OnHive.Emails.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Emails.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Emails.Api.dll"]
