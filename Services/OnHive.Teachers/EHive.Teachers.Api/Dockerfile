#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1201

ENV TeachersApiSettings__TeachersAdminPermission=teachers_admin
ENV DocsSettings__ServiceUrl=http://ehive-teachers-api:1201
ENV ProjectInfo__ProjectName='Teachers API'
ENV ProjectInfo__ProjectDescription='OnHive Teachers API'

EXPOSE 1201

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Teachers.Api/EHive.Teachers.Api.csproj", "EHive.Teachers.Api/"]
COPY ["EHive.Teachers.Domain/EHive.Teachers.Domain.csproj", "EHive.Teachers.Domain/"]
COPY ["EHive.Teachers.Repositories/EHive.Teachers.Repositories.csproj", "EHive.Teachers.Repositories/"]
COPY ["EHive.Teachers.Services/EHive.Teachers.Services.csproj", "EHive.Teachers.Services/"]
COPY ["EHive.Teachers.Tests/EHive.Teachers.Tests.csproj", "EHive.Teachers.Tests/"]

RUN dotnet restore "EHive.Teachers.Api/EHive.Teachers.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Teachers.Api"
RUN dotnet build "EHive.Teachers.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Teachers.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Teachers.Api.dll"]
