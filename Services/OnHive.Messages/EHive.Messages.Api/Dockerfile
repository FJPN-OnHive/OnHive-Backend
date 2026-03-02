#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:1902
ENV PermissionsSettings__Rules__0__Endpoint=GetMessages
ENV PermissionsSettings__Rules__0__Permissions=messages_admin
ENV PermissionsSettings__Rules__0__Features=''
ENV PermissionsSettings__Rules__1__Endpoint=GetMessageById
ENV PermissionsSettings__Rules__1__Permissions=messages_read
ENV PermissionsSettings__Rules__1__Features=''
ENV PermissionsSettings__Rules__2__Endpoint=CreateMessage
ENV PermissionsSettings__Rules__2__Permissions=messages_create
ENV PermissionsSettings__Rules__2__Features=''
ENV PermissionsSettings__Rules__3__Endpoint=UpdateMessage
ENV PermissionsSettings__Rules__3__Permissions=messages_update
ENV PermissionsSettings__Rules__3__Features=''
ENV PermissionsSettings__Rules__4__Endpoint=PatchMessage
ENV PermissionsSettings__Rules__4__Permissions=messages_update
ENV PermissionsSettings__Rules__4__Features=''
ENV PermissionsSettings__Rules__5__Endpoint=GetMessageListByUser
ENV PermissionsSettings__Rules__5__Permissions=messages_read
ENV PermissionsSettings__Rules__5__Features=''
ENV PermissionsSettings__Rules__6__Endpoint=GetMessageChannelById
ENV PermissionsSettings__Rules__6__Permissions=messageChannels_read
ENV PermissionsSettings__Rules__6__Features=''
ENV PermissionsSettings__Rules__7__Endpoint=GetMessageChannels
ENV PermissionsSettings__Rules__7__Permissions=messageChannels_read
ENV PermissionsSettings__Rules__7__Features=''
ENV PermissionsSettings__Rules__8__Endpoint=CreateMessageChannel
ENV PermissionsSettings__Rules__8__Permissions=messageChannels_create
ENV PermissionsSettings__Rules__8__Features=''
ENV PermissionsSettings__Rules__9__Endpoint=UpdateMessageChannel
ENV PermissionsSettings__Rules__9__Permissions=messageChannels_update
ENV PermissionsSettings__Rules__9__Features=''
ENV PermissionsSettings__Rules__10__Endpoint=DeleteMessageChannelById
ENV PermissionsSettings__Rules__10__Permissions=messageChannels_update
ENV PermissionsSettings__Rules__10__Features=''
ENV PermissionsSettings__Rules__11__Endpoint=UpdateUserMessage
ENV PermissionsSettings__Rules__11__Permissions=messages_read
ENV PermissionsSettings__Rules__11__Features=''
ENV PermissionsSettings__Rules__12__Endpoint=PatchUserMessage
ENV PermissionsSettings__Rules__12__Permissions=messages_read
ENV PermissionsSettings__Rules__12__Features=''

ENV MessagesApiSettings__MessagesAdminPermission=messages_admin

ENV DocsSettings__ServiceUrl=http://ehive-messages-api:1902
ENV ProjectInfo__ProjectName='Message API'
ENV ProjectInfo__ProjectDescription='EHive Message API'

ENV EnvironmentSettings__AdditionalConfigs__0=ehive-messages-api


EXPOSE 1902

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["packages/", "packages/"]
COPY ["EHive.Messages.Api/EHive.Messages.Api.csproj", "EHive.Messages.Api/"]
COPY ["EHive.Messages.Domain/EHive.Messages.Domain.csproj", "EHive.Messages.Domain/"]
COPY ["EHive.Messages.Repositories/EHive.Messages.Repositories.csproj", "EHive.Messages.Repositories/"]
COPY ["EHive.Messages.Services/EHive.Messages.Services.csproj", "EHive.Messages.Services/"]
COPY ["EHive.Messages.Tests/EHive.Messages.Tests.csproj", "EHive.Messages.Tests/"]

RUN dotnet restore "EHive.Messages.Api/EHive.Messages.Api.csproj" --source ./packages
COPY . .
WORKDIR "/src/EHive.Messages.Api"
RUN dotnet build "EHive.Messages.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EHive.Messages.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EHive.Messages.Api.dll"]
