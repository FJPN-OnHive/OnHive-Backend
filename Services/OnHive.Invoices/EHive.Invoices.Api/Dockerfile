#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1700

ENV DocsSettings__ServiceUrl=http://ehive-invoices-api:1700
ENV ProjectInfo__ProjectName='Invoices'
ENV ProjectInfo__ProjectDescription='EHive Invoices API'


EXPOSE 1700

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Invoices.Api/EHive.Invoices.Api.csproj", "EHive.Invoices.Api/"]
COPY ["EHive.Invoices.Domain/EHive.Invoices.Domain.csproj", "EHive.Invoices.Domain/"]
COPY ["EHive.Invoices.Repositories/EHive.Invoices.Repositories.csproj", "EHive.Invoices.Repositories/"]
COPY ["EHive.Invoices.Services/EHive.Invoices.Services.csproj", "EHive.Invoices.Services/"]
COPY ["EHive.Invoices.Tests/EHive.Invoices.Tests.csproj", "EHive.Invoices.Tests/"]

RUN dotnet restore "EHive.Invoices.Api/EHive.Invoices.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Invoices.Api"
RUN dotnet build "EHive.Invoices.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Invoices.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Invoices.Api.dll"]
