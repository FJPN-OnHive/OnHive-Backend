#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1200

d
ENV CoursesApiSettings__CoursesAdminPermission=courses_admin
ENV DocsSettings__ServiceUrl=http://onhive-courses-api:1200
ENV ProjectInfo__ProjectName='Courses API'
ENV ProjectInfo__ProjectDescription='OnHive courses, lessons, disciplines and exams API'

ENV EnvironmentSettings__AdditionalConfigs__0=onhive-courses-api

EXPOSE 1200

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Courses.Api/OnHive.Courses.Api.csproj", "OnHive.Courses.Api/"]
COPY ["OnHive.Courses.Domain/OnHive.Courses.Domain.csproj", "OnHive.Courses.Domain/"]
COPY ["OnHive.Courses.Repositories/OnHive.Courses.Repositories.csproj", "OnHive.Courses.Repositories/"]
COPY ["OnHive.Courses.Services/OnHive.Courses.Services.csproj", "OnHive.Courses.Services/"]
COPY ["OnHive.Courses.Tests/OnHive.Courses.Tests.csproj", "OnHive.Courses.Tests/"]

RUN dotnet restore "OnHive.Courses.Api/OnHive.Courses.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Courses.Api"
RUN dotnet build "OnHive.Courses.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Courses.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Courses.Api.dll"]
