#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1602
ENV PaymentsCieloApiSettings__PaymentsCieloAdminPermission=paymentsCielo_admin

ENV DocsSettings__ServiceUrl=http://ehive-paymentscielo-api:1601
ENV ProjectInfo__ProjectName='Cielo Payment API'
ENV ProjectInfo__ProjectDescription='EHive Cielo Payment API'
ENV EnvironmentSettings__AdditionalConfigs__0=ehive-payments-cielo

EXPOSE 1602

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.PaymentsCielo.Api/EHive.PaymentsCielo.Api.csproj", "EHive.PaymentsCielo.Api/"]
COPY ["EHive.PaymentsCielo.Domain/EHive.PaymentsCielo.Domain.csproj", "EHive.PaymentsCielo.Domain/"]
COPY ["EHive.PaymentsCielo.Repositories/EHive.PaymentsCielo.Repositories.csproj", "EHive.PaymentsCielo.Repositories/"]
COPY ["EHive.PaymentsCielo.Services/EHive.PaymentsCielo.Services.csproj", "EHive.PaymentsCielo.Services/"]
COPY ["EHive.PaymentsCielo.Tests/EHive.PaymentsCielo.Tests.csproj", "EHive.PaymentsCielo.Tests/"]

RUN dotnet restore "EHive.PaymentsCielo.Api/EHive.PaymentsCielo.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.PaymentsCielo.Api"
RUN dotnet build "EHive.PaymentsCielo.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.PaymentsCielo.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.PaymentsCielo.Api.dll"]
