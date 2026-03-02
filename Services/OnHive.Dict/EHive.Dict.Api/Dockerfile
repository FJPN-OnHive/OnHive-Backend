#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1006


ENV DocsSettings__ServiceUrl=http://ehive-dict-api:1006
ENV ProjectInfo__ProjectName='Values API'
ENV ProjectInfo__ProjectDescription='EHive Values API'


EXPOSE 1006

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Dict.Api/EHive.Dict.Api.csproj", "EHive.Dict.Api/"]
COPY ["EHive.Dict.Domain/EHive.Dict.Domain.csproj", "EHive.Dict.Domain/"]
COPY ["EHive.Dict.Repositories/EHive.Dict.Repositories.csproj", "EHive.Dict.Repositories/"]
COPY ["EHive.Dict.Services/EHive.Dict.Services.csproj", "EHive.Dict.Services/"]
COPY ["EHive.Dict.Tests/EHive.Dict.Tests.csproj", "EHive.Dict.Tests/"]

RUN dotnet restore "EHive.Dict.Api/EHive.Dict.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Dict.Api"
RUN dotnet build "EHive.Dict.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Dict.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Dict.Api.dll"]
