# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG NUGET_USERNAME
WORKDIR /app

COPY ["nuget.config", "."]
COPY [".editorconfig", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["Shared/", "Shared/"]
COPY ["AuthService/", "AuthService/"]

RUN dotnet tool install --global dotnet-ef --version 10.0.12
ENV PATH="$PATH:/root/.dotnet/tools"

RUN --mount=type=secret,id=nuget_password \
    dotnet nuget update source github \
        --username "$NUGET_USERNAME" \
        --password "$(cat /run/secrets/nuget_password)" \
        --store-password-in-clear-text \
        --configfile /app/nuget.config \
  && dotnet restore "AuthService/src/AuthService.Web/AuthService.Web.csproj" \
  && dotnet ef migrations bundle \
        --project "AuthService/src/AuthService.Infrastructure.Postgres/AuthService.Infrastructure.Postgres.csproj" \
        --startup-project "AuthService/src/AuthService.Web/AuthService.Web.csproj" \
        --configuration "$BUILD_CONFIGURATION" \
        --output /app/efbundle \
  && rm -f /app/nuget.config

RUN dotnet build "AuthService/src/AuthService.Web/AuthService.Web.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build \
    --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AuthService/src/AuthService.Web/AuthService.Web.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM base AS migrations
WORKDIR /app
COPY --from=build /app/efbundle .
ENTRYPOINT ["./efbundle"]

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.Web.dll"]
