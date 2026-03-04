#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1011

ENV DocsSettings__ServiceUrl=http://onhive-redirect-api:1011
ENV ProjectInfo__ProjectName='Redirects API'
ENV ProjectInfo__ProjectDescription='OnHive Redirects API'


EXPOSE 1011

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Redirects.Api/OnHive.Redirects.Api.csproj", "OnHive.Redirects.Api/"]
COPY ["OnHive.Redirects.Domain/OnHive.Redirects.Domain.csproj", "OnHive.Redirects.Domain/"]
COPY ["OnHive.Redirects.Repositories/OnHive.Redirects.Repositories.csproj", "OnHive.Redirects.Repositories/"]
COPY ["OnHive.Redirects.Services/OnHive.Redirects.Services.csproj", "OnHive.Redirects.Services/"]
COPY ["OnHive.Redirects.Tests/OnHive.Redirects.Tests.csproj", "OnHive.Redirects.Tests/"]

RUN dotnet restore "OnHive.Redirects.Api/OnHive.Redirects.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Redirects.Api"
RUN dotnet build "OnHive.Redirects.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Redirects.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Redirects.Api.dll"]
