#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1200

d
ENV CoursesApiSettings__CoursesAdminPermission=courses_admin
ENV DocsSettings__ServiceUrl=http://ehive-courses-api:1200
ENV ProjectInfo__ProjectName='Courses API'
ENV ProjectInfo__ProjectDescription='OnHive courses, lessons, disciplines and exams API'

ENV EnvironmentSettings__AdditionalConfigs__0=ehive-courses-api

EXPOSE 1200

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Courses.Api/EHive.Courses.Api.csproj", "EHive.Courses.Api/"]
COPY ["EHive.Courses.Domain/EHive.Courses.Domain.csproj", "EHive.Courses.Domain/"]
COPY ["EHive.Courses.Repositories/EHive.Courses.Repositories.csproj", "EHive.Courses.Repositories/"]
COPY ["EHive.Courses.Services/EHive.Courses.Services.csproj", "EHive.Courses.Services/"]
COPY ["EHive.Courses.Tests/EHive.Courses.Tests.csproj", "EHive.Courses.Tests/"]

RUN dotnet restore "EHive.Courses.Api/EHive.Courses.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Courses.Api"
RUN dotnet build "EHive.Courses.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Courses.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Courses.Api.dll"]
