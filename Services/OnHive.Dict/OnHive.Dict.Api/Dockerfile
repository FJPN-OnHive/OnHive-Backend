#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1006


ENV DocsSettings__ServiceUrl=http://onhive-dict-api:1006
ENV ProjectInfo__ProjectName='Values API'
ENV ProjectInfo__ProjectDescription='OnHive Values API'


EXPOSE 1006

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Dict.Api/OnHive.Dict.Api.csproj", "OnHive.Dict.Api/"]
COPY ["OnHive.Dict.Domain/OnHive.Dict.Domain.csproj", "OnHive.Dict.Domain/"]
COPY ["OnHive.Dict.Repositories/OnHive.Dict.Repositories.csproj", "OnHive.Dict.Repositories/"]
COPY ["OnHive.Dict.Services/OnHive.Dict.Services.csproj", "OnHive.Dict.Services/"]
COPY ["OnHive.Dict.Tests/OnHive.Dict.Tests.csproj", "OnHive.Dict.Tests/"]

RUN dotnet restore "OnHive.Dict.Api/OnHive.Dict.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Dict.Api"
RUN dotnet build "OnHive.Dict.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Dict.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Dict.Api.dll"]
