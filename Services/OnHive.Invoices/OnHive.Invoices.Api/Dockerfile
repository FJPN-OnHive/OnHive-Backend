#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1700

ENV DocsSettings__ServiceUrl=http://onhive-invoices-api:1700
ENV ProjectInfo__ProjectName='Invoices'
ENV ProjectInfo__ProjectDescription='OnHive Invoices API'


EXPOSE 1700

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Invoices.Api/OnHive.Invoices.Api.csproj", "OnHive.Invoices.Api/"]
COPY ["OnHive.Invoices.Domain/OnHive.Invoices.Domain.csproj", "OnHive.Invoices.Domain/"]
COPY ["OnHive.Invoices.Repositories/OnHive.Invoices.Repositories.csproj", "OnHive.Invoices.Repositories/"]
COPY ["OnHive.Invoices.Services/OnHive.Invoices.Services.csproj", "OnHive.Invoices.Services/"]
COPY ["OnHive.Invoices.Tests/OnHive.Invoices.Tests.csproj", "OnHive.Invoices.Tests/"]

RUN dotnet restore "OnHive.Invoices.Api/OnHive.Invoices.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Invoices.Api"
RUN dotnet build "OnHive.Invoices.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Invoices.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Invoices.Api.dll"]
