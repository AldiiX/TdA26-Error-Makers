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
WORKDIR /app
COPY --from=publish /app/publish .

USER root

# instalace nginx + redis + "mysql" (mariadb) + nodejs
RUN apt-get update && apt-get install -y --no-install-recommends \
      curl ca-certificates nginx \
      redis-server \
      mariadb-server mariadb-client \
  && curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
  && apt-get install -y --no-install-recommends nodejs \
  && rm -rf /var/lib/apt/lists/*

# instalace minio + mc
RUN curl -fsSL https://dl.min.io/server/minio/release/linux-amd64/minio -o /usr/local/bin/minio \
  && chmod +x /usr/local/bin/minio \
  && curl -fsSL https://dl.min.io/client/mc/release/linux-amd64/mc -o /usr/local/bin/mc \
  && chmod +x /usr/local/bin/mc

# seed dump pro mysql
RUN mkdir -p /app/seed
COPY ["tda26.sql", "/app/seed/tda26.sql"]

# diry pro mysql socket + minio data
RUN mkdir -p /var/run/mysqld /data/minio \
  && chown -R mysql:mysql /var/run/mysqld /var/lib/mysql

# frontend build
COPY ["tda26.client/", "/app/client/"]
RUN chown -R $APP_UID:$APP_UID /app/client
WORKDIR /app/client
RUN npm config set cache /app/.npm
RUN npm install --unsafe-perm
RUN npm run build

# nginx konfigurace
COPY nginx.conf /etc/nginx/nginx.conf

# porty: app + redis + mysql + minio api + minio konzole
EXPOSE 80 6379 3306 9000 9001

WORKDIR /app
COPY --chmod=0755 start.sh .
CMD ["./start.sh"]