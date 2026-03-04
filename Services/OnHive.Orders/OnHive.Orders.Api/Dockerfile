#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1300

 ENV DocsSettings__ServiceUrl=http://onhive-orders-api:1300
 ENV ProjectInfo__ProjectName='Orders'
 ENV ProjectInfo__ProjectDescription='OnHive Orders API'
 ENV EnvironmentSettings__AdditionalConfigs__0=onhive-orders-api
 ENV OrdersApiSettings__InvoiceRequestQueue=initialize_invoice

EXPOSE 1300

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Orders.Api/OnHive.Orders.Api.csproj", "OnHive.Orders.Api/"]
COPY ["OnHive.Orders.Domain/OnHive.Orders.Domain.csproj", "OnHive.Orders.Domain/"]
COPY ["OnHive.Orders.Repositories/OnHive.Orders.Repositories.csproj", "OnHive.Orders.Repositories/"]
COPY ["OnHive.Orders.Services/OnHive.Orders.Services.csproj", "OnHive.Orders.Services/"]
COPY ["OnHive.Orders.Tests/OnHive.Orders.Tests.csproj", "OnHive.Orders.Tests/"]

RUN dotnet restore "OnHive.Orders.Api/OnHive.Orders.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Orders.Api"
RUN dotnet build "OnHive.Orders.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Orders.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Orders.Api.dll"]
