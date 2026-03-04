#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1602
ENV PaymentsCieloApiSettings__PaymentsCieloAdminPermission=paymentsCielo_admin

ENV DocsSettings__ServiceUrl=http://onhive-paymentscielo-api:1601
ENV ProjectInfo__ProjectName='Cielo Payment API'
ENV ProjectInfo__ProjectDescription='OnHive Cielo Payment API'
ENV EnvironmentSettings__AdditionalConfigs__0=onhive-payments-cielo

EXPOSE 1602

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.PaymentsCielo.Api/OnHive.PaymentsCielo.Api.csproj", "OnHive.PaymentsCielo.Api/"]
COPY ["OnHive.PaymentsCielo.Domain/OnHive.PaymentsCielo.Domain.csproj", "OnHive.PaymentsCielo.Domain/"]
COPY ["OnHive.PaymentsCielo.Repositories/OnHive.PaymentsCielo.Repositories.csproj", "OnHive.PaymentsCielo.Repositories/"]
COPY ["OnHive.PaymentsCielo.Services/OnHive.PaymentsCielo.Services.csproj", "OnHive.PaymentsCielo.Services/"]
COPY ["OnHive.PaymentsCielo.Tests/OnHive.PaymentsCielo.Tests.csproj", "OnHive.PaymentsCielo.Tests/"]

RUN dotnet restore "OnHive.PaymentsCielo.Api/OnHive.PaymentsCielo.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.PaymentsCielo.Api"
RUN dotnet build "OnHive.PaymentsCielo.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.PaymentsCielo.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.PaymentsCielo.Api.dll"]
