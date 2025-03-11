# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /appstarter

ENV ASPNETCORE_URLS=http://*:8080
ENV HEALTHCHECKS_STARTING_PORT=8080


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./EBCEYS.Container-AppStarter/EBCEYS.Container-AppStarter.csproj", "./EBCEYS.Container-AppStarter/"]
RUN dotnet restore "./EBCEYS.Container-AppStarter/EBCEYS.Container-AppStarter.csproj"
COPY . .
WORKDIR "./EBCEYS.Container-AppStarter"
RUN dotnet build "./EBCEYS.Container-AppStarter.csproj" -c $BUILD_CONFIGURATION -o /appstarter/build /p:PublishSingleFile=true --self-contained true -r linux-x64

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EBCEYS.Container-AppStarter.csproj" -c $BUILD_CONFIGURATION -o /appstarter/publish /p:UseAppHost=true /p:PublishSingleFile=true --self-contained true -r linux-x64

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /appstarter
COPY --from=publish /appstarter/publish .
ENTRYPOINT ["/appstarter/EBCEYS.Container-AppStarter"]