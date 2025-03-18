FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled-extra AS base
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:9.0-noble AS publish
WORKDIR /src

RUN apt-get update && \
    apt-get install gcc -y && \
    rm -rf /var/lib/apt/lists/*

COPY ["./EBCEYS.Container-AppStarter/", "./"]

ARG BUILD_CONFIGURATION=Release
RUN dotnet restore "./EBCEYS.Container-AppStarter.csproj"
RUN dotnet publish "./EBCEYS.Container-AppStarter.csproj" \ 
    -c $BUILD_CONFIGURATION \ 
    /p:PublishReadyToRun=true \
    -o /appstarter/publish \ 
    -r linux-x64

FROM base AS final
WORKDIR /appstarter

COPY --from=publish /appstarter/publish .
ENTRYPOINT ["/appstarter/EBCEYS.Container-AppStarter"]