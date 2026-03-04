#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1201

ENV TeachersApiSettings__TeachersAdminPermission=teachers_admin
ENV DocsSettings__ServiceUrl=http://onhive-teachers-api:1201
ENV ProjectInfo__ProjectName='Teachers API'
ENV ProjectInfo__ProjectDescription='OnHive Teachers API'

EXPOSE 1201

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Teachers.Api/OnHive.Teachers.Api.csproj", "OnHive.Teachers.Api/"]
COPY ["OnHive.Teachers.Domain/OnHive.Teachers.Domain.csproj", "OnHive.Teachers.Domain/"]
COPY ["OnHive.Teachers.Repositories/OnHive.Teachers.Repositories.csproj", "OnHive.Teachers.Repositories/"]
COPY ["OnHive.Teachers.Services/OnHive.Teachers.Services.csproj", "OnHive.Teachers.Services/"]
COPY ["OnHive.Teachers.Tests/OnHive.Teachers.Tests.csproj", "OnHive.Teachers.Tests/"]

RUN dotnet restore "OnHive.Teachers.Api/OnHive.Teachers.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Teachers.Api"
RUN dotnet build "OnHive.Teachers.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Teachers.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Teachers.Api.dll"]
