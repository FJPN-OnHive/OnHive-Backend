#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1300

 ENV DocsSettings__ServiceUrl=http://ehive-orders-api:1300
 ENV ProjectInfo__ProjectName='Orders'
 ENV ProjectInfo__ProjectDescription='EHive Orders API'
 ENV EnvironmentSettings__AdditionalConfigs__0=ehive-orders-api
 ENV OrdersApiSettings__InvoiceRequestQueue=initialize_invoice

EXPOSE 1300

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Orders.Api/EHive.Orders.Api.csproj", "EHive.Orders.Api/"]
COPY ["EHive.Orders.Domain/EHive.Orders.Domain.csproj", "EHive.Orders.Domain/"]
COPY ["EHive.Orders.Repositories/EHive.Orders.Repositories.csproj", "EHive.Orders.Repositories/"]
COPY ["EHive.Orders.Services/EHive.Orders.Services.csproj", "EHive.Orders.Services/"]
COPY ["EHive.Orders.Tests/EHive.Orders.Tests.csproj", "EHive.Orders.Tests/"]

RUN dotnet restore "EHive.Orders.Api/EHive.Orders.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Orders.Api"
RUN dotnet build "EHive.Orders.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Orders.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Orders.Api.dll"]
