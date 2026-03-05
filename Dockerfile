ARG APP_UID=1000

# base stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# stage s node
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS with-node
RUN apt-get update && apt-get install -y --no-install-recommends curl ca-certificates \
  && curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
  && apt-get install -y --no-install-recommends nodejs \
  && rm -rf /var/lib/apt/lists/*

# build backendu
FROM with-node AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["tda26.Server/tda26.Server.csproj", "tda26.Server/"]
COPY ["tda26.client/tda26.client.esproj", "tda26.client/"]
RUN dotnet restore "./tda26.Server/tda26.Server.csproj"
COPY . .
WORKDIR "/src/tda26.Server"
RUN dotnet build "./tda26.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# publish backendu
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./tda26.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# final stage
FROM base AS final
ARG APP_UID=1000

WORKDIR /app
COPY --from=publish /app/publish .

USER root

# pokud je predany build secret BACKEND_ENV_B64, vytvori /app/.env uz pri buildu
RUN --mount=type=secret,id=BACKEND_ENV_B64,required=false \
    if [ ! -f /app/.env ] && [ -f /run/secrets/BACKEND_ENV_B64 ]; then \
      umask 077; \
      base64 -d /run/secrets/BACKEND_ENV_B64 > /app/.env; \
      chmod 600 /app/.env; \
    fi

# instalace nginx + nodejs
RUN apt-get update && apt-get install -y --no-install-recommends \
      curl ca-certificates nginx \
  && curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
  && apt-get install -y --no-install-recommends nodejs \
  && rm -rf /var/lib/apt/lists/*

# frontend build
COPY ["tda26.client/", "/app/client/"]
RUN chown -R ${APP_UID}:${APP_UID} /app/client
WORKDIR /app/client
RUN npm config set cache /app/.npm
RUN npm install --unsafe-perm
RUN npm run build

# nginx konfigurace
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

WORKDIR /app
COPY --chmod=0755 ./start.sh ./start.sh
CMD ["./start.sh"]