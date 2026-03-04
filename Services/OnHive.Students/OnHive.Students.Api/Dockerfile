#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1202

ENV StudentsApiSettings__StudentsAdminPermission=students_admin
ENV DocsSettings__ServiceUrl=http://onhive-students-api:1202
ENV ProjectInfo__ProjectName='Students API'
ENV ProjectInfo__ProjectDescription='OnHive students API'
ENV EnvironmentSettings__AdditionalConfigs__0=onhive-students-api

EXPOSE 1202

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Students.Api/OnHive.Students.Api.csproj", "OnHive.Students.Api/"]
COPY ["OnHive.Students.Domain/OnHive.Students.Domain.csproj", "OnHive.Students.Domain/"]
COPY ["OnHive.Students.Repositories/OnHive.Students.Repositories.csproj", "OnHive.Students.Repositories/"]
COPY ["OnHive.Students.Services/OnHive.Students.Services.csproj", "OnHive.Students.Services/"]
COPY ["OnHive.Students.Tests/OnHive.Students.Tests.csproj", "OnHive.Students.Tests/"]

RUN dotnet restore "OnHive.Students.Api/OnHive.Students.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Students.Api"
RUN dotnet build "OnHive.Students.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Students.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Students.Api.dll"]
