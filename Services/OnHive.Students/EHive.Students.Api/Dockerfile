#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1202

ENV StudentsApiSettings__StudentsAdminPermission=students_admin
ENV DocsSettings__ServiceUrl=http://ehive-students-api:1202
ENV ProjectInfo__ProjectName='Students API'
ENV ProjectInfo__ProjectDescription='OnHive students API'
ENV EnvironmentSettings__AdditionalConfigs__0=ehive-students-api

EXPOSE 1202

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Students.Api/EHive.Students.Api.csproj", "EHive.Students.Api/"]
COPY ["EHive.Students.Domain/EHive.Students.Domain.csproj", "EHive.Students.Domain/"]
COPY ["EHive.Students.Repositories/EHive.Students.Repositories.csproj", "EHive.Students.Repositories/"]
COPY ["EHive.Students.Services/EHive.Students.Services.csproj", "EHive.Students.Services/"]
COPY ["EHive.Students.Tests/EHive.Students.Tests.csproj", "EHive.Students.Tests/"]

RUN dotnet restore "EHive.Students.Api/EHive.Students.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Students.Api"
RUN dotnet build "EHive.Students.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Students.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Students.Api.dll"]
