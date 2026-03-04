#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1600

ENV PermissionsSettings__Rules__0__Endpoint=GetProvidersInfo
ENV PermissionsSettings__Rules__0__Permissions=payments_read
ENV PermissionsSettings__Rules__0__Features=''
ENV PermissionsSettings__Rules__0__AllowUnvalidatedEmail=true
ENV PermissionsSettings__Rules__1__Endpoint=Checkout
ENV PermissionsSettings__Rules__1__Permissions=payments_create
ENV PermissionsSettings__Rules__1__Features=''
ENV PermissionsSettings__Rules__1__AllowUnvalidatedEmail=true
ENV PermissionsSettings__Rules__2__Endpoint=CancelPayment
ENV PermissionsSettings__Rules__2__Permissions=payments_update
ENV PermissionsSettings__Rules__2__Features=''
ENV PermissionsSettings__Rules__3__Endpoint=GetReceiptById
ENV PermissionsSettings__Rules__3__Permissions=payments_read
ENV PermissionsSettings__Rules__3__Features=''
ENV PermissionsSettings__Rules__3__AllowUnvalidatedEmail=true
ENV PermissionsSettings__Rules__4__Endpoint=GetReceipts
ENV PermissionsSettings__Rules__4__Permissions=payments_read
ENV PermissionsSettings__Rules__4__Features=''
ENV PermissionsSettings__Rules__5__Endpoint=GetReceiptByOrder
ENV PermissionsSettings__Rules__5__Permissions=payments_read
ENV PermissionsSettings__Rules__5__Features=''
ENV PermissionsSettings__Rules__5__AllowUnvalidatedEmail=true
ENV PermissionsSettings__Rules__6__Endpoint=GetBankSlipSettingsById
ENV PermissionsSettings__Rules__6__Permissions=payments_admin
ENV PermissionsSettings__Rules__6__Features=''
ENV PermissionsSettings__Rules__7__Endpoint=GetBankSlipSettingses
ENV PermissionsSettings__Rules__7__Permissions=payments_admin
ENV PermissionsSettings__Rules__7__Features=''
ENV PermissionsSettings__Rules__8__Endpoint=CreateBankSlipSettings
ENV PermissionsSettings__Rules__8__Permissions=payments_admin
ENV PermissionsSettings__Rules__8__Features=''
ENV PermissionsSettings__Rules__9__Endpoint=UpdateBankSlipSettings
ENV PermissionsSettings__Rules__9__Permissions=payments_admin
ENV PermissionsSettings__Rules__9__Features=''
ENV PermissionsSettings__Rules__10__Endpoint=PatchBankSlipSettings
ENV PermissionsSettings__Rules__10__Permissions=payments_admin
ENV PermissionsSettings__Rules__10__Features=''

ENV PaymentsApiSettings__PaymentsAdminPermission=payments_admin

ENV DocsSettings__ServiceUrl=http://onhive-payments-api:1600
ENV ProjectInfo__ProjectName='Payment API'
ENV ProjectInfo__ProjectDescription='OnHive Payment API'
ENV EnvironmentSettings__AdditionalConfigs__0=onhive-payments-api


EXPOSE 1600

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["OnHive.Payments.Api/OnHive.Payments.Api.csproj", "OnHive.Payments.Api/"]
COPY ["OnHive.Payments.Domain/OnHive.Payments.Domain.csproj", "OnHive.Payments.Domain/"]
COPY ["OnHive.Payments.Repositories/OnHive.Payments.Repositories.csproj", "OnHive.Payments.Repositories/"]
COPY ["OnHive.Payments.Services/OnHive.Payments.Services.csproj", "OnHive.Payments.Services/"]
COPY ["OnHive.Payments.Tests/OnHive.Payments.Tests.csproj", "OnHive.Payments.Tests/"]

RUN dotnet restore "OnHive.Payments.Api/OnHive.Payments.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/OnHive.Payments.Api"
RUN dotnet build "OnHive.Payments.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnHive.Payments.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnHive.Payments.Api.dll"]
